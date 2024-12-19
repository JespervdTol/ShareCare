using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Components.Authorization;
using ShareCare.Services;
using Microsoft.Maui.Hosting;

namespace ShareCare
{
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
                });

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            //string connectionString = "Server=192.168.123.167;Port=3306;Database=sharecare;User=Manager;Password=DitIsEenSterkWachtwoord12345!;";
            string connectionString = "Server=192.168.2.11;Port=3306;Database=sharecare;User=root;Password=jtol5014;";

            builder.Services.AddSingleton(new DatabaseService(connectionString));
            builder.Services.AddScoped<CustomAuthenticationStateProvider>();
            builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
                provider.GetRequiredService<CustomAuthenticationStateProvider>());
            builder.Services.AddScoped<UserService>();
            builder.Services.AddScoped<TaskService>();
            builder.Services.AddScoped<EventService>();
            builder.Services.AddScoped<PaymentService>();

            return builder.Build();
        }
    }
}