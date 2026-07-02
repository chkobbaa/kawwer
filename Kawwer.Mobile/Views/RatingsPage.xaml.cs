using Kawwer.Mobile.ViewModels;

namespace Kawwer.Mobile.Views;

public partial class RatingsPage : ContentPage
{
    public RatingsPage(RatingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
