using System.Diagnostics;
using Kawwer.Mobile.Services;
using Kawwer.Mobile.Views;

namespace kawwer;

public partial class AppShell : Shell
{
    private readonly AuthService _auth;
    private readonly Task _sessionLoad;

    public AppShell(AuthService auth)
    {
        InitializeComponent();
        _auth = auth;

        // Detail and modal routes resolved through dependency injection.
        Routing.RegisterRoute("register", typeof(RegisterPage));
        Routing.RegisterRoute("creatematch", typeof(CreateMatchPage));
        Routing.RegisterRoute("matchdetails", typeof(MatchDetailsPage));
        Routing.RegisterRoute("inviteplayers", typeof(InvitePlayersPage));
        Routing.RegisterRoute("chat", typeof(ChatPage));
        Routing.RegisterRoute("payments", typeof(PaymentsPage));
        Routing.RegisterRoute("notifications", typeof(NotificationsPage));
        Routing.RegisterRoute("groups", typeof(GroupsPage));
        Routing.RegisterRoute("fields", typeof(FieldsPage));
        Routing.RegisterRoute("createfield", typeof(CreateFieldPage));
        Routing.RegisterRoute("mappicker", typeof(MapPickerPage));
        Routing.RegisterRoute("settings", typeof(SettingsPage));
        Routing.RegisterRoute("livematch", typeof(LiveMatchPage));
        Routing.RegisterRoute("ratings", typeof(RatingsPage));
        Routing.RegisterRoute("playerprofile", typeof(PlayerProfilePage));

        // Start restoring the tokens right away; API calls await this via AuthHeaderHandler.
        _sessionLoad = _auth.InitializeAsync();

        // If a session was persisted, skip the login screen entirely - no "thinking" flash.
        if (_auth.Session.HasPersistedSession)
        {
            ShowMainTabs();
            _initialized = true;
        }
    }

    /// <summary>Switches to the main tab bar with the Home tab selected.</summary>
    public void ShowMainTabs()
    {
        var main = Items.FirstOrDefault(i => i.Route == "main");
        if (main is null)
        {
            return;
        }

        var home = main.Items.FirstOrDefault(s => s.Route == "hometab");
        if (home is not null)
        {
            main.CurrentItem = home;
        }

        CurrentItem = main;
    }

    protected override async void OnNavigated(ShellNavigatedEventArgs args)
    {
        base.OnNavigated(args);

        // A tapped push notification may be waiting for the Shell to become ready.
        _ = NotificationNavigation.TryNavigateAsync();

        // Fallback for older installs that have tokens but no persisted-session flag yet.
        if (_initialized)
        {
            return;
        }

        _initialized = true;

        try
        {
            await _sessionLoad;
            if (_auth.Session.IsAuthenticated)
            {
                ShowMainTabs();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"AppShell startup initialization failed: {ex}");
        }
    }

    private bool _initialized;
}
