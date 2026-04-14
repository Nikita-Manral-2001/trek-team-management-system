using System.ComponentModel.DataAnnotations;
using TTH.Areas.Super.Data.Operation_Team;

namespace TTH.Areas.Super.Models
{
    public class TLFormViewModel
    {
        public List<ParticipantNamesModel> Participants { get; set; }

        public string TrekName { get; set; }
        public string Designation { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int TrekId { get; set; }
        public int DepartureId { get; set; }
        public string Email { get; set; }
        public string id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string TextArea { get; set; }

        [Required(ErrorMessage = "Please select an option")]
        public string TLBriefing { get; set; }

        [Required(ErrorMessage = "Please select an option")]
        public string AmsCPRLecture { get; set; }

        [Required(ErrorMessage = "Please select an option")]
        public string FloraFaunaLecture { get; set; }

        [Required(ErrorMessage = "Please select an option")]
        public string SOSSignalLecture { get; set; }

        [Required(ErrorMessage = "Please select an option")]
        public string TerminologyLecture { get; set; }

        [Required(ErrorMessage = "Please select an option")]
        public string RandomTopicLecture { get; set; }

        [Required(ErrorMessage = "Please select an option")]
        public string BPCheck { get; set; }

        [Required(ErrorMessage = "Please select an option")]
        public string OxygenCheck { get; set; }

        [Required(ErrorMessage = "Please select an option")]
        public string HealthFeedbackMorning { get; set; }

        [Required(ErrorMessage = "Please select an option")]
        public string HealthFeedbackAfterDinner { get; set; }

        [Required(ErrorMessage = "Please select an option")]
        public string Debriefing { get; set; }

        [Required(ErrorMessage = "Please select an option")]
        public string OxygenCylinderAndRopeOnsummit { get; set; }

        [Required(ErrorMessage = "Please select an option")]
        public string MealsAndEveningTea { get; set; }

        [Required(ErrorMessage = "Please select an option")]
        public string CampCleanDrive { get; set; }

        [Required(ErrorMessage = "Please select an option")]
        public string HotLunchServed { get; set; }

        [Required(ErrorMessage = "Please select an option")]
        public string HotTeaServed { get; set; }

        [Required(ErrorMessage = "Please enter Google Review Count")]
        public int GReviewCount { get; set; }

        public string? GRApprovel { get; set; }

        [Required(ErrorMessage = "Please select an option")]
        public string HealthCheckupVandP { get; set; }

        [Required(ErrorMessage = "Please select an option")]
        public string ExercisePandV { get; set; }

        [Required(ErrorMessage = "Please select an option")]
        public string LecturePandV { get; set; }

        [Required(ErrorMessage = "Please select an option")]
        public string WhileTrekkingPandV { get; set; }

        [Required(ErrorMessage = "Please select an option")]
        public string SceneryPandV { get; set; }

        [Required(ErrorMessage = "Please select an option")]
        public string FoodPandV { get; set; }

        [Required(ErrorMessage = "Please select an option")]
        public string SubmitPandV { get; set; }

        [Required(ErrorMessage = "Please select an option")]
        public string CampSitePandV { get; set; }

        [Required(ErrorMessage = "Please select an option")]
        public string CampCleaningPandV { get; set; }

        [Required(ErrorMessage = "Please select an option")]
        public string TrekkerActivityPandV { get; set; }

        [Required(ErrorMessage = "Please select an option")]
        public string InterviewVideo { get; set; }

        public List<TrekkersStatus> TrekkersStatus { get; set; } = new List<TrekkersStatus>();
        public List<CampManagmentRatingTL> CampManagmentRatingTL { get; set; }
    }
}

