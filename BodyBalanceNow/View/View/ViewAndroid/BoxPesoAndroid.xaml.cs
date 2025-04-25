namespace BodyBalanceNow.View.ViewAndroid;

public partial class BoxPesoAndroid : ContentPage
{
	public BoxPesoAndroid()
	{
		InitializeComponent();
	}

    private async void IrARegistroIMCAndroid_Clicked(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new BodyBalanceNow.View.ViewAndroid.CalculadoraIMCAndroid());
    }

    private async void IrARegistroPesoAndroid_Clicked(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new BodyBalanceNow.View.ViewAndroid.RegistroPesoAndroid());
    }
}