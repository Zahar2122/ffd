using HRReserveApp.Models;

namespace HRReserveApp.Forms;

public sealed class AttestationEditorForm : Form
{
    private readonly ComboBox _cmbEmployee = new();
    private readonly DateTimePicker _dtPlanned = new();
    private readonly CheckBox _chkCompleted = new();
    private readonly DateTimePicker _dtCompleted = new();
    private readonly ComboBox _cmbStatus = new();
    private readonly ComboBox _cmbResult = new();
    private readonly TextBox _txtComment = new();
    private readonly List<Employee> _employees;
    private readonly Guid _id;

    public AttestationRecord? Record { get; private set; }

    public AttestationEditorForm(List<Employee> employees, AttestationRecord? source = null)
    {
        _employees = employees;
        _id = source?.Id ?? Guid.NewGuid();

        Text = source is null ? "Назначение аттестации" : "Редактирование аттестации";
        Width = 560;
        Height = 500;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        var y = 20;
        Controls.Add(CreateLabel("Сотрудник", y));
        _cmbEmployee.SetBounds(20, y += 22, 500, 30);
        _cmbEmployee.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbEmployee.DataSource = _employees;
        _cmbEmployee.DisplayMember = nameof(Employee.FullName);
        Controls.Add(_cmbEmployee);

        Controls.Add(CreateLabel("Плановая дата", y += 40));
        _dtPlanned.SetBounds(20, y += 22, 220, 30);
        _dtPlanned.Format = DateTimePickerFormat.Short;
        Controls.Add(_dtPlanned);

        _chkCompleted.Text = "Аттестация завершена";
        _chkCompleted.SetBounds(20, y += 42, 220, 24);
        _chkCompleted.CheckedChanged += (_, _) => _dtCompleted.Enabled = _chkCompleted.Checked;
        Controls.Add(_chkCompleted);

        _dtCompleted.SetBounds(20, y += 24, 220, 30);
        _dtCompleted.Format = DateTimePickerFormat.Short;
        _dtCompleted.Enabled = false;
        Controls.Add(_dtCompleted);

        Controls.Add(CreateLabel("Статус", y += 40));
        _cmbStatus.SetBounds(20, y += 22, 220, 30);
        _cmbStatus.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbStatus.Items.AddRange(["Запланирована", "В процессе", "Завершена"]);
        Controls.Add(_cmbStatus);

        Controls.Add(CreateLabel("Результат", y += 40));
        _cmbResult.SetBounds(20, y += 22, 220, 30);
        _cmbResult.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbResult.Items.AddRange(["Ожидается", "Рекомендован", "Нуждается в развитии", "Не рекомендован"]);
        Controls.Add(_cmbResult);

        Controls.Add(CreateLabel("Комментарий комиссии", y += 40));
        _txtComment.SetBounds(20, y += 22, 500, 70);
        _txtComment.Multiline = true;
        Controls.Add(_txtComment);

        var btnSave = new Button { Text = "Сохранить", Left = 390, Top = 410, Width = 130, Height = 34 };
        btnSave.Click += (_, _) => SaveRecord();
        Controls.Add(btnSave);

        if (source is not null)
        {
            var idx = _employees.FindIndex(x => x.Id == source.EmployeeId);
            if (idx >= 0) _cmbEmployee.SelectedIndex = idx;
            _dtPlanned.Value = source.PlannedDate;
            _chkCompleted.Checked = source.CompletedDate is not null;
            if (source.CompletedDate is not null) _dtCompleted.Value = source.CompletedDate.Value;
            _cmbStatus.SelectedItem = source.Status;
            _cmbResult.SelectedItem = source.Result;
            _txtComment.Text = source.CommissionComment;
        }
        else
        {
            _cmbStatus.SelectedIndex = 0;
            _cmbResult.SelectedIndex = 0;
        }
    }

    private static Label CreateLabel(string text, int top)
        => new() { Text = text, Left = 20, Top = top, Width = 300 };

    private void SaveRecord()
    {
        if (_cmbEmployee.SelectedItem is not Employee employee)
        {
            MessageBox.Show("Выберите сотрудника.");
            return;
        }

        Record = new AttestationRecord
        {
            Id = _id,
            EmployeeId = employee.Id,
            PlannedDate = _dtPlanned.Value.Date,
            CompletedDate = _chkCompleted.Checked ? _dtCompleted.Value.Date : null,
            Status = _cmbStatus.SelectedItem?.ToString() ?? "Запланирована",
            Result = _cmbResult.SelectedItem?.ToString() ?? "Ожидается",
            CommissionComment = _txtComment.Text.Trim()
        };

        DialogResult = DialogResult.OK;
        Close();
    }
}
