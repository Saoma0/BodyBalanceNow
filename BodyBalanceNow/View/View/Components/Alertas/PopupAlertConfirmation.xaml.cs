using CommunityToolkit.Maui.Views;

namespace TFGMultiplatform.Views.Components.Alertas;

public partial class PopupAlertConfirmation : Popup
{
	public PopupAlertConfirmation()
	{
		InitializeComponent();
	}

    private void OnCloseAlert(object sender, EventArgs e)
    {
		Close();
    }
}