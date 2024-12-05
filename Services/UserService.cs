using System;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using BCrypt.Net;
using Org.BouncyCastle.Crypto.Generators;
using ShareCare.Services;
using ShareCare.Module;
using Microsoft.AspNetCore.Components.Authorization;

public class UserService
{
    private readonly DatabaseService _databaseService;
    private readonly CustomAuthenticationStateProvider _authenticationStateProvider;

    public UserService(DatabaseService databaseService, CustomAuthenticationStateProvider authenticationStateProvider)
    {
        _databaseService = databaseService;
        _authenticationStateProvider = authenticationStateProvider;
    }

    public async Task<bool> RegisterUser(Account account, Person person)
    {
        var hashedPassword = HashPassword(account.Password);

        const string query = @"
        INSERT INTO user (username, email, password, firstname, intersertion, lastname, date_of_birth) 
        VALUES (@Username, @Email, @Password, @FirstName, @Intersertion, @LastName, @DateOfBirth)";

        var parameters = new MySqlParameter[]
        {
            new MySqlParameter("@Username", account.Username),
            new MySqlParameter("@Email", person.Email),
            new MySqlParameter("@Password", hashedPassword),
            new MySqlParameter("@FirstName", person.FirstName),
            new MySqlParameter("@Intersertion", person.Intersertion),
            new MySqlParameter("@LastName", person.LastName),
            new MySqlParameter("@DateOfBirth", person.DateOfBirth.ToString("yyyy-MM-dd"))
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
                if (VerifyPassword(password, storedHash))
                {
                    _authenticationStateProvider.MarkUserAsAuthenticated(username);
                    return true;
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during login: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> LogoutUser()
    {
        try
        {
            _authenticationStateProvider.MarkUserAsLoggedOut();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during logout: {ex.Message}");
            return false;
        }
    }

    public async Task<Person> GetLoggedInUser(string username)
    {
        const string query = "SELECT firstname, intersertion, lastname, email, date_of_birth FROM user WHERE username = @Username";
        var parameters = new MySqlParameter[]
        {
            new MySqlParameter("@Username", username)
        };

        try
        {
            var dataTable = await _databaseService.ExecuteQueryAsync(query, parameters);
            if (dataTable.Rows.Count > 0)
            {
                var row = dataTable.Rows[0];

                DateOnly dateOfBirth = DateOnly.MinValue;
                string dateOfBirthString = row["date_of_birth"].ToString();

                return new Person
                {
                    FirstName = row["firstname"].ToString(),
                    Intersertion = row["intersertion"].ToString(),
                    LastName = row["lastname"].ToString(),
                    Email = row["email"].ToString(),
                    DateOfBirth = dateOfBirth
                };
            }

            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error fetching logged-in user: {ex.Message}");
            return null;
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