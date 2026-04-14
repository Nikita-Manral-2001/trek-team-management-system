using System.ComponentModel.DataAnnotations;

namespace TTH.Areas.Super.Data.Operation_Team
{
    public class TrekDepartureItinerary
    {
        public int Id { get; set; }
       
        public int TrekId { get; set; }
        
        public string ItineraryDay { get; set; }

        [Required(ErrorMessage = "Itinerary Heading is required.")]
        public string ItineraryHeading { get; set; }
        public TrekDepartureItineraryDetail TrekDepartureItineraryDetail { get; set; }

    }
}
