using CommunityToolkit.Maui.Views;
using System.Diagnostics;
using BodyBalanceNow.Services;

namespace BodyBalanceNow.View.ViewAndroid;

public partial class ListaEjerciciosAndroid : ContentPage
{
    DatabaseServiceAndroid db;
    int _idRutina;

    public ListaEjerciciosAndroid(int idRutina)
    {
        InitializeComponent();
        db = new DatabaseServiceAndroid();
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

    private async void OnPlayButtonClicked(object sender, EventArgs e)
    {
        var imageButton = sender as ImageButton;
        if (imageButton == null) return;

        var ejercicio = imageButton.BindingContext as EjerciciosRutina;
        if (ejercicio == null)
        {
            Debug.WriteLine("No se pudo encontrar el ejercicio");
            return;
        }

        int idEjercicioRutina = ejercicio.ID;

        string action = await DisplayActionSheet("Opciones", "Cancelar", null, "Series", "Eliminar", "Ver Detalles");

        switch (action)
        {
            case "Series":
                Debug.WriteLine($"Ejercicio ID: {idEjercicioRutina}");
                var popupVer = new ListaSeriesPopUpAndroid(idEjercicioRutina, _idRutina);
                this.ShowPopup(popupVer);
                break;

            case "Eliminar":
                bool confirmar = await DisplayAlert("Eliminar ejercicio",
                    $"¿Estás seguro de que quieres eliminar el ejercicio \"{ejercicio.NombreEjercicio}\"?", "Sí", "Cancelar");

                if (confirmar)
                {
                    int idEjRut = await db.ObtenerIdEjercicioEnRutinaAsync(_idRutina, idEjercicioRutina);
                    Debug.WriteLine($"ID del ejercicio para eliminar: {idEjercicioRutina}");
                    await db.EliminarEjercicioEnRutinaAsync(idEjRut);
                    await DisplayAlert("Eliminado", "El ejercicio ha sido eliminado correctamente.", "OK");
                    cargarEjercicios();
                }
                break;

            case "Ver Detalles":
                var ejercicioCompleto = await db.ObtenerEjercicioPorIdAsync(idEjercicioRutina);
                if (ejercicioCompleto == null)
                {
                    await DisplayAlert("Error", "No se encontró el ejercicio.", "OK");
                    return;
                }
                var popupEditar = new ListaEjerciciosPopUpAndroid(ejercicioCompleto);
                this.ShowPopup(popupEditar);
                break;
        }
    }
}