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
        Routing.RegisterRoute("lineup", typeof(LineupPage));
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
            RouteAuthenticatedStart();
            _initialized = true;
        }
    }

    /// <summary>
    /// Sends an already-authenticated user to the right place at startup: users who still need to
    /// finish the first-run onboarding flow land on it, everyone else goes straight to the tabs.
    /// The decision uses the fast persisted flag; the onboarding page double-checks with the server.
    /// </summary>
    private void RouteAuthenticatedStart()
    {
        if (_auth.RequiresOnboarding)
        {
            ShowOnboarding();
        }
        else
        {
            ShowMainTabs();
        }
    }

    /// <summary>Switches to the full-screen onboarding flow.</summary>
    public void ShowOnboarding()
    {
        var onboarding = Items.FirstOrDefault(i => i.Route == "onboarding");
        if (onboarding is not null)
        {
            CurrentItem = onboarding;
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

    /// <summary>
    /// Replaces the app's root page with a brand-new <see cref="AppShell"/>. Assigning a fresh
    /// shell destroys the current navigation stack and every transient view model, so a user who
    /// logs out (and back in) lands on a clean Login screen rather than the last page they viewed.
    /// </summary>
    public static void ResetToLogin(AuthService auth)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (Application.Current is { } app && app.Windows.Count > 0)
            {
                app.Windows[0].Page = new AppShell(auth);
            }
        });
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
                RouteAuthenticatedStart();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"AppShell startup initialization failed: {ex}");
        }
    }

    private bool _initialized;
}
