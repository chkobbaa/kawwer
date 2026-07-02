using Kawwer.Mobile.ViewModels;

namespace Kawwer.Mobile.Views;

public partial class CalendarPage : ContentPage
{
    private readonly CalendarViewModel _viewModel;

    public CalendarPage(CalendarViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Show cached content instantly; only re-fetch when the data is stale.
        if (_viewModel.IsStale(TimeSpan.FromSeconds(30)))
        {
            _viewModel.LoadCommand.Execute(null);
        }
    }
}
