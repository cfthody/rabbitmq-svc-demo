namespace SVC.Core;


public  record EmployeeRequest(string Name, long RoleId, long PlatoonId)
{
    public DateTime EntryDate { get; } = DateTime.Today;
};