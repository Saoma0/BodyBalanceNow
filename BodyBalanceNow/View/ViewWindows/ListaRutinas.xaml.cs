using System.Collections.ObjectModel;
using System.Diagnostics;
using BodyBalanceNow.Services;

namespace BodyBalanceNow.View.ViewWindows;

public partial class ListaRutinas : ContentPage
{
    DatabaseService db;
    private int idUsuarioActual = Preferences.Get("current_user_id", -1);
    private bool usuarioAutenticado = false;

    public ListaRutinas()
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

    private async void OnEditarClicked(object sender, EventArgs e)
    {
        string nombreRutina = await DisplayPromptAsync("Nuevo Nombre", "Escribe el nuevo nombre de la rutina", "Guardar", "Cancelar", placeholder: "...", maxLength: 100);
        var imagebutton = sender as ImageButton;
        if (imagebutton == null) return;

        var rutina = imagebutton.BindingContext as Rutina;
        if (rutina == null)
        {
            Debug.WriteLine("No se pudo encontrar la rutina");
            return;
        }

        int rutinaID = rutina.ID;

        await db.EditarNombreRutinaAsync(rutinaID, nombreRutina);
        cargarRutinas();
    }

    private async void OnEliminarClicked(object sender, EventArgs e)
    {
        var imagebutton = sender as ImageButton;
        if (imagebutton == null) return;

        var rutina = imagebutton.BindingContext as Rutina;
        if (rutina == null)
        {
            Debug.WriteLine("No se pudo encontrar la rutina");
            return;
        }

        bool confirmar = await DisplayAlert("Eliminar rutina",
            $"¿Estás seguro de que quieres eliminar la rutina \"{rutina.NombreRutina}\"?", "Sí", "Cancelar");

        if (!confirmar) return;

        int rutinaID = rutina.ID;
        await db.EliminarRutinaAsync(rutinaID);
        cargarRutinas();
    }

    private async void OnVerRutinaClicked(object sender, EventArgs e)
    {
        var imagebutton = sender as ImageButton;
        if (imagebutton == null) return;

        var rutina = imagebutton.BindingContext as Rutina;
        if (rutina == null)
        {
            Debug.WriteLine("No se pudo encontrar la rutina");
            return;
        }

        int rutinaID = rutina.ID;

        // Aquí navegas a la nueva página con el ID de la rutina
        await Navigation.PushAsync(new ListaEjercicios(rutinaID));
    }
}
