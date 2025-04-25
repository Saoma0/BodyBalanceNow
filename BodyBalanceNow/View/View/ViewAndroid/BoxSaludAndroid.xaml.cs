namespace BodyBalanceNow.View.ViewAndroid;

public partial class BoxSaludAndroid : ContentPage
{
	public BoxSaludAndroid()
	{
		InitializeComponent();
	}


    private async void IrARegistroAguaAndroid_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new BodyBalanceNow.View.ViewAndroid.RegistroAguaAndroid());
    }

    private async void IrARegistroSuenoAndroid_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new BodyBalanceNow.View.ViewAndroid.RegistroSuenoAndroid());
    }

    private async void IrARegistroEstresAndroid_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new BodyBalanceNow.View.ViewAndroid.RegistroEstresAndroid());
    }
}