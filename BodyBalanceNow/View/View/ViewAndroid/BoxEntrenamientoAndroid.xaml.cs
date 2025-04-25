namespace BodyBalanceNow.View.ViewAndroid;

public partial class BoxEntrenamientoAndroid : ContentPage
{
	public BoxEntrenamientoAndroid()
	{
		InitializeComponent();
	}



    private async void IrACrearNuevaRutina_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new NuevaRutinaAndroid());
    }

    private async void IrAVerRutinasExistentes_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ListaRutinasAndroid());
    }
}