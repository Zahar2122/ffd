using HRReserveApp.Models;

namespace HRReserveApp.Forms;

public sealed class EmployeeEditorForm : Form
{
    private readonly TextBox _txtName = new();
    private readonly TextBox _txtPosition = new();
    private readonly TextBox _txtDepartment = new();
    private readonly NumericUpDown _numExperience = new();
    private readonly DateTimePicker _dtAdded = new();
    private readonly ComboBox _cmbCompetency = new();
    private readonly TextBox _txtPlan = new();
    private readonly Guid _id;

    public Employee? Employee { get; private set; }

    public EmployeeEditorForm(Employee? source = null)
    {
        _id = source?.Id ?? Guid.NewGuid();

        Text = source is null ? "Добавление кандидата" : "Редактирование кандидата";
        Width = 560;
        Height = 520;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        var y = 20;
        Controls.Add(CreateLabel("ФИО", y));
        _txtName.SetBounds(20, y += 22, 500, 30);
        Controls.Add(_txtName);

        Controls.Add(CreateLabel("Должность", y += 40));
        _txtPosition.SetBounds(20, y += 22, 500, 30);
        Controls.Add(_txtPosition);

        Controls.Add(CreateLabel("Подразделение", y += 40));
        _txtDepartment.SetBounds(20, y += 22, 500, 30);
        Controls.Add(_txtDepartment);

        Controls.Add(CreateLabel("Стаж (лет)", y += 40));
        _numExperience.SetBounds(20, y += 22, 120, 30);
        _numExperience.Maximum = 60;
        Controls.Add(_numExperience);

        Controls.Add(CreateLabel("Дата включения в резерв", y += 40));
        _dtAdded.SetBounds(20, y += 22, 220, 30);
        _dtAdded.Format = DateTimePickerFormat.Short;
        Controls.Add(_dtAdded);

        Controls.Add(CreateLabel("Уровень компетенций", y += 40));
        _cmbCompetency.SetBounds(20, y += 22, 220, 30);
        _cmbCompetency.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbCompetency.Items.AddRange(["Базовый", "Средний", "Высокий"]);
        Controls.Add(_cmbCompetency);

        Controls.Add(CreateLabel("План развития", y += 40));
        _txtPlan.SetBounds(20, y += 22, 500, 60);
        _txtPlan.Multiline = true;
        Controls.Add(_txtPlan);

        var btnSave = new Button { Text = "Сохранить", Left = 390, Top = 420, Width = 130, Height = 34 };
        btnSave.Click += (_, _) => SaveEmployee();
        Controls.Add(btnSave);

        if (source is not null)
        {
            _txtName.Text = source.FullName;
            _txtPosition.Text = source.Position;
            _txtDepartment.Text = source.Department;
            _numExperience.Value = source.ExperienceYears;
            _dtAdded.Value = source.AddedToReserve;
            _cmbCompetency.SelectedItem = source.CompetencyLevel;
            _txtPlan.Text = source.DevelopmentPlan;
        }
        else
        {
            _cmbCompetency.SelectedIndex = 1;
        }
    }

    private static Label CreateLabel(string text, int top)
        => new() { Text = text, Left = 20, Top = top, Width = 300 };

    private void SaveEmployee()
    {
        if (string.IsNullOrWhiteSpace(_txtName.Text) || string.IsNullOrWhiteSpace(_txtPosition.Text))
        {
            MessageBox.Show("Заполните обязательные поля: ФИО и должность.");
            return;
        }

        Employee = new Employee
        {
            Id = _id,
            FullName = _txtName.Text.Trim(),
            Position = _txtPosition.Text.Trim(),
            Department = _txtDepartment.Text.Trim(),
            ExperienceYears = (int)_numExperience.Value,
            AddedToReserve = _dtAdded.Value.Date,
            CompetencyLevel = _cmbCompetency.SelectedItem?.ToString() ?? "Средний",
            DevelopmentPlan = _txtPlan.Text.Trim()
        };

        DialogResult = DialogResult.OK;
        Close();
    }
}
