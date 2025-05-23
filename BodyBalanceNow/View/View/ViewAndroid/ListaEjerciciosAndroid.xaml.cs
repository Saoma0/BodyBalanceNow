using CommunityToolkit.Maui.Views;
using System.Diagnostics;
using BodyBalanceNow.Services;
using BodyBalanceNow.View.Components;
namespace BodyBalanceNow.View.ViewAndroid;

public partial class ListaEjerciciosAndroid : ContentPage
{
    DatabaseService db;
    int _idRutina;

    public ListaEjerciciosAndroid(int idRutina)
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
                {
                    var popup = new ConfirmPopup($"¿Estás seguro de que quieres eliminar el ejercicio \"{ejercicio.NombreEjercicio}\"?");
                    var confirmar = await this.ShowPopupAsync(popup) as bool?;

                    if (confirmar != true) break;

                    int idEjRut = await db.ObtenerIdEjercicioEnRutinaAsync(_idRutina, idEjercicioRutina);
                    Debug.WriteLine($"ID del ejercicio para eliminar: {idEjercicioRutina}");
                    await db.EliminarEjercicioEnRutinaAsync(idEjRut);

                    var popup2 = new CustomPopup("El ejercicio ha sido eliminado correctamente");
                    this.ShowPopup(popup2);
                    cargarEjercicios();
                    break;
                }


            case "Ver Detalles":
                var ejercicioCompleto = await db.ObtenerEjercicioPorIdAsync(idEjercicioRutina);
                if (ejercicioCompleto == null)
                {
                    var popup = new CustomPopup("No se encontró el ejercicio");
                    this.ShowPopup(popup);
                    return;
                }
                var popupEditar = new ListaEjerciciosPopUpAndroid(ejercicioCompleto);
                this.ShowPopup(popupEditar);
                break;
        }
    }
}