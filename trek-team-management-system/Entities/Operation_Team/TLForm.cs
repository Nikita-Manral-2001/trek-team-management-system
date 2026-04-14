using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TTH.Areas.Super.Data.Operation_Team
{
    public class TLForm
    {
        [Key]
        public int Id { get; set; } // Primary Key
        public int? TeamId { get; set; }
        [ForeignKey("TeamId")]
        public AddedTeamMember AddedTeamMember { get; set; }
        public string? TrekName { get; set; }
        public int? TrekId { get; set; }
        public int? DepartureId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        public string? Email { get; set; }
        public string? TLBriefing { get; set; }
        public string? AmsCPRLecture { get; set; }
        public string? FloraFaunaLecture { get; set; }
        public string? SOSSignalLecture { get; set; }
        public string? TerminologyLecture { get; set; }
        public string? RandomTopicLecture { get; set; }
        public string? BPCheck { get; set; }
        public string? OxygenCheck { get; set; }
        public string? HealthFeedbackMorning { get; set; }
        public string? HealthFeedbackAfterDinner { get; set; }

        public string? Debriefing { get; set; }
        public string? OxygenCylinderAndRopeOnsummit { get; set; }
        public string? MealsAndEveningTea { get; set; }
        public string? CampCleanDrive { get; set; }
        public string? HotLunchServed { get; set; }
        public string? HotTeaServed { get; set; }



        public int? GReviewCount { get; set; }

        public string? HealthCheckupVandP { get; set; }
        public string? ExercisePandV { get; set; }

        public string? LecturePandV { get; set; }
        public string? WhileTrekkingPandV { get; set; }
        public string? SceneryPandV { get; set; }

        public string? FoodPandV { get; set; }
        public string? SubmitPandV { get; set; }
        public string? CampSitePandV { get; set; }

        public string? CampCleaningPandV { get; set; }

        public string? TrekkerActivityPandV { get; set; }
        public string? TextArea { get; set; }
        public string? InterviewVideo { get; set; }
        public List<TrekkersStatus> TrekkersStatus { get; set; } = new List<TrekkersStatus>();
        // Relationship: One TLForm can have multiple RentingDetails
        public ICollection<CampManagmentRatingTL> CampManagmentRatingTL { get; set; }


    }
}
