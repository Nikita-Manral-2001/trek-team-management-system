using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TTH.Areas.Super.Data.Operation_Team
{
    public class CampManagmentRatingTL
    {
        [Key]
        public int Id { get; set; } // Primary Key

        [ForeignKey("TLForm")]
        public int TLFormId { get; set; } // Foreign Key referencing TLForm

  public int? toiletRating { get; set; }
        public int? DiningTentRating { get; set; }
        public int? KitchenTentRating { get; set; }
        public int? TableServingRating { get; set; }
        public int? hygieneRating { get; set; }

        // Navigation property
        public TLForm TLForm { get; set; }
    }
}
