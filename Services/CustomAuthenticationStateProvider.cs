using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using System.Threading.Tasks;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private ClaimsPrincipal _currentUser = new ClaimsPrincipal(new ClaimsIdentity());

    public void MarkUserAsAuthenticated(string username, int personId)
    {
        var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim("PersonID", personId.ToString())
        }, "customAuth"));

        _currentUser = authenticatedUser;
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public void MarkUserAsLoggedOut()
    {
        _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public string GetCurrentUsername()
    {
        return _currentUser.Identity.Name;
    }

    public int GetCurrentUserPersonId()
    {
        var personIdClaim = _currentUser.FindFirst("PersonID");
        if (personIdClaim != null && int.TryParse(personIdClaim.Value, out int personId))
        {
            return personId;
        }
        return 0;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return Task.FromResult(new AuthenticationState(_currentUser));
    }
}