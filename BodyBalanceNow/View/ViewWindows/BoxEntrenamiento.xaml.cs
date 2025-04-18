namespace BodyBalanceNow.View.ViewWindows;

public partial class BoxEntrenamiento : ContentPage
{
	public BoxEntrenamiento()
	{
		InitializeComponent();
	}



    private async void IrACrearNuevaRutina_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new NuevaRutina());
    }

    private async void IrAVerRutinasExistentes_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ListaRutinas());
    }
}