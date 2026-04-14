using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TTH.Areas.Super.Data.Operation_Team
{
    public class HelperFreelancer
    {
        [Key]
        public int Id { get; set; }

        public string? Name { get; set; }
        public string? Vendor { get; set; }

        public int? Days { get; set; }

        // Foreign Key to BaseCampManager
        public int BaseCampManagerId { get; set; }

        [ForeignKey("BaseCampManagerId")]
        public TrekBaseCampManager TrekBaseCampManager { get; set; }
    }
}
