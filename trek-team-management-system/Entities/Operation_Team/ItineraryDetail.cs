using DocumentFormat.OpenXml.Drawing.Diagrams;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TTH.Areas.Super.Data.Operation_Team
{
    public class ItineraryDetail
    {
        [Key]
        public int ItineraryId { get; set; }
        public string ItineraryDay { get; set; }
        public string ItineraryHeading { get; set; }
        public int TrekItineraryId { get; set; }

        public int TrekId { get; set; }

        public DateTime? ItineraryDate { get; set; }

        public TrekItinerary TrekItinerary { get; set; }
    }
}






