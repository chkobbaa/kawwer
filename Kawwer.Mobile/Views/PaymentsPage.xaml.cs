using Kawwer.Mobile.ViewModels;

namespace Kawwer.Mobile.Views;

public partial class PaymentsPage : ContentPage
{
    private readonly PaymentsViewModel _viewModel;

    public PaymentsPage(PaymentsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.SubscribeRealtime();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.UnsubscribeRealtime();
    }
}
