using TTH.Areas.Super.Data.Operation_Team;

namespace TTH.Areas.Super.Models
{
    public class BaseCampManagerViewModel
    {
      
       
        public List<DateTime> AvailableDates { get; set; }
        public string Designation { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string TrekName { get; set; }
        public int TrekId { get; set; }
        public int DepartureId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Batch { get; set; }
        public string TeaxtArea { get; set; }
        public string Email { get; set; }
        public string id { get; set; }
        // Trekker details
        public int MaleTrekker { get; set; }
        public int FemaleTrekker { get; set; }

        // Freelancer helpers
        public List<HelperFreelancer> HelperFreelancers { get; set; } = new List<HelperFreelancer>();
        public List<GuideFreelancer> GuideFreelancer { get; set; } = new List<GuideFreelancer>();
        public List<Mule> Mules { get; set; } = new List<Mule>();
        public List<Porter> Porters { get; set; } = new List<Porter>();

        // Transport and Room details
        public List<MainTransportArrival> MainTransportArrivals { get; set; } = new List<MainTransportArrival>();
        public List<MainTransportArrivalDeparture> MainTransportDepartures { get; set; } = new List<MainTransportArrivalDeparture>();
        public List<LocalTransportArrival> LocalTransportArrivals { get; set; } = new List<LocalTransportArrival>();
        public List<LocalTransportArrivalDeparture> LocalTransportDepartures { get; set; } = new List<LocalTransportArrivalDeparture>();
        public List<RoomArrival> RoomArrivals { get; set; } = new List<RoomArrival>();
        public List<RoomArrivalsDeparture> RoomArrivalsDepartures { get; set; } = new List<RoomArrivalsDeparture>();

        // Trekker Status
       

        public int? OnlineOffloadMode { get; set; }
        public int? OflineOffloadMode { get; set; }
    }
}
