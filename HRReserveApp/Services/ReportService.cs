using System.Text;
using HRReserveApp.Models;

namespace HRReserveApp.Services;

public sealed class ReportService(ReserveService reserveService)
{
    private readonly string _reportDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        "HRReserveReports");

    public string GenerateSummaryReport()
    {
        Directory.CreateDirectory(_reportDirectory);
        var metrics = reserveService.GetMetrics();
        var upcoming = reserveService.GetUpcomingAttestations(30);
        var overdue = reserveService.GetOverdueAttestations();
        var byDepartment = reserveService.GetDepartmentStats();

        var builder = new StringBuilder();
        builder.AppendLine("Сводный аналитический отчёт по кадровому резерву");
        builder.AppendLine($"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}");
        builder.AppendLine(new string('-', 60));
        builder.AppendLine($"Всего сотрудников в резерве: {metrics.EmployeesTotal}");
        builder.AppendLine($"Активные аттестации: {metrics.ActiveAttestation}");
        builder.AppendLine($"Завершённые аттестации: {metrics.CompletedAttestation}");
        builder.AppendLine($"Рекомендованы к повышению: {metrics.RecommendedCount}");
        builder.AppendLine();

        builder.AppendLine("Статистика по подразделениям:");
        foreach (var row in byDepartment)
        {
            builder.AppendLine($"- {row.Department}: {row.CandidatesCount} чел.");
        }

        builder.AppendLine();
        builder.AppendLine($"Аттестации в ближайшие 30 дней: {upcoming.Count}");
        foreach (var item in upcoming)
        {
            builder.AppendLine($"  • {item.EmployeeName} — {item.PlannedDate:dd.MM.yyyy} ({item.Status})");
        }

        builder.AppendLine();
        builder.AppendLine($"Просроченные аттестации: {overdue.Count}");
        foreach (var item in overdue)
        {
            builder.AppendLine($"  • {item.EmployeeName} — план: {item.PlannedDate:dd.MM.yyyy}, статус: {item.Status}");
        }

        var file = Path.Combine(_reportDirectory, $"summary-{DateTime.Now:yyyyMMdd-HHmmss}.txt");
        File.WriteAllText(file, builder.ToString(), Encoding.UTF8);
        return file;
    }

    public string ExportEmployeesCsv()
    {
        Directory.CreateDirectory(_reportDirectory);
        var employees = reserveService.GetEmployees();

        var builder = new StringBuilder();
        builder.AppendLine("ФИО;Должность;Подразделение;Стаж;Уровень компетенций;Дата в резерве;План развития");
        foreach (var employee in employees)
        {
            builder.AppendLine(string.Join(';',
                Escape(employee.FullName),
                Escape(employee.Position),
                Escape(employee.Department),
                employee.ExperienceYears,
                Escape(employee.CompetencyLevel),
                employee.AddedToReserve.ToString("dd.MM.yyyy"),
                Escape(employee.DevelopmentPlan)));
        }

        var file = Path.Combine(_reportDirectory, $"employees-{DateTime.Now:yyyyMMdd-HHmmss}.csv");
        File.WriteAllText(file, builder.ToString(), Encoding.UTF8);
        return file;
    }

    public string ExportAttestationsCsv()
    {
        Directory.CreateDirectory(_reportDirectory);
        var attestations = reserveService.GetAttestations();

        var builder = new StringBuilder();
        builder.AppendLine("Сотрудник;Плановая дата;Дата завершения;Статус;Результат;Комментарий комиссии");
        foreach (var record in attestations)
        {
            builder.AppendLine(string.Join(';',
                Escape(record.EmployeeName),
                record.PlannedDate.ToString("dd.MM.yyyy"),
                record.CompletedDate?.ToString("dd.MM.yyyy") ?? string.Empty,
                Escape(record.Status),
                Escape(record.Result),
                Escape(record.CommissionComment)));
        }

        var file = Path.Combine(_reportDirectory, $"attestations-{DateTime.Now:yyyyMMdd-HHmmss}.csv");
        File.WriteAllText(file, builder.ToString(), Encoding.UTF8);
        return file;
    }

    private static string Escape(string value) => value.Replace(';', ',').Replace('\n', ' ');
}

public sealed record DepartmentStat(string Department, int CandidatesCount);
