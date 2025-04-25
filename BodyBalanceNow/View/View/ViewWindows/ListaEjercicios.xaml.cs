using CommunityToolkit.Maui.Views;
using System.Diagnostics;
using BodyBalanceNow.Services;
namespace BodyBalanceNow.View.ViewWindows;

public partial class ListaEjercicios : ContentPage
{

    DatabaseService db;
    int _idRutina;
	public ListaEjercicios(int idRutina)
	{
		InitializeComponent();
        db = new DatabaseService();
        _idRutina = idRutina;
	}

    protected override async void OnAppearing()
    {
        base.OnAppearing();


        cargarEjercicios();

    }

    public void cargarEjercicios()
    {
        var lista = db.GetEjerciciosPorRutina(_idRutina);
        listaEjerciciosRutina.ItemsSource = lista;
    }

    private void OnVerEjercicioClicked(object sender, EventArgs e)
    {
        var imagebutton = sender as ImageButton;
        if (imagebutton == null) return;

        var ejercicio = imagebutton.BindingContext as EjerciciosRutina;
        if (ejercicio == null)
        {
            Debug.WriteLine("No se pudo encontrar el ejercicio");
            return;
        }

        int idEjercicioRutina = ejercicio.ID; // Usar el ID del objeto EjerciciosRutina
        Debug.WriteLine($"Ejercicio ID: {ejercicio.ID}");   
        var popup = new ListaSeriesPopUp(idEjercicioRutina, _idRutina); // Pasar el ID de ejercicio al popup
        this.ShowPopup(popup);
    }


    private async void OnEliminarEjercicioClicked(object sender, EventArgs e)
    {
        var imagebutton = sender as ImageButton;
        if (imagebutton == null) return;

        var ejercicio = imagebutton.BindingContext as EjerciciosRutina;
        if (ejercicio == null)
        {
            Debug.WriteLine("No se pudo encontrar el ejercicio");
            return;
        }

        bool confirmar = await DisplayAlert("Eliminar ejercicio",
            $"¿Estás seguro de que quieres eliminar el ejercicio \"{ejercicio.NombreEjercicio}\"?", "Sí", "Cancelar");

        if (!confirmar) return;

        int idEjercicioRutina = ejercicio.ID;
        int idEjRut = await db.ObtenerIdEjercicioEnRutinaAsync(_idRutina, idEjercicioRutina);

        Debug.WriteLine($"ID del ejercicio para eliminar: {idEjercicioRutina}");

        await db.EliminarEjercicioEnRutinaAsync(idEjRut);

        // Notificación (opcional)
        await DisplayAlert("Eliminado", "El ejercicio ha sido eliminado correctamente.", "OK");

        // Refrescar la lista
        cargarEjercicios();
    }

    private async void OnMasInformacionClicked(object sender, EventArgs e)
    {
        var imagebutton = sender as ImageButton;
        if (imagebutton == null) return;

        var ejercicioRutina = imagebutton.BindingContext as EjerciciosRutina;
        if (ejercicioRutina == null)
        {
            Debug.WriteLine("No se pudo encontrar el ejercicio");
            return;
        }

        // Llamada al método asíncrono para obtener el ejercicio completo
        var ejercicio = await db.ObtenerEjercicioPorIdAsync(ejercicioRutina.ID);

        if (ejercicio == null)
        {
            DisplayAlert("Error", "No se encontró el ejercicio.", "OK");
            return;
        }

        // Mostrar el popup con la información del ejercicio
        var popup = new ListaEjerciciosPopUp(ejercicio);
        this.ShowPopup(popup);
    }


}