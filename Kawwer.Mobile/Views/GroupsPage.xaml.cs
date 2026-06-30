using Kawwer.Mobile.ViewModels;

namespace Kawwer.Mobile.Views;

public partial class GroupsPage : ContentPage
{
    private readonly GroupsViewModel _viewModel;

    public GroupsPage(GroupsViewModel viewModel)
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
