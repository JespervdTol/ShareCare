using Moq;
using Xunit;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using ShareCare.Services;
using ShareCare.Module;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

public class UserServiceTests
{
    private readonly Mock<DatabaseService> _mockDatabaseService;
    private readonly Mock<CustomAuthenticationStateProvider> _mockAuthStateProvider;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _mockDatabaseService = new Mock<DatabaseService>();
        _mockAuthStateProvider = new Mock<CustomAuthenticationStateProvider>();

        _userService = new UserService(_mockDatabaseService.Object, _mockAuthStateProvider.Object);
    }

    //[Fact]
    //public async System.Threading.Tasks.Task GetUsersAsync_ShouldReturnListOfPersons_WhenDataIsValid()
    //{
    //    var mockDataTable = new System.Data.DataTable();
    //    mockDataTable.Columns.Add("id");
    //    mockDataTable.Columns.Add("firstname");
    //    mockDataTable.Columns.Add("intersertion");
    //    mockDataTable.Columns.Add("lastname");
    //    mockDataTable.Columns.Add("email");

    //    var row = mockDataTable.NewRow();
    //    row["id"] = 1;
    //    row["firstname"] = "John";
    //    row["intersertion"] = "Mr.";
    //    row["lastname"] = "Doe";
    //    row["email"] = "john.doe@example.com";
    //    mockDataTable.Rows.Add(row);

    //    _mockDatabaseService.Setup(d => d.ExecuteQueryAsync(It.IsAny<string>()))
    //        .ReturnsAsync(mockDataTable);

    //    var result = await _userService.GetUsersAsync();

    //    result.Should().HaveCount(1);
    //    result[0].FirstName.Should().Be("John");
    //    result[0].LastName.Should().Be("Doe");
    //}

    //[Fact]
    //public async System.Threading.Tasks.Task RegisterUser_ShouldReturnTrue_WhenUserIsRegistered()
    //{
    //    var account = new Account { Username = "EKelder", Password = "PSVFAN123" };
    //    var person = new Person { FirstName = "Erik", LastName = "Kelder", Email = "e.kelder@fontys.nl" };

    //    _mockDatabaseService.Setup(d => d.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<MySqlParameter[]>()))
    //        .ReturnsAsync(1);

    //    var result = await _userService.RegisterUser(account, person);

    //    result.Should().BeTrue();
    //}

    //[Fact]
    //public async System.Threading.Tasks.Task RegisterUser_ShouldReturnFalse_WhenPasswordIsInvalid()
    //{
    //    var account = new Account { Username = "EKelder", Password = "**$^^!^$**" };
    //    var person = new Person { FirstName = "Erik", LastName = "Kelder", Email = "e.kelder@fontys.nl" };

    //    var result = await _userService.RegisterUser(account, person);

    //    result.Should().BeFalse();
    //}

    //[Fact]
    //public async System.Threading.Tasks.Task LoginUser_ShouldReturnTrue_WhenCredentialsAreValid()
    //{
    //    var username = "EKelder";
    //    var password = "PSVFAN123";
    //    var mockDataTable = new System.Data.DataTable();
    //    mockDataTable.Columns.Add("password");
    //    mockDataTable.Columns.Add("id");

    //    var row = mockDataTable.NewRow();
    //    row["password"] = BCrypt.Net.BCrypt.HashPassword(password);
    //    row["id"] = 1;
    //    mockDataTable.Rows.Add(row);

    //    _mockDatabaseService.Setup(d => d.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<MySqlParameter[]>()))
    //        .ReturnsAsync(mockDataTable);

    //    var result = await _userService.LoginUser(username, password);

    //    result.Should().BeTrue();
    //}

    //[Fact]
    //public async System.Threading.Tasks.Task LoginUser_ShouldReturnFalse_WhenCredentialsAreInvalid()
    //{
    //    var username = "EKelder";
    //    var password = "AJAXFAN123";
    //    var mockDataTable = new System.Data.DataTable();
    //    mockDataTable.Columns.Add("password");
    //    mockDataTable.Columns.Add("id");

    //    var row = mockDataTable.NewRow();
    //    row["password"] = BCrypt.Net.BCrypt.HashPassword(password);
    //    row["id"] = 1;
    //    mockDataTable.Rows.Add(row);

    //    _mockDatabaseService.Setup(d => d.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<MySqlParameter[]>()))
    //        .ReturnsAsync(mockDataTable);

    //    var result = await _userService.LoginUser(username, password);

    //    result.Should().BeFalse();
    //}

    //[Fact]
    //public async System.Threading.Tasks.Task LoginUser_ShouldReturnFalse_WhenCredentialsAreInvalid()
    //{
    //    var username = "johndoe";
    //    var password = "IncorrectPassword";
    //    var mockDataTable = new System.Data.DataTable();
    //    mockDataTable.Columns.Add("password");
    //    mockDataTable.Columns.Add("id");

    //    var row = mockDataTable.NewRow();
    //    row["password"] = BCrypt.Net.BCrypt.HashPassword("CorrectPassword");
    //    row["id"] = 1;
    //    mockDataTable.Rows.Add(row);

    //    _mockDatabaseService.Setup(d => d.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<MySqlParameter[]>()))
    //        .ReturnsAsync(mockDataTable);

    //    var result = await _userService.LoginUser(username, password);

    //    result.Should().BeFalse();
    //}

    //[Fact]
    //public async System.Threading.Tasks.Task LogoutUser_ShouldReturnTrue_WhenSuccessfullyLoggedOut()
    //{
    //    _mockAuthStateProvider.Setup(a => a.MarkUserAsLoggedOut());

    //    var result = await _userService.LogoutUser();

    //    result.Should().BeTrue();
    //}

    //[Fact]
    //public async System.Threading.Tasks.Task GetLoggedInUser_ShouldReturnPerson_WhenUserExists()
    //{
    //    var username = "johndoe";
    //    var mockDataTable = new System.Data.DataTable();
    //    mockDataTable.Columns.Add("firstname");
    //    mockDataTable.Columns.Add("intersertion");
    //    mockDataTable.Columns.Add("lastname");
    //    mockDataTable.Columns.Add("email");
    //    mockDataTable.Columns.Add("date_of_birth");

    //    var row = mockDataTable.NewRow();
    //    row["firstname"] = "John";
    //    row["intersertion"] = "Mr.";
    //    row["lastname"] = "Doe";
    //    row["email"] = "john.doe@example.com";
    //    row["date_of_birth"] = "01/01/1990";
    //    mockDataTable.Rows.Add(row);

    //    _mockDatabaseService.Setup(d => d.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<MySqlParameter[]>()))
    //        .ReturnsAsync(mockDataTable);

    //    var result = await _userService.GetLoggedInUser(username);

    //    result.Should().NotBeNull();
    //    result.FirstName.Should().Be("John");
    //    result.LastName.Should().Be("Doe");
    //}
}