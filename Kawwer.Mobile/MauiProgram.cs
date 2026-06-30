using System.Net.Http.Headers;
using Kawwer.Mobile.Services;
using Kawwer.Mobile.ViewModels;
using Kawwer.Mobile.Views;
using Microsoft.Extensions.Logging;

namespace kawwer;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // ----- Core services -----
        builder.Services.AddSingleton<SessionState>();
        builder.Services.AddSingleton<AuthService>();
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
        builder.Services.AddTransient<ChatViewModel>();
        builder.Services.AddTransient<PaymentsViewModel>();
        builder.Services.AddTransient<NotificationsViewModel>();

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
        builder.Services.AddTransient<ChatPage>();
        builder.Services.AddTransient<PaymentsPage>();
        builder.Services.AddTransient<NotificationsPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
