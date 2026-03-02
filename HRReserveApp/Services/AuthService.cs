using HRReserveApp.Data;
using HRReserveApp.Models;

namespace HRReserveApp.Services;

public sealed class AuthService(AppDataStore dataStore)
{
    public AppUser? SignIn(string login, string password)
    {
        var data = dataStore.Load();
        return data.Users.FirstOrDefault(x =>
            x.Login.Equals(login, StringComparison.OrdinalIgnoreCase) &&
            x.Password == password);
    }
}
