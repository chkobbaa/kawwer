using CommunityToolkit.Maui.Views;

namespace Kawwer.Mobile.Views;

/// <summary>A small, styled confirmation toast shown instead of a native success alert.</summary>
public partial class SuccessPopup : Popup
{
    public SuccessPopup(string message)
    {
        InitializeComponent();
        MessageLabel.Text = message;
    }
}
