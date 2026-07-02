using Kawwer.Mobile.ViewModels;

namespace Kawwer.Mobile.Views;

public partial class FieldsPage : ContentPage
{
    private readonly FieldsViewModel _viewModel;

    public FieldsPage(FieldsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadCommand.Execute(null);
    }
}
