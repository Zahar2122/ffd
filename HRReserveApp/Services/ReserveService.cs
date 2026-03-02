using HRReserveApp.Data;
using HRReserveApp.Models;

namespace HRReserveApp.Services;

public sealed class ReserveService(AppDataStore dataStore)
{
    public IReadOnlyList<Employee> GetEmployees() => dataStore.Load().Employees.OrderBy(x => x.FullName).ToList();

    public IReadOnlyList<Employee> SearchEmployees(string query, string? competency = null)
    {
        var items = dataStore.Load().Employees.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var q = query.Trim();
            items = items.Where(x =>
                x.FullName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                x.Position.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                x.Department.Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(competency) && competency != "Все")
        {
            items = items.Where(x => x.CompetencyLevel == competency);
        }

        return items.OrderBy(x => x.FullName).ToList();
    }

    public IReadOnlyList<AttestationViewModel> GetAttestations()
    {
        var data = dataStore.Load();

        return data.Attestations
            .Select(a =>
            {
                var emp = data.Employees.FirstOrDefault(e => e.Id == a.EmployeeId);
                return new AttestationViewModel
                {
                    Id = a.Id,
                    EmployeeId = a.EmployeeId,
                    EmployeeName = emp?.FullName ?? "Неизвестный сотрудник",
                    PlannedDate = a.PlannedDate,
                    CompletedDate = a.CompletedDate,
                    Status = a.Status,
                    Result = a.Result,
                    CommissionComment = a.CommissionComment
                };
            })
            .OrderBy(a => a.PlannedDate)
            .ToList();
    }

    public IReadOnlyList<AttestationViewModel> GetUpcomingAttestations(int days)
    {
        var end = DateTime.Today.AddDays(days);
        return GetAttestations()
            .Where(x => x.PlannedDate.Date >= DateTime.Today && x.PlannedDate.Date <= end)
            .ToList();
    }

    public IReadOnlyList<AttestationViewModel> GetOverdueAttestations()
        => GetAttestations()
            .Where(x => x.PlannedDate.Date < DateTime.Today && x.Status != "Завершена")
            .ToList();

    public IReadOnlyList<DepartmentStat> GetDepartmentStats()
        => dataStore.Load().Employees
            .GroupBy(x => string.IsNullOrWhiteSpace(x.Department) ? "Без подразделения" : x.Department)
            .Select(x => new DepartmentStat(x.Key, x.Count()))
            .OrderByDescending(x => x.CandidatesCount)
            .ToList();

    public void AddEmployee(Employee employee)
    {
        var data = dataStore.Load();
        data.Employees.Add(employee);
        dataStore.Save(data);
    }

    public void UpdateEmployee(Employee employee)
    {
        var data = dataStore.Load();
        var item = data.Employees.FirstOrDefault(x => x.Id == employee.Id);
        if (item is null) return;

        item.FullName = employee.FullName;
        item.Position = employee.Position;
        item.Department = employee.Department;
        item.ExperienceYears = employee.ExperienceYears;
        item.AddedToReserve = employee.AddedToReserve;
        item.CompetencyLevel = employee.CompetencyLevel;
        item.DevelopmentPlan = employee.DevelopmentPlan;

        dataStore.Save(data);
    }

    public void DeleteEmployee(Guid id)
    {
        var data = dataStore.Load();
        data.Employees.RemoveAll(x => x.Id == id);
        data.Attestations.RemoveAll(x => x.EmployeeId == id);
        dataStore.Save(data);
    }

    public void AddAttestation(AttestationRecord record)
    {
        var data = dataStore.Load();
        data.Attestations.Add(record);
        dataStore.Save(data);
    }

    public void UpdateAttestation(AttestationRecord record)
    {
        var data = dataStore.Load();
        var item = data.Attestations.FirstOrDefault(x => x.Id == record.Id);
        if (item is null) return;

        item.EmployeeId = record.EmployeeId;
        item.PlannedDate = record.PlannedDate;
        item.CompletedDate = record.CompletedDate;
        item.Status = record.Status;
        item.Result = record.Result;
        item.CommissionComment = record.CommissionComment;

        dataStore.Save(data);
    }

    public void DeleteAttestation(Guid id)
    {
        var data = dataStore.Load();
        data.Attestations.RemoveAll(x => x.Id == id);
        dataStore.Save(data);
    }

    public void MarkAttestationCompleted(Guid id, string result, string comment)
    {
        var data = dataStore.Load();
        var item = data.Attestations.FirstOrDefault(x => x.Id == id);
        if (item is null) return;

        item.Status = "Завершена";
        item.CompletedDate = DateTime.Today;
        item.Result = result;
        item.CommissionComment = comment;

        dataStore.Save(data);
    }

    public DashboardMetrics GetMetrics()
    {
        var data = dataStore.Load();

        var total = data.Employees.Count;
        var activeAttestation = data.Attestations.Count(x => x.Status is "Запланирована" or "В процессе");
        var completed = data.Attestations.Count(x => x.Status == "Завершена");
        var recommended = data.Attestations.Count(x => x.Result == "Рекомендован");
        var overdue = data.Attestations.Count(x => x.PlannedDate.Date < DateTime.Today && x.Status != "Завершена");

        return new DashboardMetrics(total, activeAttestation, completed, recommended, overdue);
    }
}

public sealed class AttestationViewModel
{
    public Guid Id { get; init; }
    public Guid EmployeeId { get; init; }
    public string EmployeeName { get; init; } = string.Empty;
    public DateTime PlannedDate { get; init; }
    public DateTime? CompletedDate { get; init; }
    public string Status { get; init; } = string.Empty;
    public string Result { get; init; } = string.Empty;
    public string CommissionComment { get; init; } = string.Empty;
}

public sealed record DashboardMetrics(int EmployeesTotal, int ActiveAttestation, int CompletedAttestation, int RecommendedCount, int OverdueAttestation);
