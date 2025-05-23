using System.Collections.ObjectModel;
using System.Diagnostics;
using BodyBalanceNow.Services;
using CommunityToolkit.Maui.Views;
using BodyBalanceNow.View.Components;
namespace BodyBalanceNow.View.ViewAndroid;

public partial class ListaRutinasAndroid : ContentPage
{
    DatabaseService db;
    private int idUsuarioActual = Preferences.Get("current_user_id", -1);
    private bool usuarioAutenticado = false;

    public ListaRutinasAndroid()
    {
        InitializeComponent();
        db = new DatabaseService();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        usuarioAutenticado = Authenticacion.UsuarioHaIniciadoSesion();

        if (!usuarioAutenticado || idUsuarioActual == -1)
        {
            var popup = new CustomPopup("Debes iniciar sesión para ver tus rutinas");
            this.ShowPopup(popup);
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
                var popupC = new ConfirmPopup($"¿Estás seguro de que quieres eliminar la rutina \"{rutina.NombreRutina}\"?");
                var confirmar = await this.ShowPopupAsync(popupC) as bool?;

                if (confirmar != true) return;

                await db.EliminarRutinaAsync(rutina.ID);
                cargarRutinas();
                break;

            case "Modificar":
                var popup = new PromptPopup("Escribe el nuevo nombre de la rutina", "...");
                string nombreRutina = await this.ShowPopupAsync(popup) as string;

                // Validación de entrada vacía o solo espacios
                if (string.IsNullOrWhiteSpace(nombreRutina))
                {
                    var popup2 = new CustomPopup("El nombre no puede estar vacío");
                    this.ShowPopup(popup2);
                    break;
                }

                var popup3 = new CustomPopup("El nombre ha sido modificado");
                this.ShowPopup(popup3);

                await db.EditarNombreRutinaAsync(rutinaID, nombreRutina.Trim());
                cargarRutinas();
                break;



            case "Ver Detalles":
                await Navigation.PushAsync(new ListaEjerciciosAndroid(rutinaID));
                break;
        }
    }
}