using Kawwer.Mobile.ViewModels;

namespace Kawwer.Mobile.Views;

public partial class LiveMatchPage : ContentPage
{
    public LiveMatchPage(LiveMatchViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
