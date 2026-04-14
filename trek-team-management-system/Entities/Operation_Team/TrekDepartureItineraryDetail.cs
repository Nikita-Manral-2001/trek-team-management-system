using System.ComponentModel.DataAnnotations;

namespace TTH.Areas.Super.Data.Operation_Team
{
    public class TrekDepartureItineraryDetail
    {
        public int Id { get; set; }
        public int TrekId { get; set; }
        public string TrekName { get; set; }
        public string TrekShortName { get; set; }
        public int DepartureId { get; set; }
        public string Batch { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
       
        public ICollection<TrekDepartureItinerary> TrekDepartureItinerary { get; set; }


    }
}
