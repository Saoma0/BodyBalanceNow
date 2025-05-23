using CommunityToolkit.Maui.Views;

namespace BodyBalanceNow.View.Components;

public partial class PromptPopup : Popup
{
    public PromptPopup(string mensaje, string placeholder = "")
    {
        InitializeComponent();
        PromptLabel.Text = mensaje;
        PromptEntry.Placeholder = placeholder;
    }

    private void OnAcceptClicked(object sender, EventArgs e)
    {
        Close(PromptEntry.Text); // Devuelve el texto ingresado
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        Close(null);
    }
}
