using System.Text.Json;
using HRReserveApp.Models;

namespace HRReserveApp.Data;

public sealed class AppDataStore
{
    private readonly string _dbPath;

    public AppDataStore()
    {
        var appDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "HRReserveDiploma");

        Directory.CreateDirectory(appDirectory);
        _dbPath = Path.Combine(appDirectory, "app-data.json");
    }

    public AppData Load()
    {
        if (!File.Exists(_dbPath))
        {
            var initial = AppData.CreateSeed();
            Save(initial);
            return initial;
        }

        var json = File.ReadAllText(_dbPath);
        return JsonSerializer.Deserialize<AppData>(json) ?? AppData.CreateSeed();
    }

    public void Save(AppData data)
    {
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(_dbPath, json);
    }
}

public sealed class AppData
{
    public List<Employee> Employees { get; init; } = [];
    public List<AttestationRecord> Attestations { get; init; } = [];
    public List<AppUser> Users { get; init; } = [];

    public static AppData CreateSeed()
    {
        var user = new AppUser
        {
            Login = "admin",
            Password = "admin123",
            DisplayName = "Администратор школы",
            Role = "Кадровый специалист"
        };

        var employee = new Employee
        {
            FullName = "Иванова Мария Петровна",
            Position = "Учитель математики",
            Department = "Методическое объединение естественных наук",
            ExperienceYears = 7,
            AddedToReserve = DateTime.Today.AddMonths(-2),
            CompetencyLevel = "Высокий",
            DevelopmentPlan = "Подготовка к должности заместителя директора по УВР"
        };

        var attestation = new AttestationRecord
        {
            EmployeeId = employee.Id,
            PlannedDate = DateTime.Today.AddDays(30),
            Status = "Запланирована",
            Result = "Ожидается",
            CommissionComment = "Назначить наставника и сформировать пакет документов"
        };

        return new AppData
        {
            Users = [user],
            Employees = [employee],
            Attestations = [attestation]
        };
    }
}
