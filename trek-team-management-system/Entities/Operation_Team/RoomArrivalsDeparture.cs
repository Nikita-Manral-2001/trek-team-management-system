using System.ComponentModel.DataAnnotations.Schema;

namespace TTH.Areas.Super.Data.Operation_Team
{
    public class RoomArrivalsDeparture
    {
        public int? Id { get; set; }
        public DateTime? Date { get; set; }
        public int? Number { get; set; }
        public string? Vendor { get; set; }
        public int? BaseCampManagerId { get; set; }

        [ForeignKey("BaseCampManagerId")]
        public TrekBaseCampManager TrekBaseCampManager { get; set; }
    }
}
