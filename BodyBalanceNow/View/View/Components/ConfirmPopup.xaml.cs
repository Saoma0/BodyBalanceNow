using CommunityToolkit.Maui.Views;

namespace BodyBalanceNow.View.Components;

public partial class ConfirmPopup : Popup
{
    public ConfirmPopup(string mensaje)
    {
        InitializeComponent();
        MessageLabel.Text = mensaje;
    }

    private void OnYesClicked(object sender, EventArgs e)
    {
        Close(true); // Devuelve true
    }

    private void OnNoClicked(object sender, EventArgs e)
    {
        Close(false); // Devuelve false
    }
}