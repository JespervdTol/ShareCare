using System;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using BCrypt.Net;
using Org.BouncyCastle.Crypto.Generators;
using ShareCare.Services;

public class UserService
{
    private readonly DatabaseService _databaseService;

    public UserService(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public async Task<bool> RegisterUser(string username, string email, string password)
    {
        var hashedPassword = HashPassword(password);

        const string query = "INSERT INTO user (username, email, password) VALUES (@Username, @Email, @Password)";
        var parameters = new MySqlParameter[]
        {
            new MySqlParameter("@Username", username),
            new MySqlParameter("@Email", email),
            new MySqlParameter("@Password", hashedPassword)
        };

        try
        {
            int result = await _databaseService.ExecuteNonQueryAsync(query, parameters);
            return result > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during registration: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> LoginUser(string username, string password)
    {
        const string query = "SELECT password FROM user WHERE username = @Username";
        var parameters = new MySqlParameter[]
        {
            new MySqlParameter("@Username", username)
        };

        try
        {
            var result = await _databaseService.ExecuteScalarAsync(query, parameters);
            if (result != null)
            {
                string storedHash = result.ToString();
                return VerifyPassword(password, storedHash);
            }
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during login: {ex.Message}");
            return false;
        }
    }

    private string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    private bool VerifyPassword(string password, string storedHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, storedHash);
    }
}