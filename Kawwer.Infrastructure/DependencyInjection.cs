using Kawwer.Application.Common.Interfaces;
using Kawwer.Infrastructure.BackgroundJobs;
using Kawwer.Infrastructure.Identity;
using Kawwer.Infrastructure.Notifications;
using Kawwer.Infrastructure.Persistence;
using Kawwer.Infrastructure.Persistence.Repositories;
using Kawwer.Infrastructure.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kawwer.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<FirebaseOptions>(configuration.GetSection(FirebaseOptions.SectionName));
        services.Configure<WebPushOptions>(configuration.GetSection(WebPushOptions.SectionName));

        var connectionString = configuration.GetConnectionString("Postgres")
            ?? configuration.GetConnectionString("Default")
            ?? "Host=localhost;Port=5432;Database=kawwer;Username=postgres;Password=postgres";

        services.AddDbContext<KawwerDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IFriendshipRepository, FriendshipRepository>();
        services.AddScoped<ITeamRepository, TeamRepository>();
        services.AddScoped<IFootballFieldRepository, FootballFieldRepository>();
        services.AddScoped<IMatchRepository, MatchRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IChatRepository, ChatRepository>();
        services.AddScoped<IRatingRepository, RatingRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IPushSubscriptionRepository, PushSubscriptionRepository>();

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

        services.AddSingleton<IPushNotificationSender, FirebasePushNotificationSender>();
        services.AddSingleton<IWebPushSender, WebPushNotificationSender>();

        services.AddHostedService<MatchReminderService>();

        return services;
    }
}
