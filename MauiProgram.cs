using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using ShareCare.Services;

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
            //string connectionString = "Server=10.0.2.2;Port=3306;Database=sharecare;Uid=root;Pwd=jtol123;SslMode=REQUIRED;";
            //string connectionString = "Server=192.168.68.98;Port=3306;Database=sharecare;Uid=root;Pwd=jtol123;sslMode=REQUIRED";
            string connectionString = "Server=192.168.123.167;Port=3306;Database=sharecare;User=Manager;Password=DitIsEenSterkWachtwoord12345!;sslMode=REQUIRED;";

            builder.Services.AddSingleton(new DatabaseService(connectionString));

            //TestMySqlConnection(connectionString);

            builder.Services.AddTransient<UserService>();

            return builder.Build();

        }
        private static void TestMySqlConnection(string connectionString)
        {
            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                using var command = new MySqlCommand("SELECT 1;", connection);
                var result = command.ExecuteScalar();
                System.Diagnostics.Debug.WriteLine("Successfully connected to MySQL: " + result);
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine($"MySQL Error Code: {ex.Number}");
                System.Diagnostics.Debug.WriteLine($"Error Message: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"SQLState: {ex.SqlState}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"General Error: {ex.Message}");
            }
        }

    }
}