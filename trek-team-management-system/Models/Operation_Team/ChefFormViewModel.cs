using TTH.Areas.Super.Data.Operation_Team;

namespace TTH.Areas.Super.Models
{
    public class ChefFormViewModel
    {
        public AddedTeamMember TeamMember { get; set; }
        public string TrekName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int TrekId { get; set; }
        public int DepartureId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Batch { get; set; }
        public List<ChefFormDayRating> ChefformDayRating { get; set; } = new List<ChefFormDayRating>();
    }
}
