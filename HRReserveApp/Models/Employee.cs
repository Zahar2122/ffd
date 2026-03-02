namespace HRReserveApp.Models;

public sealed class Employee
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FullName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public int ExperienceYears { get; set; }
    public DateTime AddedToReserve { get; set; } = DateTime.Today;
    public string CompetencyLevel { get; set; } = "Средний";
    public string DevelopmentPlan { get; set; } = string.Empty;
}
