using HRReserveApp.Data;
using HRReserveApp.Models;

namespace HRReserveApp.Services;

public sealed class ReserveService(AppDataStore dataStore)
{
    public IReadOnlyList<Employee> GetEmployees() => dataStore.Load().Employees.OrderBy(x => x.FullName).ToList();

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

    public DashboardMetrics GetMetrics()
    {
        var data = dataStore.Load();

        var total = data.Employees.Count;
        var activeAttestation = data.Attestations.Count(x => x.Status == "Запланирована" || x.Status == "В процессе");
        var completed = data.Attestations.Count(x => x.Status == "Завершена");
        var recommended = data.Attestations.Count(x => x.Result == "Рекомендован");

        return new DashboardMetrics(total, activeAttestation, completed, recommended);
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

public sealed record DashboardMetrics(int EmployeesTotal, int ActiveAttestation, int CompletedAttestation, int RecommendedCount);
