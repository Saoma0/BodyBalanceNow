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
        IMCTab.ContentTemplate = new DataTemplate(typeof(CalculadoraIMCAndroid));
        TiempoTab.ContentTemplate = new DataTemplate(typeof(TiempoAndroid));
        LoginTab.ContentTemplate = new DataTemplate(typeof(LoginPageAndroid));
        RegistroTab.ContentTemplate = new DataTemplate(typeof(RegisterPageAndroid));
#elif WINDOWS
        InicioTab.ContentTemplate = new DataTemplate(typeof(MainPageWindows));
        IMCTab.ContentTemplate = new DataTemplate(typeof(CalculadoraIMC));
        TiempoTab.ContentTemplate = new DataTemplate(typeof(CronometroTemporizador));
        LoginTab.ContentTemplate = new DataTemplate(typeof(LoginPage));
        RegistroTab.ContentTemplate = new DataTemplate(typeof(RegisterPage));
        
#else

#endif
    }

}