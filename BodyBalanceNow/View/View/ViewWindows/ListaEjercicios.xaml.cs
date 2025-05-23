using CommunityToolkit.Maui.Views;
using System.Diagnostics;
using BodyBalanceNow.Services;
using Microsoft.Maui.Controls;
using BodyBalanceNow.View.Components;
using Org.BouncyCastle.Tls;
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

        int idEjercicioRutina = ejercicio.ID; 
        Debug.WriteLine($"Ejercicio ID: {ejercicio.ID}");   
        var popup = new ListaSeriesPopUp(idEjercicioRutina, _idRutina);
        this.ShowPopup(popup);
    }


    private async void OnEliminarEjercicioClicked(object sender, EventArgs e)
    {
        if (sender is not ImageButton imageButton) return;
        if (imageButton.BindingContext is not EjerciciosRutina ejercicio)
        {
            Debug.WriteLine("No se pudo encontrar el ejercicio");
            return;
        }

        var popup = new ConfirmPopup($"¿Estás seguro de que quieres eliminar el ejercicio \"{ejercicio.NombreEjercicio}\"?");
        var confirmar = await this.ShowPopupAsync(popup) as bool?;

        if (confirmar != true) return;

        int idEjercicioRutina = ejercicio.ID;
        int idEjRut = await db.ObtenerIdEjercicioEnRutinaAsync(_idRutina, idEjercicioRutina);

        Debug.WriteLine($"ID del ejercicio para eliminar: {idEjRut}");

        await db.EliminarEjercicioEnRutinaAsync(idEjRut);

        var popup2 = new CustomPopup("El ejercicio ha sido eliminado correctamente");
        this.ShowPopup(popup2);
        cargarEjercicios(); // Refresca la lista
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
            var popup2 = new CustomPopup("No se encontró el ejercicio");
            this.ShowPopup(popup2); ;
            return;
        }

        // Mostrar el popup con la información del ejercicio
        var popup = new ListaEjerciciosPopUp(ejercicio);
        this.ShowPopup(popup);
    }


}