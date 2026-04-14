using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TTH.Areas.Super.Data.Operation_Team
{
    public class ChefFormRatings
    {
        [Key]
        public int Id { get; set; }
        public int? TeamId { get; set; }
        [ForeignKey("TeamId")]
        public AddedTeamMember AddedTeamMember { get; set; }
        public string? Email { get; set; }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public int? TrekId { get; set; }  // Foreign key to identify the trek
        public int? DepartureId { get; set; }
        public string? TrekName { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string? ChefName { get; set; }
        public string? CampDays { get; set; }
        public int? TimingRating { get; set; }
        public int? HygieneRating { get; set; }
        public int? TasteRating { get; set; }
        public int? AsPerMenuRating { get; set; }
        public int? InvolvementRating { get; set; }

    }
}
