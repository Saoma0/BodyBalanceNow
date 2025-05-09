using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using BodyBalanceNow.View.ViewAndroid;
using BodyBalanceNow.View.ViewWindows;

namespace BodyBalanceNow;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

#if ANDROID
        InicioTab.ContentTemplate = new DataTemplate(typeof(MainPageAndroid));
        PesoTab.ContentTemplate = new DataTemplate(typeof(BoxPesoAndroid));
        TiempoTab.ContentTemplate = new DataTemplate(typeof(TiempoAndroid));
        SaludTab.ContentTemplate = new DataTemplate(typeof(BoxSaludAndroid));
        EntrenamientoTab.ContentTemplate = new DataTemplate(typeof(BoxEntrenamientoAndroid));
        estadisticasTab.ContentTemplate = new DataTemplate(typeof(StatisticsPageAndroid));

        Routing.RegisterRoute("RegisterPage", typeof(RegisterPageAndroid));
#elif WINDOWS
        InicioTab.ContentTemplate = new DataTemplate(typeof(MainPageWindows));
        PesoTab.ContentTemplate = new DataTemplate(typeof(BoxPeso));
        TiempoTab.ContentTemplate = new DataTemplate(typeof(CronometroTemporizador));
        SaludTab.ContentTemplate = new DataTemplate(typeof(BoxSalud));
        EntrenamientoTab.ContentTemplate = new DataTemplate(typeof(BoxEntrenamiento));
        estadisticasTab.ContentTemplate = new DataTemplate(typeof(StatisticsPage));


        Routing.RegisterRoute("RegisterPage", typeof(RegisterPage));

#else

#endif
    }

}