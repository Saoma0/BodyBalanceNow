using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;
using System.Diagnostics;
using BodyBalanceNow.Services;
namespace BodyBalanceNow.View.ViewAndroid;

public partial class ListaSeriesPopUpAndroid : Popup
{
    private DatabaseServiceAndroid db;
    private int _idEjercicioRutina, _idRutina;

    public ListaSeriesPopUpAndroid(int idEjercicioRutina, int idRutina)
    {
        InitializeComponent();
        db = new DatabaseServiceAndroid();
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
            int serie = Convert.ToInt32(SerieEntry.Text);
            int peso = Convert.ToInt32(PesoEntry.Text);
            int repeticiones = Convert.ToInt32(RepeticionesEntry.Text);
            int idejerciciorutina = await db.ObtenerIdEjercicioEnRutinaAsync(_idRutina, _idEjercicioRutina);
            db.InsertarSerieRealizada(idejerciciorutina, serie, repeticiones, peso);
            Debug.WriteLine("Serie agregada");
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