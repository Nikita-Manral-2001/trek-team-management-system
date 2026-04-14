using System.ComponentModel.DataAnnotations.Schema;

namespace TTH.Areas.Super.Data.Operation_Team
{
    public class Mule
    {
        public int? Id { get; set; }
        public int? Number { get; set; }
        public int? Days { get; set; }
        public int BaseCampManagerId { get; set; }

        [ForeignKey("BaseCampManagerId")]
        public TrekBaseCampManager TrekBaseCampManager { get; set; }
    }
}
