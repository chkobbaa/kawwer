using Kawwer.Mobile.ViewModels;

namespace Kawwer.Mobile.Views;

public partial class MatchDetailsPage : ContentPage
{
    public MatchDetailsPage(MatchDetailsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
