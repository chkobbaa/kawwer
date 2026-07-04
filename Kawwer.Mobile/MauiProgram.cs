using System.Net.Http.Headers;
using CommunityToolkit.Maui;
using Kawwer.Mobile.Services;
using Kawwer.Mobile.ViewModels;
using Kawwer.Mobile.Views;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
#if IOS
using Plugin.Firebase.Bundled.Shared;
using Plugin.Firebase.Bundled.Platforms.iOS;
using Plugin.Firebase.CloudMessaging;
#endif

namespace kawwer;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if IOS
        builder.ConfigureLifecycleEvents(events =>
        {
            events.AddiOS(iOS => iOS.WillFinishLaunching((_, __) =>
            {
                CrossFirebase.Initialize(new CrossFirebaseSettings(isCloudMessagingEnabled: true));
                FirebaseCloudMessagingImplementation.Initialize();
                PushNotifications.WireEvents();
                return false;
            }));
        });
#endif

#if ANDROID
        // Icon-only bottom tabs + pop-to-root when the current tab is re-tapped.
        builder.ConfigureMauiHandlers(handlers => handlers.AddHandler(typeof(AppShell), typeof(KawwerShellRenderer)));
#endif

#if IOS
        // Icon-only bottom tabs on iOS (hide titles, center the icons).
        builder.ConfigureMauiHandlers(handlers => handlers.AddHandler(typeof(AppShell), typeof(CustomShellRenderer)));
#endif

        // ----- Core services -----
        builder.Services.AddSingleton<SessionState>();
        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<PushRegistrationService>();
        builder.Services.AddSingleton<MatchReminderService>();
        builder.Services.AddSingleton<IDialogService, DialogService>();
        builder.Services.AddTransient<UpdateService>();
        builder.Services.AddTransient<AuthHeaderHandler>();

        builder.Services
            .AddHttpClient<KawwerApiClient>(client =>
            {
                client.BaseAddress = new Uri(AppConfig.ApiBaseUrl.TrimEnd('/') + "/");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            })
            .AddHttpMessageHandler<AuthHeaderHandler>();

        // ----- Shell & app -----
        builder.Services.AddSingleton<AppShell>();

        // ----- View models -----
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<DiscoverViewModel>();
        builder.Services.AddTransient<CalendarViewModel>();
        builder.Services.AddTransient<FriendsViewModel>();
        builder.Services.AddTransient<GroupsViewModel>();
        builder.Services.AddTransient<ProfileViewModel>();
        builder.Services.AddTransient<CreateMatchViewModel>();
        builder.Services.AddTransient<MatchDetailsViewModel>();
        builder.Services.AddTransient<InvitePlayersViewModel>();
        builder.Services.AddTransient<ChatViewModel>();
        builder.Services.AddTransient<PaymentsViewModel>();
        builder.Services.AddTransient<NotificationsViewModel>();
        builder.Services.AddTransient<FieldsViewModel>();
        builder.Services.AddTransient<CreateFieldViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<LiveMatchViewModel>();
        builder.Services.AddTransient<RatingsViewModel>();
        builder.Services.AddTransient<PlayerProfileViewModel>();

        // ----- Pages -----
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<DiscoverPage>();
        builder.Services.AddTransient<CalendarPage>();
        builder.Services.AddTransient<FriendsPage>();
        builder.Services.AddTransient<GroupsPage>();
        builder.Services.AddTransient<ProfilePage>();
        builder.Services.AddTransient<CreateMatchPage>();
        builder.Services.AddTransient<MatchDetailsPage>();
        builder.Services.AddTransient<InvitePlayersPage>();
        builder.Services.AddTransient<ChatPage>();
        builder.Services.AddTransient<PaymentsPage>();
        builder.Services.AddTransient<NotificationsPage>();
        builder.Services.AddTransient<FieldsPage>();
        builder.Services.AddTransient<CreateFieldPage>();
        builder.Services.AddTransient<MapPickerPage>();
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<LiveMatchPage>();
        builder.Services.AddTransient<RatingsPage>();
        builder.Services.AddTransient<PlayerProfilePage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
