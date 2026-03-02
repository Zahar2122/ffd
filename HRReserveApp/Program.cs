using HRReserveApp.Data;
using HRReserveApp.Forms;
using HRReserveApp.Services;

namespace HRReserveApp;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        var store = new AppDataStore();
        var authService = new AuthService(store);
        var reserveService = new ReserveService(store);

        using var login = new LoginForm(authService);
        if (login.ShowDialog() != DialogResult.OK || login.CurrentUser is null)
        {
            return;
        }

        Application.Run(new MainForm(login.CurrentUser, reserveService));
    }
}
