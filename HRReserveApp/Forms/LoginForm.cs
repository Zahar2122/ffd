using HRReserveApp.Models;
using HRReserveApp.Services;

namespace HRReserveApp.Forms;

public sealed class LoginForm : Form
{
    private readonly AuthService _authService;
    private readonly TextBox _txtLogin;
    private readonly TextBox _txtPassword;

    public AppUser? CurrentUser { get; private set; }

    public LoginForm(AuthService authService)
    {
        _authService = authService;

        Text = "Авторизация | HR Reserve";
        Width = 560;
        Height = 360;
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        BackColor = Color.FromArgb(243, 246, 254);

        var card = new Panel
        {
            Width = 460,
            Height = 250,
            Left = 45,
            Top = 45,
            BackColor = Color.White
        };

        var title = new Label
        {
            Text = "Система кадрового резерва",
            Font = new Font("Segoe UI", 15, FontStyle.Bold),
            Left = 24,
            Top = 20,
            Width = 400
        };

        var subtitle = new Label
        {
            Text = "Авторизация для кадровой службы и аттестационной комиссии",
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.DimGray,
            Left = 24,
            Top = 52,
            Width = 420
        };

        _txtLogin = new TextBox { Left = 24, Top = 92, Width = 410, PlaceholderText = "Логин" };
        _txtPassword = new TextBox { Left = 24, Top = 130, Width = 410, PlaceholderText = "Пароль", UseSystemPasswordChar = true };

        var btnLogin = new Button
        {
            Text = "Войти в систему",
            Left = 24,
            Top = 176,
            Width = 410,
            Height = 38,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(47, 94, 214),
            ForeColor = Color.White
        };

        btnLogin.Click += (_, _) => SignIn();
        AcceptButton = btnLogin;

        var hint = new Label
        {
            Text = "Тестовый доступ: admin / admin123",
            Left = 24,
            Top = 218,
            Width = 410,
            ForeColor = Color.Gray
        };

        card.Controls.AddRange([title, subtitle, _txtLogin, _txtPassword, btnLogin, hint]);
        Controls.Add(card);
    }

    private void SignIn()
    {
        var user = _authService.SignIn(_txtLogin.Text.Trim(), _txtPassword.Text.Trim());
        if (user is null)
        {
            MessageBox.Show("Неверный логин или пароль", "Ошибка входа", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        CurrentUser = user;
        DialogResult = DialogResult.OK;
        Close();
    }
}
