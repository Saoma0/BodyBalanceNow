using System.Collections.ObjectModel;
using System.Diagnostics;
using BodyBalanceNow.Services;
using BodyBalanceNow.View.Components;
using CommunityToolkit.Maui.Views;
using BodyBalanceNow.View.Components;
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
            var popup2 = new CustomPopup("Debes iniciar sesión para ver tus rutinas");
            this.ShowPopup(popup2);

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
        var popup = new PromptPopup("Escribe el nuevo nombre de la rutina", "...");
        string nombreRutina = await this.ShowPopupAsync(popup) as string;

        // Validación de entrada vacía o solo espacios
        if (string.IsNullOrWhiteSpace(nombreRutina))
        {
            var popup2 = new CustomPopup("El nombre no puede estar vacío");
            this.ShowPopup(popup2);
            return;

        }
        else
        {
            var popup3 = new CustomPopup("El nombre ha sido modificado");
            this.ShowPopup(popup3);
        }

            var imagebutton = sender as ImageButton;
        if (imagebutton == null) return;

        var rutina = imagebutton.BindingContext as Rutina;
        if (rutina == null)
        {
            Debug.WriteLine("No se pudo encontrar la rutina");
            return;
        }

        int rutinaID = rutina.ID;

        await db.EditarNombreRutinaAsync(rutinaID, nombreRutina.Trim());
        cargarRutinas();
    }


    private async void OnEliminarClicked(object sender, EventArgs e)
    {
        if (sender is not ImageButton imageButton) return;
        if (imageButton.BindingContext is not Rutina rutina)
        {
            Debug.WriteLine("No se pudo encontrar la rutina");
            return;
        }

        var popup = new ConfirmPopup($"¿Estás seguro de que quieres eliminar la rutina \"{rutina.NombreRutina}\"?");
        var confirmar = await this.ShowPopupAsync(popup) as bool?;

        if (confirmar != true) return;

        await db.EliminarRutinaAsync(rutina.ID);
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
