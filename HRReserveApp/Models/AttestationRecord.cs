namespace HRReserveApp.Models;

public sealed class AttestationRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EmployeeId { get; set; }
    public DateTime PlannedDate { get; set; } = DateTime.Today;
    public DateTime? CompletedDate { get; set; }
    public string Status { get; set; } = "Запланирована";
    public string Result { get; set; } = "Ожидается";
    public string CommissionComment { get; set; } = string.Empty;
}
