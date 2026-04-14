using Org.BouncyCastle.Bcpg;
using System.ComponentModel.DataAnnotations.Schema;
using TTH.Models.booking;

namespace TTH.Areas.Super.Data.Operation_Team
{
    public class TrekkersStatus
    {
        public int Id { get; set; }
       
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string TrekkerStatus { get; set; }
        public int TLFormId { get; set; }

        [ForeignKey(nameof(TempParticipant))] // ✅ Reference the navigation property
        public int? ParticipantId { get; set; }
        public TempParticipantModel TempParticipant { get; set; }


        [ForeignKey("BaseCampManagerId")]
        public TLForm TLForm { get; set; }
    }
}
