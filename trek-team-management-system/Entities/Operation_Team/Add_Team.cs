using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TTH.Areas.Super.Data.Operation_Team
{
    public class Add_Team
    {
        [Key]
        public int Id { get; set; }
        public string EmpId { get; set; }

        public string? Name { get; set; }
        [Required]
        [StringLength(15, ErrorMessage = "Phone number cannot exceed 15 characters.")]
        public string? PhoneNo { get; set; }

        public string? Email { get; set; }
        public DateOnly? DateOfJoining { get; set; }
        public string? Cource { get; set; }
        public string? Education { get; set; }
        public DateOnly? DOB { get; set; }
        public string? Position { get; set; }
        public string? Location { get; set; }
        public string? Gender { get; set; }
        [NotMapped]
        public IEnumerable<IFormFile>? Upload_Id { get; set; } 
        public string? UploadIdPaths { get; set; } 
        [NotMapped]
        public IFormFile? Photo { get; set; }
        public string? PhotoPath { get; set; }
        
    }
}
