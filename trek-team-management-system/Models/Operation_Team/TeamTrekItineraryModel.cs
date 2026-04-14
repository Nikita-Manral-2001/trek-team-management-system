namespace TTH.Areas.Super.Models.Operation_Team
{
    public class TeamTrekItineraryModel
    {
        public int TrekId { get; set; }
        public string TrekName { get; set; }
        public DateTime? TrekDate { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string ItineraryType { get; set; }

        public List<TeamTrekItineraryDetailModel> ItineraryDetails { get; set; } = new List<TeamTrekItineraryDetailModel>();
    }
}
