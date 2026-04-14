namespace TTH.Areas.Super.Models.Operation_Team
{
    public class TrekAssignmentToEmplyeeViewModel
    {
        public List<string> EmployeeNames { get; set; }
        public List<string> EmployeesDesignation { get; set; }

        public List<string> TrekDates { get; set; }
        public Dictionary<string, Dictionary<string, string>> Assignments { get; set; }
    }
}
