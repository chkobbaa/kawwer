using Kawwer.Mobile.ViewModels;

namespace Kawwer.Mobile.Views;

public partial class InvitePlayersPage : ContentPage
{
    public InvitePlayersPage(InvitePlayersViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
