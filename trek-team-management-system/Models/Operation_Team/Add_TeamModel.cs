using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using TTH.Areas.Super.Data.Rent;
using TTH.Areas.Super.Models.Rent;

namespace TTH.Areas.Super.Models.Operation_Team
{
    public class Add_TeamModel
    {
        [Key]
        public int Id { get; set; }
        
       
        public string? Name { get; set; }

        public int PhoneNo { get; set; }

        public string? Email { get; set; }
        public DateOnly? DateOfJoining { get; set; }
        public string? Cource { get; set; }
        public string? Education { get; set; }
        public DateOnly? DOB { get; set; }
        public string? Position { get; set; }
        public string? Gender { get; set; }


        [NotMapped]
        public IFormFile? Photo { get; set; }
        public string? PhotoPath { get; set; }


    }
}
