using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;
using System.Diagnostics;
using BodyBalanceNow.Services;
using BodyBalanceNow.View.Components;
namespace BodyBalanceNow.View.ViewWindows;

public partial class ListaSeriesPopUp : Popup
{
    private DatabaseService db;
    private int _idEjercicioRutina, _idRutina;

    public ListaSeriesPopUp(int idEjercicioRutina, int idRutina)
    {
        InitializeComponent();
        db = new DatabaseService();
        _idEjercicioRutina = idEjercicioRutina;
        _idRutina = idRutina;
        
       
        cargarDatosSeries();
        cargarSeriesGuardadas();
    }

    private  void cargarDatosSeries()
    {
        Debug.WriteLine($"ID EjercicioRutina en cargarDatosSeries: {_idEjercicioRutina} {_idRutina}");

        var detallesEjercicio =  db.ObtenerDetallesRutinaYEjercicio(_idRutina, _idEjercicioRutina);
        if (detallesEjercicio != null)
        {
            NombreEjercicioLabel.Text = detallesEjercicio.NombreEjercicio ?? "No disponible";
            NombreRutinaLabel.Text = detallesEjercicio.NombreRutina ?? "No disponible";
            GrupoMuscularLabel.Text = detallesEjercicio.GrupoMuscular ?? "No disponible";
        }
        else
        {
            Debug.WriteLine("No se encontraron los detalles del ejercicio.");
        }
    }

    private async void cargarSeriesGuardadas()
    {
        int idejerciciorutina = await db.ObtenerIdEjercicioEnRutinaAsync(_idRutina, _idEjercicioRutina);
        var series = await db.ObtenerSeriesPorEjercicioAsync(idejerciciorutina);
        ListaSeries.ItemsSource = series;
    }

    private void OnShowNewSeriesForm(object sender, EventArgs e)
    {
        FormularioSeries.IsVisible = true;
    }

    private async void OnSaveNewSeries(object sender, EventArgs e)
    {
        try
        {
            // Validar que los campos no estén vacíos
            if (string.IsNullOrWhiteSpace(SerieEntry.Text) ||
                string.IsNullOrWhiteSpace(PesoEntry.Text) ||
                string.IsNullOrWhiteSpace(RepeticionesEntry.Text))
            {
                var popup = new CustomPopup("Rellene todos los campos");
                Application.Current.MainPage.ShowPopup(popup);
                return;
            }

            int serie = Convert.ToInt32(SerieEntry.Text);
            int peso = Convert.ToInt32(PesoEntry.Text);
            int repeticiones = Convert.ToInt32(RepeticionesEntry.Text);

            int idejerciciorutina = await db.ObtenerIdEjercicioEnRutinaAsync(_idRutina, _idEjercicioRutina);
            db.InsertarSerieRealizada(idejerciciorutina, serie, repeticiones, peso);
            Debug.WriteLine("Serie agregada");

            var popup2 = new CustomPopup("Serie agregada");
            Application.Current.MainPage.ShowPopup(popup2);

            var seriesActualizadas = await db.ObtenerSeriesPorEjercicioAsync(idejerciciorutina);
            ListaSeries.ItemsSource = seriesActualizadas;

            FormularioSeries.IsVisible = false;

            SerieEntry.Text = "";
            PesoEntry.Text = "";
            RepeticionesEntry.Text = "";
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.ToString());
        }
    }
    private void OnCloseWindow(object sender, EventArgs e)
    {
        Close();
    }
}