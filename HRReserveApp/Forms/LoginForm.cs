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

        Text = "Авторизация | Кадровый резерв";
        Width = 460;
        Height = 320;
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(30),
            BackColor = Color.FromArgb(245, 248, 255)
        };

        var lblTitle = new Label
        {
            Text = "Система формирования кадрового резерва",
            Font = new Font("Segoe UI", 13, FontStyle.Bold),
            Dock = DockStyle.Top,
            Height = 60
        };

        var lblLogin = new Label { Text = "Логин", Top = 75, Left = 15, Width = 80 };
        _txtLogin = new TextBox { Top = 95, Left = 15, Width = 380, Text = "admin" };

        var lblPassword = new Label { Text = "Пароль", Top = 130, Left = 15, Width = 80 };
        _txtPassword = new TextBox { Top = 150, Left = 15, Width = 380, UseSystemPasswordChar = true, Text = "admin123" };

        var btnLogin = new Button
        {
            Text = "Войти",
            Top = 200,
            Left = 15,
            Width = 380,
            Height = 38,
            BackColor = Color.FromArgb(45, 108, 223),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };

        btnLogin.Click += (_, _) => SignIn();
        AcceptButton = btnLogin;

        panel.Controls.Add(lblTitle);
        panel.Controls.Add(lblLogin);
        panel.Controls.Add(_txtLogin);
        panel.Controls.Add(lblPassword);
        panel.Controls.Add(_txtPassword);
        panel.Controls.Add(btnLogin);

        Controls.Add(panel);
    }

    private void SignIn()
    {
        var user = _authService.SignIn(_txtLogin.Text.Trim(), _txtPassword.Text.Trim());
        if (user is null)
        {
            MessageBox.Show("Неверный логин или пароль", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        CurrentUser = user;
        DialogResult = DialogResult.OK;
        Close();
    }
}
