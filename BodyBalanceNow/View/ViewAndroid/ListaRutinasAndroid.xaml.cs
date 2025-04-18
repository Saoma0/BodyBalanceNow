using System.Collections.ObjectModel;
using System.Diagnostics;
using BodyBalanceNow.Services;
using CommunityToolkit.Maui.Views;

namespace BodyBalanceNow.View.ViewAndroid;

public partial class ListaRutinasAndroid : ContentPage
{
    DatabaseServiceAndroid db;
    private int idUsuarioActual = Preferences.Get("current_user_id", -1);
    private bool usuarioAutenticado = false;

    public ListaRutinasAndroid()
    {
        InitializeComponent();
        db = new DatabaseServiceAndroid();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        usuarioAutenticado = Authenticacion.UsuarioHaIniciadoSesion();

        if (!usuarioAutenticado || idUsuarioActual == -1)
        {
            await DisplayAlert("Atención", "Debes iniciar sesión para ver tus rutinas.", "Cerrar");
            await Navigation.PopAsync(); // opcional, si quieres salir de esta vista
            return;
        }

        cargarRutinas();
    }

    public void cargarRutinas()
    {
        var lista = db.GetRutinasPorUsuario(idUsuarioActual);
        listaRutinasUsuario.ItemsSource = lista;
    }

    private async void OnPlayButtonClicked(object sender, EventArgs e)
    {
        var imageButton = sender as ImageButton;
        if (imageButton == null) return;

        var rutina = imageButton.BindingContext as Rutina;
        if (rutina == null)
        {
            Debug.WriteLine("No se pudo encontrar la rutina");
            return;
        }

        // Declaramos rutinaID una sola vez aquí, fuera del switch
        int rutinaID = rutina.ID;

        string action = await DisplayActionSheet("Opciones", "Cancelar", null, "Eliminar", "Modificar", "Ver Detalles");

        switch (action)
        {
            case "Eliminar":
                bool confirmar = await DisplayAlert("Eliminar rutina",
                    $"¿Estás seguro de que quieres eliminar la rutina \"{rutina.NombreRutina}\"?", "Sí", "Cancelar");

                if (confirmar)
                {
                    await db.EliminarRutinaAsync(rutinaID);
                    cargarRutinas();
                }
                break;

            case "Modificar":
                string nombreRutina = await DisplayPromptAsync("Nuevo Nombre", "Escribe el nuevo nombre de la rutina", "Guardar", "Cancelar", placeholder: "...", maxLength: 100);
                if (!string.IsNullOrEmpty(nombreRutina))
                {
                    await db.EditarNombreRutinaAsync(rutinaID, nombreRutina);
                    cargarRutinas();
                }
                break;

            case "Ver Detalles":
                await Navigation.PushAsync(new ListaEjerciciosAndroid(rutinaID));
                break;
        }
    }
}