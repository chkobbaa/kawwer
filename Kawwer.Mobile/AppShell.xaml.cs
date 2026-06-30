using Kawwer.Mobile.Services;
using Kawwer.Mobile.Views;

namespace kawwer;

public partial class AppShell : Shell
{
    private readonly AuthService _auth;

    public AppShell(AuthService auth)
    {
        InitializeComponent();
        _auth = auth;

        // Detail and modal routes resolved through dependency injection.
        Routing.RegisterRoute("register", typeof(RegisterPage));
        Routing.RegisterRoute("creatematch", typeof(CreateMatchPage));
        Routing.RegisterRoute("matchdetails", typeof(MatchDetailsPage));
        Routing.RegisterRoute("chat", typeof(ChatPage));
        Routing.RegisterRoute("payments", typeof(PaymentsPage));
        Routing.RegisterRoute("notifications", typeof(NotificationsPage));
        Routing.RegisterRoute("groups", typeof(GroupsPage));
    }

    protected override async void OnNavigated(ShellNavigatedEventArgs args)
    {
        base.OnNavigated(args);

        // Restore the session once and route the user to the right place on first navigation.
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        await _auth.InitializeAsync();
        if (_auth.Session.IsAuthenticated)
        {
            await GoToAsync("//main");
        }
    }

    private bool _initialized;
}
