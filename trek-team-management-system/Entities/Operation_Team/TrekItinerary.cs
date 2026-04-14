using TTH.Areas.Super.Data.Operation_Team;

public class TrekItinerary
{
    public int Id { get; set; }
    public int TrekId { get; set; }
    public string TrekName { get; set; }
    public string TrekShortName { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? FromDate { get; set; }
    public string? ToDate { get; set; }
    public string ItineraryType { get; set; }
    public int ItineraryDayCount { get; set; }
    public ICollection<ItineraryDetail> ItineraryDetails { get; set; }
}
