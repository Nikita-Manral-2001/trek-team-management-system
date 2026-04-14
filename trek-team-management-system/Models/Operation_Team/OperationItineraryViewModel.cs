namespace TTH.Models
{
    public class OperationItineraryViewModel
    {
       public int TrekId { get; set; }
        public int ColumnNumber { get; set; } // Column number for the view
        public Dictionary<string, string> DatesWithHeadings { get; set; } // Map of dates and headings
        public List<int> DepartureId { get; set; }
        public string Batch { get; set; }
        public List<string> TrekName { get; set; }
        public List<string> AllTrekDates { get; set; }
public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
