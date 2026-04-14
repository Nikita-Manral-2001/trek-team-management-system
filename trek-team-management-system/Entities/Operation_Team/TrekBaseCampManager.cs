using SixLabors.ImageSharp;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TTH.Areas.Super.Data.Operation_Team
{
    public class TrekBaseCampManager
    {

        [Key]
        public int Id { get; set; }
        public int? TeamId { get; set; }
        [ForeignKey("TeamId")]
        public AddedTeamMember AddedTeamMember { get; set; }
        public string TrekName { get; set; }
        public int TrekId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int DepartureId { get; set; }
        public string? Batch { get; set; }

        // Trekker details
        public int? MaleTrekker { get; set; }
        public int? FemaleTrekker { get; set; }
        public string? TeaxtArea { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        // Freelancer helpers
        public List<HelperFreelancer> HelperFreelancers { get; set; }
        public List<GuideFreelancer> GuideFreelancers { get; set; }
        public List<Mule> Mules { get; set; }
        public List<Porter> Porters { get; set; }
        public List<MainTransportArrival> MainTransportArrival { get; set; }
        public List<MainTransportArrivalDeparture> MainTransportArrivalDeparture { get; set; }
        public List<LocalTransportArrival> LocalTransportArrival { get; set; }
        public List<LocalTransportArrivalDeparture> LocalTransportArrivalDeparture { get; set; }

        // Room and Transport details
        public List<RoomArrival> RoomArrivals { get; set; }
        public List<RoomArrivalsDeparture> RoomArrivalsDepartures { get; set; }
        public List<TrekkersStatus> TrekkersStatus { get; set; }

        public int? OnlineOffloadMode { get; set; }
        public int? OflineOffloadMode { get; set; }

    }
}
