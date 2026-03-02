using System.ComponentModel;
using HRReserveApp.Models;
using HRReserveApp.Services;

namespace HRReserveApp.Forms;

public sealed class MainForm : Form
{
    private readonly AppUser _currentUser;
    private readonly ReserveService _service;
    private readonly BindingList<Employee> _employees = [];
    private readonly BindingList<AttestationViewModel> _attestations = [];
    private readonly DataGridView _employeesGrid = new();
    private readonly DataGridView _attestationGrid = new();
    private readonly Label _lblMetrics = new();

    public MainForm(AppUser currentUser, ReserveService service)
    {
        _currentUser = currentUser;
        _service = service;

        Text = "Автоматизированная система кадрового резерва и аттестации";
        WindowState = FormWindowState.Maximized;
        BackColor = Color.White;

        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 82,
            BackColor = Color.FromArgb(33, 87, 158)
        };

        var lblTitle = new Label
        {
            Text = "Кадровый резерв общеобразовательного учреждения",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            AutoSize = false,
            Width = 900,
            Height = 36,
            Left = 20,
            Top = 10
        };

        var lblUser = new Label
        {
            Text = $"Пользователь: {_currentUser.DisplayName} ({_currentUser.Role})",
            ForeColor = Color.WhiteSmoke,
            Font = new Font("Segoe UI", 10),
            AutoSize = true,
            Left = 22,
            Top = 48
        };

        header.Controls.Add(lblTitle);
        header.Controls.Add(lblUser);

        var tabs = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10) };
        tabs.TabPages.Add(CreateDashboardTab());
        tabs.TabPages.Add(CreateEmployeesTab());
        tabs.TabPages.Add(CreateAttestationTab());

        Controls.Add(tabs);
        Controls.Add(header);

        ReloadAll();
    }

    private TabPage CreateDashboardTab()
    {
        var tab = new TabPage("Аналитика");
        _lblMetrics.Dock = DockStyle.Top;
        _lblMetrics.Height = 130;
        _lblMetrics.Font = new Font("Segoe UI", 12);
        _lblMetrics.Padding = new Padding(20);

        var note = new Label
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20),
            Text = "Рекомендации:\n• Ежемесячно обновляйте планы развития кандидатов.\n• Контролируйте статусы аттестаций за 2 недели до комиссии.\n• Формируйте протоколы и сводные отчёты по результатам комиссии.",
            Font = new Font("Segoe UI", 11)
        };

        tab.Controls.Add(note);
        tab.Controls.Add(_lblMetrics);
        return tab;
    }

    private TabPage CreateEmployeesTab()
    {
        var tab = new TabPage("Кадровый резерв");

        _employeesGrid.Dock = DockStyle.Fill;
        _employeesGrid.ReadOnly = true;
        _employeesGrid.AutoGenerateColumns = true;
        _employeesGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

        var panel = CreateToolbar(
            ("Добавить", (_, _) => AddEmployee()),
            ("Редактировать", (_, _) => EditEmployee()),
            ("Удалить", (_, _) => DeleteEmployee()));

        tab.Controls.Add(_employeesGrid);
        tab.Controls.Add(panel);
        return tab;
    }

    private TabPage CreateAttestationTab()
    {
        var tab = new TabPage("Аттестации");

        _attestationGrid.Dock = DockStyle.Fill;
        _attestationGrid.ReadOnly = true;
        _attestationGrid.AutoGenerateColumns = true;
        _attestationGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

        var panel = CreateToolbar(
            ("Назначить", (_, _) => AddAttestation()),
            ("Изменить", (_, _) => EditAttestation()),
            ("Удалить", (_, _) => DeleteAttestation()));

        tab.Controls.Add(_attestationGrid);
        tab.Controls.Add(panel);

        return tab;
    }

    private static Panel CreateToolbar(params (string text, EventHandler handler)[] buttons)
    {
        var panel = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = Color.FromArgb(240, 243, 250) };
        var left = 12;
        foreach (var (text, handler) in buttons)
        {
            var button = new Button
            {
                Text = text,
                Left = left,
                Width = 120,
                Top = 8,
                Height = 32,
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            button.Click += handler;
            panel.Controls.Add(button);
            left += 132;
        }

        return panel;
    }

    private void ReloadAll()
    {
        _employees.Clear();
        foreach (var employee in _service.GetEmployees())
        {
            _employees.Add(employee);
        }
        _employeesGrid.DataSource = _employees;

        _attestations.Clear();
        foreach (var record in _service.GetAttestations())
        {
            _attestations.Add(record);
        }
        _attestationGrid.DataSource = _attestations;

        var m = _service.GetMetrics();
        _lblMetrics.Text =
            $"Всего кандидатов в резерве: {m.EmployeesTotal}\n" +
            $"Активные аттестации: {m.ActiveAttestation}\n" +
            $"Завершённые аттестации: {m.CompletedAttestation}\n" +
            $"Рекомендованы к повышению: {m.RecommendedCount}";
    }

    private Employee? SelectedEmployee()
        => _employeesGrid.CurrentRow?.DataBoundItem as Employee;

    private AttestationViewModel? SelectedAttestation()
        => _attestationGrid.CurrentRow?.DataBoundItem as AttestationViewModel;

    private void AddEmployee()
    {
        using var form = new EmployeeEditorForm();
        if (form.ShowDialog() != DialogResult.OK || form.Employee is null) return;
        _service.AddEmployee(form.Employee);
        ReloadAll();
    }

    private void EditEmployee()
    {
        var selected = SelectedEmployee();
        if (selected is null) return;

        using var form = new EmployeeEditorForm(selected);
        if (form.ShowDialog() != DialogResult.OK || form.Employee is null) return;
        _service.UpdateEmployee(form.Employee);
        ReloadAll();
    }

    private void DeleteEmployee()
    {
        var selected = SelectedEmployee();
        if (selected is null) return;

        if (MessageBox.Show($"Удалить {selected.FullName}?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            _service.DeleteEmployee(selected.Id);
            ReloadAll();
        }
    }

    private void AddAttestation()
    {
        using var form = new AttestationEditorForm(_service.GetEmployees().ToList());
        if (form.ShowDialog() != DialogResult.OK || form.Record is null) return;
        _service.AddAttestation(form.Record);
        ReloadAll();
    }

    private void EditAttestation()
    {
        var selected = SelectedAttestation();
        if (selected is null) return;

        var record = new AttestationRecord
        {
            Id = selected.Id,
            EmployeeId = selected.EmployeeId,
            PlannedDate = selected.PlannedDate,
            CompletedDate = selected.CompletedDate,
            Status = selected.Status,
            Result = selected.Result,
            CommissionComment = selected.CommissionComment
        };

        using var form = new AttestationEditorForm(_service.GetEmployees().ToList(), record);
        if (form.ShowDialog() != DialogResult.OK || form.Record is null) return;
        _service.UpdateAttestation(form.Record);
        ReloadAll();
    }

    private void DeleteAttestation()
    {
        var selected = SelectedAttestation();
        if (selected is null) return;

        if (MessageBox.Show("Удалить запись аттестации?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            _service.DeleteAttestation(selected.Id);
            ReloadAll();
        }
    }
}
