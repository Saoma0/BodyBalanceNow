using CommunityToolkit.Maui.Views;
namespace BodyBalanceNow.View.ViewWindows;
/**
 * Se cambian los enlaces a enlaces embebidos para que solo cargue el video directo, no la página de YouTube
 * 
 * Se utilizan los hilos para evitar los errores de la UI, forzando que se ejecute de forma correcta
 */
public partial class ListaEjerciciosPopUp : Popup
{
    public ListaEjerciciosPopUp(Ejercicios ejercicio)
    {
        InitializeComponent();
        EstablecerDetallesEjercicio(ejercicio);
    }

    private void EstablecerDetallesEjercicio(Ejercicios ejercicio)
    {
        NombreEjercicioLabel.Text = ejercicio.NombreEjercicio;
        DificultadValueSpan.Text = ejercicio.NivelDificultad;
        GrupoValueSpan.Text = ejercicio.GrupoMuscular;
        EquipamientoValueSpan.Text = ejercicio.Equipamiento;
        AgarreValueSpan.Text = ejercicio.Agarre;
        ZonaCorporalValueSpan.Text = ejercicio.ZonaCorporal;
        LateralidadValueSpan.Text = ejercicio.Lateralidad;

        ConfigurarBotonVideo(ejercicio.UrlDemostracionCorta, VideoCortoBtn, NoVideoCortoLabel);
        ConfigurarBotonVideo(ejercicio.UrlDemostracionLarga, VideoLargoBtn, NoVideoLargoLabel);
    }

    private void ConfigurarBotonVideo(string urlVideo, Button botonVideo, Label etiquetaNoVideo)
    {
        if (string.IsNullOrEmpty(urlVideo))
        {
            botonVideo.IsVisible = false;
            etiquetaNoVideo.IsVisible = true;
        }
        else
        {
            botonVideo.IsVisible = true;
            etiquetaNoVideo.IsVisible = false;
            botonVideo.Clicked += (s, e) =>
            {
                ContenedorInfo.IsVisible = false;
                ContenedorVideoFrame.IsVisible = true;
                WebViewPantallaCompleta.Source = ObtenerUrlEmbebido(urlVideo);
            };
        }
    }

    private string ObtenerUrlEmbebido(string url)
    {
        if (url.Contains("watch?v="))
        {
            return url.Replace("watch?v=", "embed/");
        }
        else if (url.Contains("youtu.be/"))
        {
            return url.Replace("youtu.be/", "www.youtube.com/embed/");
        }
        return url; // Si no es un enlace de YouTube, lo retornamos tal cual
    }

    private void AlCerrarClicked(object sender, EventArgs e)
    {
        CerrarWebView();
        Close();
    }

    private void AlCerrarVideoClicked(object sender, EventArgs e)
    {
        ContenedorVideoFrame.IsVisible = false;
        CerrarWebView();
        
        ContenedorInfo.IsVisible = true;
    }

    private void CerrarWebView()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            WebViewPantallaCompleta.Source = "about:blank"; // Carga una página en blanco para asegurarse de que cierre cualquier reproduccion activa

        });
    }
}
