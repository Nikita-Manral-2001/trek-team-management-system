using System.ComponentModel.DataAnnotations;

namespace TTH.Areas.Super.Data.Operation_Team
{
    public class AddedTeamMember
    {
        [Key]
        public int Id { get; set; }
        public int TrekId { get; set; }
        public int? DepartureId { get; set; }
        public string? Batch { get; set; }
        public string? Trekitinerary { get; set; }
        public string? Email { get; set; }
        public string TrekName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string TeamMemberName { get; set; }
        public string Designation { get; set; }
        public string? FreelancingField { get; set; } // Nullable field
        public string FormSubmission { get; set; }
        public string Approval { get; set; }
        public List<ChefFormRatings> ChefFormRatings { get; set; }
        public List<TLForm> TLForm { get; set; }
        public List<TrekBaseCampManager> TrekBaseCampManager { get; set; }
        public DateTime FormSubmissionDate { get; set; }
        public string? UserId { get; set; }

    }
}
