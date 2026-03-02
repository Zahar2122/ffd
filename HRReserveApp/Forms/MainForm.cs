using System.ComponentModel;
using System.Diagnostics;
using HRReserveApp.Models;
using HRReserveApp.Services;

namespace HRReserveApp.Forms;

public sealed class MainForm : Form
{
    private readonly AppUser _currentUser;
    private readonly ReserveService _service;
    private readonly ReportService _reportService;

    private readonly BindingList<Employee> _employees = [];
    private readonly BindingList<AttestationViewModel> _attestations = [];

    private readonly DataGridView _employeesGrid = new();
    private readonly DataGridView _attestationGrid = new();

    private readonly Label _lblCardReserve = new();
    private readonly Label _lblCardActive = new();
    private readonly Label _lblCardRecommended = new();
    private readonly Label _lblCardOverdue = new();

    private readonly ListBox _lstAlerts = new();
    private readonly TextBox _txtSearchEmployee = new();
    private readonly ComboBox _cmbFilterCompetency = new();
    private readonly RichTextBox _txtReportPreview = new();

    public MainForm(AppUser currentUser, ReserveService service, ReportService reportService)
    {
        _currentUser = currentUser;
        _service = service;
        _reportService = reportService;

        Text = "Единая система кадрового резерва и аттестации";
        WindowState = FormWindowState.Maximized;
        BackColor = Color.FromArgb(245, 247, 252);

        Controls.Add(CreateTabs());
        Controls.Add(CreateHeader());

        ReloadAll();
    }

    private Control CreateHeader()
    {
        var panel = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Color.FromArgb(29, 78, 216) };

        var title = new Label
        {
            Text = "Автоматизированная система формирования кадрового резерва",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            Left = 20,
            Top = 12,
            Width = 900
        };

        var info = new Label
        {
            Text = $"{_currentUser.DisplayName} · {_currentUser.Role} · {DateTime.Now:dd.MM.yyyy}",
            ForeColor = Color.WhiteSmoke,
            Font = new Font("Segoe UI", 10),
            Left = 22,
            Top = 45,
            Width = 600
        };

