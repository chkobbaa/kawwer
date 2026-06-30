using Kawwer.Mobile.ViewModels;

namespace Kawwer.Mobile.Views;

public partial class PaymentsPage : ContentPage
{
    public PaymentsPage(PaymentsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
