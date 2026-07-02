using Kawwer.Mobile.ViewModels;

namespace Kawwer.Mobile.Views;

public partial class CreateFieldPage : ContentPage
{
    public CreateFieldPage(CreateFieldViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