        panel.Controls.Add(title);
        panel.Controls.Add(info);
        return panel;
    }

    private Control CreateTabs()
    {
        var tabs = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10) };
        tabs.TabPages.Add(CreateDashboardTab());
        tabs.TabPages.Add(CreateEmployeesTab());
        tabs.TabPages.Add(CreateAttestationTab());
        tabs.TabPages.Add(CreateReportsTab());
        return tabs;
    }

    private TabPage CreateDashboardTab()
    {
        var tab = new TabPage("Панель управления");

        var cards = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 145,
            Padding = new Padding(18),
            FlowDirection = FlowDirection.LeftToRight
        };

        cards.Controls.Add(CreateMetricCard("Кандидаты в резерве", _lblCardReserve, Color.FromArgb(37, 99, 235)));
        cards.Controls.Add(CreateMetricCard("Активные аттестации", _lblCardActive, Color.FromArgb(3, 105, 161)));
        cards.Controls.Add(CreateMetricCard("Рекомендованы", _lblCardRecommended, Color.FromArgb(5, 150, 105)));
        cards.Controls.Add(CreateMetricCard("Просрочено", _lblCardOverdue, Color.FromArgb(220, 38, 38)));

        var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, SplitterDistance = 650 };

        var note = new Label
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(16),
            Text = "Логика сопровождения:\n• отбор сотрудников и формирование индивидуальных планов;\n• контроль сроков аттестации и фиксация решений комиссии;\n• формирование отчётов для руководства учреждения.",
            Font = new Font("Segoe UI", 11)
        };

        var alertPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
        var alertTitle = new Label
        {
            Text = "Ближайшие и просроченные аттестации",
            Dock = DockStyle.Top,
            Height = 28,
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };

        _lstAlerts.Dock = DockStyle.Fill;
        alertPanel.Controls.Add(_lstAlerts);
        alertPanel.Controls.Add(alertTitle);

        split.Panel1.Controls.Add(note);
        split.Panel2.Controls.Add(alertPanel);

        tab.Controls.Add(split);
        tab.Controls.Add(cards);
        return tab;
    }

    private static Panel CreateMetricCard(string title, Label valueLabel, Color accent)
    {
        var card = new Panel
        {
            Width = 260,
            Height = 100,
            Margin = new Padding(8),
            BackColor = Color.White
        };

        var left = new Panel { Width = 8, Height = 100, BackColor = accent, Dock = DockStyle.Left };
        var lblTitle = new Label { Text = title, Left = 18, Top = 20, Width = 220, Font = new Font("Segoe UI", 10), ForeColor = Color.DimGray };

        valueLabel.Left = 18;
        valueLabel.Top = 48;
        valueLabel.Width = 220;
        valueLabel.Font = new Font("Segoe UI", 20, FontStyle.Bold);

        card.Controls.Add(left);
        card.Controls.Add(lblTitle);
        card.Controls.Add(valueLabel);
        return card;
    }

    private TabPage CreateEmployeesTab()
    {
        var tab = new TabPage("Кадровый резерв");

        var top = new Panel { Dock = DockStyle.Top, Height = 85, BackColor = Color.White, Padding = new Padding(12) };

        _txtSearchEmployee.SetBounds(12, 12, 280, 30);
        _txtSearchEmployee.PlaceholderText = "Поиск: ФИО, должность, подразделение";

        _cmbFilterCompetency.SetBounds(300, 12, 180, 30);
        _cmbFilterCompetency.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbFilterCompetency.Items.AddRange(["Все", "Базовый", "Средний", "Высокий"]);
        _cmbFilterCompetency.SelectedIndex = 0;

        var btnSearch = CreateActionButton("Применить", 490, 12, (_, _) => LoadEmployees());
        var btnReset = CreateActionButton("Сброс", 610, 12, (_, _) =>
        {
            _txtSearchEmployee.Text = string.Empty;
            _cmbFilterCompetency.SelectedIndex = 0;
            LoadEmployees();
        });

        var btnAdd = CreateActionButton("Добавить", 12, 48, (_, _) => AddEmployee());
        var btnEdit = CreateActionButton("Редактировать", 132, 48, (_, _) => EditEmployee());
        var btnDelete = CreateActionButton("Удалить", 252, 48, (_, _) => DeleteEmployee());

        top.Controls.AddRange([_txtSearchEmployee, _cmbFilterCompetency, btnSearch, btnReset, btnAdd, btnEdit, btnDelete]);

        _employeesGrid.Dock = DockStyle.Fill;
        _employeesGrid.AutoGenerateColumns = true;
        _employeesGrid.ReadOnly = true;
        _employeesGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

        tab.Controls.Add(_employeesGrid);
        tab.Controls.Add(top);
        return tab;
    }

    private TabPage CreateAttestationTab()
    {
        var tab = new TabPage("Аттестационные процедуры");

        var top = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = Color.White, Padding = new Padding(12) };
        top.Controls.Add(CreateActionButton("Назначить", 12, 10, (_, _) => AddAttestation()));
        top.Controls.Add(CreateActionButton("Изменить", 132, 10, (_, _) => EditAttestation()));
        top.Controls.Add(CreateActionButton("Удалить", 252, 10, (_, _) => DeleteAttestation()));
        top.Controls.Add(CreateActionButton("Завершить", 372, 10, (_, _) => CompleteAttestation()));

        _attestationGrid.Dock = DockStyle.Fill;
        _attestationGrid.AutoGenerateColumns = true;
        _attestationGrid.ReadOnly = true;
        _attestationGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

        tab.Controls.Add(_attestationGrid);
        tab.Controls.Add(top);
        return tab;
    }

    private TabPage CreateReportsTab()
    {
        var tab = new TabPage("Отчёты");

        var top = new Panel { Dock = DockStyle.Top, Height = 58, BackColor = Color.White, Padding = new Padding(12) };
        top.Controls.Add(CreateActionButton("Сводный отчёт", 12, 12, (_, _) => GenerateSummary()));
        top.Controls.Add(CreateActionButton("CSV: Резерв", 152, 12, (_, _) => ExportEmployees()));
        top.Controls.Add(CreateActionButton("CSV: Аттестации", 292, 12, (_, _) => ExportAttestations()));

        _txtReportPreview.Dock = DockStyle.Fill;
        _txtReportPreview.ReadOnly = true;
        _txtReportPreview.Font = new Font("Consolas", 11);
        _txtReportPreview.Text = "Здесь будет отображаться информация о сгенерированных отчётах.";

        tab.Controls.Add(_txtReportPreview);
        tab.Controls.Add(top);
        return tab;
    }

    private static Button CreateActionButton(string text, int left, int top, EventHandler onClick)
    {
        var btn = new Button
        {
            Text = text,
            Left = left,
            Top = top,
            Width = 112,
            Height = 32,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(241, 245, 249)
        };
        btn.Click += onClick;
        return btn;
    }

    private void ReloadAll()
    {
        LoadEmployees();

        _attestations.Clear();
        foreach (var record in _service.GetAttestations())
        {
            _attestations.Add(record);
        }
        _attestationGrid.DataSource = _attestations;

        var m = _service.GetMetrics();
        _lblCardReserve.Text = m.EmployeesTotal.ToString();
        _lblCardActive.Text = m.ActiveAttestation.ToString();
        _lblCardRecommended.Text = m.RecommendedCount.ToString();
        _lblCardOverdue.Text = m.OverdueAttestation.ToString();

        _lstAlerts.Items.Clear();
        foreach (var upcoming in _service.GetUpcomingAttestations(30))
        {
            _lstAlerts.Items.Add($"Ближайшая: {upcoming.EmployeeName} — {upcoming.PlannedDate:dd.MM.yyyy}");
        }
        foreach (var overdue in _service.GetOverdueAttestations())
        {
            _lstAlerts.Items.Add($"ПРОСРОЧЕНО: {overdue.EmployeeName} — {overdue.PlannedDate:dd.MM.yyyy}");
        }

        if (_lstAlerts.Items.Count == 0)
        {
            _lstAlerts.Items.Add("Критичных событий не обнаружено.");
        }
    }

    private void LoadEmployees()
    {
        _employees.Clear();
        foreach (var employee in _service.SearchEmployees(_txtSearchEmployee.Text, _cmbFilterCompetency.SelectedItem?.ToString()))
        {
            _employees.Add(employee);
        }
        _employeesGrid.DataSource = _employees;
    }

    private Employee? SelectedEmployee() => _employeesGrid.CurrentRow?.DataBoundItem as Employee;
    private AttestationViewModel? SelectedAttestation() => _attestationGrid.CurrentRow?.DataBoundItem as AttestationViewModel;

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

        if (MessageBox.Show($"Удалить кандидата {selected.FullName}?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
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

        var model = new AttestationRecord
        {
            Id = selected.Id,
            EmployeeId = selected.EmployeeId,
            PlannedDate = selected.PlannedDate,
            CompletedDate = selected.CompletedDate,
            Status = selected.Status,
            Result = selected.Result,
            CommissionComment = selected.CommissionComment
        };

        using var form = new AttestationEditorForm(_service.GetEmployees().ToList(), model);
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

    private void CompleteAttestation()
    {
        var selected = SelectedAttestation();
        if (selected is null) return;

        _service.MarkAttestationCompleted(selected.Id, "Рекомендован", "Завершено через быстрый сценарий");
        ReloadAll();
    }

    private void GenerateSummary()
    {
        var path = _reportService.GenerateSummaryReport();
        _txtReportPreview.Text = $"Сводный отчёт сформирован:\n{path}";
        OpenPath(path);
    }

    private void ExportEmployees()
    {
        var path = _reportService.ExportEmployeesCsv();
        _txtReportPreview.Text = $"CSV по кадровому резерву сформирован:\n{path}";
        OpenPath(path);
    }

    private void ExportAttestations()
    {
        var path = _reportService.ExportAttestationsCsv();
        _txtReportPreview.Text = $"CSV по аттестациям сформирован:\n{path}";
        OpenPath(path);
    }

    private static void OpenPath(string path)
    {
        try
        {
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }
        catch
        {
            // Игнорируем ошибки открытия файла в средах без shell.
        }
    }
}
