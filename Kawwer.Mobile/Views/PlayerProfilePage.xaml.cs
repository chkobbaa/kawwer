using Kawwer.Mobile.ViewModels;

namespace Kawwer.Mobile.Views;

public partial class PlayerProfilePage : ContentPage
{
    public PlayerProfilePage(PlayerProfileViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
