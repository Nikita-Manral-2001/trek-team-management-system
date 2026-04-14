using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2021.DocumentTasks;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.X509;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using TTH.Areas.Super.Data;
using TTH.Areas.Super.Data.Operation_Team;
using TTH.Areas.Super.Models.Operation_Team;
using TTH.Areas.Super.Models;
using TTH.Data;
using TTH.Migrations;
using TTH.Models;
using TTH.Models.home;

namespace TTH.Controllers
{

    [Area("super")]
    [Route("super/[controller]")]
    [Authorize(Roles = "Super,TLM,OperationTeam,TrekLeader,SEO")]
    public class ViewOperationTeamDetailsController : Controller
    {
        private readonly AppDataContext _context;

        public ViewOperationTeamDetailsController(AppDataContext context)
        {
            _context = context;
        }



        [HttpGet]
        [Route("GetDateWiseTreks")]
        public IActionResult GetDateWiseTreks(DateTime FromDate, DateTime ToDate)
        {
            var DeparturetTrekIds = _context.TrekDeparture
          .Where(t => t.StartDate >= FromDate && t.StartDate <= ToDate)
          .Select(t => new
          {
              t.TrekId,

          })
          .ToList();

            var TreksIds = DeparturetTrekIds.Select(d => d.TrekId).Distinct().ToList();
            var trekList = _context.TrekDetails
   .Where(t => TreksIds.Contains(t.TrekId))
   .OrderBy(t => t.TrekName)
   .Select(t => new
   {
       Value = t.TrekId,
       Text = t.TrekName
   })
   .ToList();

            return Ok(trekList);
        }


        [HttpGet]
        [Route("DepartureWise")]
        public IActionResult DepartureWise()

        {
            List<SelectListItem> trekList = new List<SelectListItem>();

            var ListofDepartureIds = new List<int>();
            var ListofTrekIds = new List<int>();
            var ListOfStartDate = new List<DateTime>();


            // Fetch trek details for the dropdown

            var DeparturetTrekIds = _context.TrekDeparture

    .Select(t => new
    {
        t.TrekId,

    })
    .ToList();

            var TreksIds = DeparturetTrekIds.Select(d => d.TrekId).Distinct().ToList();
            trekList = _context.TrekDetails
  .Where(t => TreksIds.Contains(t.TrekId))
  .OrderBy(t => t.TrekName)
  .Select(t => new SelectListItem
  {
      Value = t.TrekId.ToString(), // Ensures TrekId is converted to string as required by SelectListItem
      Text = t.TrekName // Sets the display text
  })
  .ToList();

            ViewData["TrekList"] = trekList;

            // Get By Default One Month Data 

            var today = DateTime.Today;
            var oneMonthLater = today.AddMonths(1);
            var AllTrekDates = new HashSet<string>();



            var TrekItineraryColumns = new List<Dictionary<string, string>>();


            var trekDetails = _context.TrekItineraries
    .Include(t => t.ItineraryDetails) // Eager loading to fetch related ItineraryDetails
    .Select(t => new
    {
        t.TrekId,
        t.StartDate,
        t.EndDate,
        t.Id,

        t.ItineraryType,
        ItineraryDetails = t.ItineraryDetails // Directly use navigation property
            .Select(d => new
            {
                d.ItineraryDay,
                d.ItineraryHeading,
                d.TrekId,

            })
            .ToList(),
        DaysCount = t.ItineraryDetails.Count() // Avoid extra DB call
    })
    .ToList();

            var fixedTrekDetails = trekDetails.Where(t => t.ItineraryType == "Fixed").ToList();
            var customizeTrekDetails = trekDetails.Where(t => t.ItineraryType == "Customize").ToList();




            var FixedTrekIdsdetails = trekDetails
.Where(t => t.ItineraryType == "Fixed")
.Select(t => t.TrekId)
.ToList();

            // Step 1: Get the TrekId and Id from the TrekItinerary table where ItineraryType is "Fixed"
            var FixedTrekIdsdetailsId = trekDetails
 .Where(t => t.ItineraryType == "Fixed")
 .Select(t => t.Id)
 .ToList();
            var AllTrekItineraries = _context.TrekDepartureItineraryDetail
                    .Where(td => FixedTrekIdsdetails.Contains(td.TrekId))
                    .Select(d => new
                    {
                        d.TrekId,
                        d.FromDate,
                        d.ToDate,
                        d.Id,
                        d.Batch,

                        ItineraryDetails = _context.TrekDepartureItinerary
                             .Where(td => EF.Property<int>(td, "TrekDepartureItineraryDetailId") == d.Id)
                            .Select(td => new
                            {
                                td.ItineraryDay,
                                td.ItineraryHeading,
                                td.TrekId

                            })
                            .ToList(),
                        DaysCount = _context.TrekDepartureItinerary
                             .Where(td => EF.Property<int>(td, "TrekDepartureItineraryDetailId") == d.Id)
                            .Count()
                    })
                    .ToList();
            var itineraryDetails = _context.ItineraryDetails
                 .Where(d => FixedTrekIdsdetailsId.Contains(d.TrekItineraryId))
                 .Select(d => new
                 {
                     d.ItineraryHeading,
                     d.ItineraryDay,
                     d.TrekId
                 })
                 .ToList();
            var trekIds = _context.TrekItineraries

                .OrderBy(td => td.TrekName) // Sort alphabetically
                .Select(td => td.TrekId)
                .Distinct() // Ensure unique TrekIds
                .ToList();

            var startDates = _context.TrekDeparture
     .Where(td => trekIds.Contains(td.TrekId) &&
                 (td.HideFrom == "none" || td.HideFrom == "website"))
     .OrderBy(td => td.TrekId)
     .Select(td => new
     {
         td.DepartureId,
         td.Batch,
         td.StartDate,
         td.TrekId,

     })
     .ToList();











            DateTime Today = DateTime.Today;
            var AllsevenDaysAgo = Today.AddDays(-7);
            var AllCoustomizeFromDates = customizeTrekDetails
                  .Where(fixedDate => fixedDate.StartDate >= AllsevenDaysAgo && fixedDate.StartDate < today)
                  .ToList();



            var afterAllCustomizeFromDates = customizeTrekDetails
                .Where(trekDetailsItinerary => trekDetailsItinerary.StartDate >= today && trekDetailsItinerary.StartDate <= oneMonthLater)
                .OrderBy(trekDetailsItinerary => trekDetailsItinerary.StartDate) // Sort by date (earliest first)
                .ToList();

            // Combine both lists
            var combinedAllCustomizeFromDates = AllCoustomizeFromDates.Concat(afterAllCustomizeFromDates).OrderBy(item => item.StartDate).ToList();




            var AllFixedFromDates = startDates
           .Where(fixedDate => fixedDate.StartDate >= AllsevenDaysAgo && fixedDate.StartDate < today)
           .ToList();

            var afterAllFixedFromDates = startDates
                    .Where(trekDetailsItinerary => trekDetailsItinerary.StartDate >= today && trekDetailsItinerary.StartDate <= oneMonthLater)
                    //.OrderBy(trekDetailsItinerary => trekDetailsItinerary.StartDate) // Sort by date (earliest first)
                    .ToList();

            // Combine both lists
            //var combinedAllFixedFromDates = AllFixedFromDates.Concat(afterAllFixedFromDates).ToList();

            var combinedAllFixedFromDates = AllFixedFromDates
    .Concat(afterAllFixedFromDates) // Merge both lists
    .OrderBy(item => item.StartDate)  // Order the combined list by TrekId
    .ToList();

            var trekItineraryDayCounts = _context.TrekItineraries
  .Where(ti => ti.ItineraryType == "Fixed") // Filter for "Fixed" itineraries
  .SelectMany(ti => _context.ItineraryDetails
      .Where(td => td.TrekItineraryId == ti.Id) // Match TrekItineraryId with Id from TrekItineraries
      .Select(td => new
      {
          td.TrekId,

          td.ItineraryDay // Fetch ItineraryDays for counting
      })
  )
  .GroupBy(td => td.TrekId) // Group by TrekId
  .Select(g => new
  {
      TrekId = g.Key,
      TotalDays = g.Count() // Count ItineraryDays for each TrekId
  })
  .ToList();


            //foreach (var trekId in trekIds)
            //{

            //    var fixedDatesForTrek = combinedAllCustomizeFromDates
            //        .Where(f => f.TrekId == trekId)
            //        .ToList();

            //    foreach (var trekDetailsItinerary in fixedDatesForTrek)
            //    {
            //        var startDate = trekDetailsItinerary.StartDate.Value;
            //        for (int i = 0; i < trekDetailsItinerary.DaysCount; i++)
            //        {
            //            var currentDate = startDate.AddDays(i);
            //            AllTrekDates.Add(currentDate.ToString("yyyy-MM-dd"));
            //        }
            //    }
            //}
            foreach (var trekId in trekIds)
            {

                var fixedDatesForTrek = combinedAllFixedFromDates
                    .Where(f => f.TrekId == trekId)
                    .ToList();

                foreach (var fixedDate in fixedDatesForTrek)
                {

                    var existingItinerary = AllTrekItineraries
                        .FirstOrDefault(i => i.FromDate.Date == fixedDate.StartDate.Date);


                    int daysToProcess = existingItinerary?.ItineraryDetails?.Count
                        ?? trekItineraryDayCounts.FirstOrDefault(t => t.TrekId == fixedDate.TrekId)?.TotalDays
                        ?? 0;


                    for (int i = 0; i < daysToProcess; i++)
                    {
                        var currentDate = fixedDate.StartDate.AddDays(i);
                        AllTrekDates.Add(currentDate.ToString("yyyy-MM-dd"));
                    }
                }


            }


            var CustomizetrekItineraryDayCountsWithNames = _context.TrekItineraries
.Where(td => td.ItineraryType == "Customize")
.Select(d => new
{
    d.TrekName,
    d.TrekShortName,
    d.TrekId
})
.GroupBy(t => t.TrekId)
.Select(g => g.First())
.ToList();


            var trekNameMapping = CustomizetrekItineraryDayCountsWithNames
       .ToDictionary(t => t.TrekId, t => (t.TrekShortName, t.TrekName));


            var FixedtrekItineraryDayCountsWithNames = _context.TrekItineraries
.Where(td => td.ItineraryType == "Fixed")
.Select(d => new
{
    d.TrekName,
    d.TrekShortName,
    d.TrekId
})
.GroupBy(t => t.TrekId)
.Select(g => g.First())
.ToList();// Fetch as a list


            var FixedtrekNameMapping = FixedtrekItineraryDayCountsWithNames
                .ToDictionary(t => t.TrekId, t => (t.TrekShortName, t.TrekName));

            var maindepartureId = string.Empty;



            foreach (var trekId in trekIds)
            {
                // Get all fixed dates for this trek
                var fixedDatesForTrek = combinedAllFixedFromDates
                    .Where(f => f.TrekId == trekId)
                    .ToList();

                // Create a new column for this TrekId
                var assignedColumn = AllTrekDates.ToDictionary(dateKey => dateKey, _ => "");
                var trekSpecificColumns = new List<Dictionary<string, string>> { assignedColumn };

                foreach (var startDateEntry in fixedDatesForTrek)
                {
                    var startDate = startDateEntry.StartDate;
                    var TrekId = startDateEntry.TrekId;
                    var DepartureId = startDateEntry.DepartureId;
                    var Batch = startDateEntry.Batch;

                    // Get all participants
                    var AllTrekers = _context.TempParticipants
    .Where(t => t.TrekId == TrekId
             && t.DepartureId == DepartureId
             && (t.BookingStatus == "Pending" || t.BookingStatus == "Transfer")
             && t.PaymentStatus == "Paid")
    .Count();

                    // Get team info
                    var allTeam = _context.AddTeamToTrek
                        .Where(t => t.TrekId == TrekId && t.DepartureId == DepartureId)
                        .Select(t => new { t.Designation, t.TeamMemberName, t.DepartureId })
                        .ToList();

                    bool hasTeamData = allTeam != null && allTeam.Count > 0;
                    foreach (var team in allTeam)
                    {
                        ListofDepartureIds.Add(team.DepartureId ?? 0);
                    }

                    ViewData["DepartureId"] = ListofDepartureIds;

                    // Check if there's an existing itinerary
                    var existingItinerary = AllTrekItineraries
                        .FirstOrDefault(i => i.FromDate.Date == startDate.Date);

                    int defaultDaysToProcess = trekItineraryDayCounts
                        .FirstOrDefault(t => t.TrekId == TrekId)?.TotalDays ?? 0;

                    int daysToProcess = existingItinerary?.ItineraryDetails.Count ?? defaultDaysToProcess;

                    // Determine which column to assign
                    Dictionary<string, string> selectedColumn = null;

                    foreach (var column in trekSpecificColumns)
                    {
                        bool isBlank = true;

                        for (int i = 0; i < daysToProcess; i++)
                        {
                            var currentDate = startDate.AddDays(i);
                            var dateKey = currentDate.ToString("yyyy-MM-dd");

                            if (column.ContainsKey(dateKey) && !string.IsNullOrEmpty(column[dateKey]))
                            {
                                isBlank = false;
                                break;
                            }
                        }

                        if (isBlank)
                        {
                            selectedColumn = column;
                            break;
                        }
                    }

                    // If no blank column found, create a new one
                    if (selectedColumn == null)
                    {
                        selectedColumn = AllTrekDates.ToDictionary(dateKey => dateKey, _ => "");
                        trekSpecificColumns.Add(selectedColumn);
                    }

                    // Get trek name
                    var AlltrekName = FixedtrekNameMapping.ContainsKey(TrekId)
                        ? FixedtrekNameMapping[TrekId]
                        : ("Unknown", "Unknown Trek");

                    // Fill itinerary info
                    for (int i = 0; i < daysToProcess; i++)
                    {
                        var currentDate = startDate.AddDays(i);
                        var dateKey = currentDate.ToString("yyyy-MM-dd");
                        string heading;

                        if (existingItinerary != null)
                        {
                            var itineraryDetail = existingItinerary.ItineraryDetails.ElementAtOrDefault(i);
                            heading = itineraryDetail?.ItineraryHeading ?? "No Heading";
                        }
                        else
                        {
                            var itineraryDetail = itineraryDetails
                                .Where(it => it.TrekId == TrekId)
                                .ElementAtOrDefault(i);
                            heading = itineraryDetail?.ItineraryHeading ?? "No Heading";
                        }

                        if (i == 0)
                        {
                            heading = $"F - ({AllTrekers})-{AlltrekName.Item1}-{AlltrekName.Item2}-{heading}-({Batch})-({DepartureId})";
                            maindepartureId = $"({DepartureId})";
                        }

                        selectedColumn[dateKey] = heading;
                    }
                }

                // Assign trek-specific column to global TrekItineraryColumns
                TrekItineraryColumns.AddRange(trekSpecificColumns);
            }


            // Method to get TrekName by TrekId


            var ViewModel = TrekItineraryColumns.Select((column, index) => new OperationItineraryViewModel
            {
                ColumnNumber = index + 1,
                DatesWithHeadings = column,

                // You can also pass Batch and TrekName if needed, for example:
                //Batch = Batch,                 // Pass Batch
                //TrekName = trekName           // Pass TrekName (formatted from AlltrekName)
            }).ToList();

            return View(ViewModel);
        }

        [HttpPost]
        [Route("DepartureWise")]
        public IActionResult DepartureWise(string[] trekId, string TeamRoll, string Heading, DateTime? FromDate, DateTime? ToDate, string itineraryType)
        {
            int? NullTrekId = null; // Define NullTrekId as nullable int
            var ListofTrekIds = new List<int>();
            var ListOfStartDate = new List<DateTime>();
            var ListofDepartureIds = new List<int>();


            int[] selectedtrekIds = trekId != null && trekId.Any()
                ? trekId.Where(id => !string.IsNullOrWhiteSpace(id)) // Remove empty values
                        .Select(id => Convert.ToInt32(id))
                        .ToArray()
                : null;






            DateTime today = DateTime.Today;
            var trekList = _context.TrekDetails.Select(t => new SelectListItem
            {
                Value = t.TrekId.ToString(),
                Text = t.TrekName,
                Selected = selectedtrekIds != null && selectedtrekIds.Contains(t.TrekId) // Check if the trekId is in the list of selected ids
            }).ToList();

            // Pass the list to the ViewData for use in the dropdown
            ViewData["TrekList"] = trekList;
            ViewData["SelectedTrekId"] = trekId;



            var AllDesignation = _context.Users
        .Where(t => t.Position != null)  // Filter out null values
        .Select(t => t.Position)         // Select Position values
        .Distinct()                      // Get distinct values
        .Select(p => new SelectListItem
        {
            Value = p.ToString(),
            Text = p
        })
        .ToList();


            ViewData["AllDesignation"] = AllDesignation;




            //here we chcek AllTrek with Dates and without Dates 
            if (trekId == null || trekId.All(string.IsNullOrWhiteSpace))
            {


                var TeamviewModel = new List<OperationItineraryViewModel>();

                if (!string.IsNullOrEmpty(TeamRoll))
                {
                    var teamMembers = _context.Users
   .Where(t => t.Position == TeamRoll)
   .Select(t => new SelectListItem
   {
       Value = t.FirstName + " " + t.LastName,
       Text = t.FirstName + " " + t.LastName,
   })
   .ToList();

                    return Json(teamMembers);

                }





                var trekDetails = _context.TrekItineraries
    .Include(t => t.ItineraryDetails) // Eager loading to fetch related ItineraryDetails
    .Select(t => new
    {
        t.TrekId,
        t.StartDate,
        t.EndDate,
        t.Id,
        t.ItineraryType,
        ItineraryDetails = t.ItineraryDetails // Directly use navigation property
            .Select(d => new
            {
                d.ItineraryDay,
                d.ItineraryHeading,
                d.TrekId
            })
            .ToList(),
        DaysCount = t.ItineraryDetails.Count() // Avoid extra DB call
    })
    .ToList();


                var customizeTrekDetails = trekDetails.Where(t => t.ItineraryType == "Customize").ToList();

                var fixedTrekDetails = trekDetails.Where(t => t.ItineraryType == "Fixed").ToList();


                var FixedTrekIdsdetails = trekDetails
    .Where(t => t.ItineraryType == "Fixed")
    .Select(t => t.TrekId)
    .ToList();

                // Step 1: Get the TrekId and Id from the TrekItinerary table where ItineraryType is "Fixed"
                var FixedTrekIdsdetailsId = trekDetails
     .Where(t => t.ItineraryType == "Fixed")
     .Select(t => t.Id)
     .ToList();



                var itineraryDetails = _context.ItineraryDetails
                    .Where(d => FixedTrekIdsdetailsId.Contains(d.TrekItineraryId))
                    .Select(d => new
                    {
                        d.ItineraryHeading,
                        d.ItineraryDay,
                        d.TrekId
                    })
                    .ToList();



                var AllTrekFixedItineraries = _context.ItineraryDetails
.Where(td => FixedTrekIdsdetails.Contains(td.TrekId))
.Select(d => new
{
    d.ItineraryDay,
    d.ItineraryHeading
})
.ToList();



                var AllTrekItineraries = _context.TrekDepartureItineraryDetail
                    .Where(td => FixedTrekIdsdetails.Contains(td.TrekId))
                    .Select(d => new
                    {
                        d.TrekId,
                        d.FromDate,
                        d.ToDate,
                        d.Id,
                        d.Batch,

                        ItineraryDetails = _context.TrekDepartureItinerary
                             .Where(td => EF.Property<int>(td, "TrekDepartureItineraryDetailId") == d.Id)
                            .Select(td => new
                            {
                                td.ItineraryDay,
                                td.ItineraryHeading,
                                td.TrekId

                            })
                            .ToList(),
                        DaysCount = _context.TrekDepartureItinerary
                             .Where(td => EF.Property<int>(td, "TrekDepartureItineraryDetailId") == d.Id)
                            .Count()
                    })
                    .ToList();








                var trekIds = _context.TrekItineraries

              .OrderBy(td => td.TrekName) // Sort alphabetically
              .Select(td => td.TrekId)
              .Distinct() // Ensure unique TrekIds
              .ToList();


                var startDates = _context.TrekDeparture
  .Where(td => trekIds.Contains(td.TrekId) && (td.HideFrom == "none" || td.HideFrom == "website"))  // Filter by TrekId in trekIds list
  .OrderBy(d => d.StartDate)  // Sort by StartDate in ascending order
  .Select(d => new
  {
      d.DepartureId,
      d.Batch,
      d.StartDate,
      d.TrekId
  })
  .ToList();


                var trekItineraryDayCounts = _context.TrekItineraries
      .Where(ti => ti.ItineraryType == "Fixed") // Filter for "Fixed" itineraries
      .SelectMany(ti => _context.ItineraryDetails
          .Where(td => td.TrekItineraryId == ti.Id) // Match TrekItineraryId with Id from TrekItineraries
          .Select(td => new
          {
              td.TrekId,

              td.ItineraryDay // Fetch ItineraryDays for counting
          })
      )
      .GroupBy(td => td.TrekId) // Group by TrekId
      .Select(g => new
      {
          TrekId = g.Key,
          TotalDays = g.Count() // Count ItineraryDays for each TrekId
      })
      .ToList();




                var TrekName = _context.TrekDetails
           .Where(t => selectedtrekIds.Contains(t.TrekId))
           .Select(t => t.TrekName)
           .FirstOrDefault() ?? "Unknown Trek";

                var TrekItineraryColumns = new List<Dictionary<string, string>>();
                var AllTrekDates = new HashSet<string>();


                DateTime Today = DateTime.Today;
                var AllsevenDaysAgo = Today.AddDays(-7);


                DateTime AllFromDateAgo = (FromDate ?? Today).AddDays(-7);


                //Here we get All Trek Data by FromDate and ToDate

                if (FromDate.HasValue && ToDate.HasValue && (trekId == null || trekId.All(string.IsNullOrWhiteSpace)))


                {



                    // Filter the list for items where StartDate is within the last 7 days
                    var AllCDateBSD = customizeTrekDetails
                        .Where(fixedDate => fixedDate.StartDate >= AllFromDateAgo && fixedDate.StartDate < FromDate)
                        .ToList();


                    var AllCDateASD = customizeTrekDetails
    .Where(trekDetailsItinerary => trekDetailsItinerary.StartDate >= FromDate && trekDetailsItinerary.StartDate <= ToDate)
    .OrderBy(trekDetailsItinerary => trekDetailsItinerary.StartDate) // Sort by date (earliest first)
    .ToList();


                    // Combine both lists
                    var CombineBothCustomize = AllCDateBSD.Concat(AllCDateASD).OrderBy(item => item.StartDate).ToList();




                    var AllFDateBSD = startDates
                   .Where(fixedDate => fixedDate.StartDate >= AllFromDateAgo && fixedDate.StartDate < FromDate)
                   .ToList();


                    var AllDDateASD = startDates
                        .Where(trekDetailsItinerary => trekDetailsItinerary.StartDate >= FromDate && trekDetailsItinerary.StartDate <= ToDate)
                        .OrderBy(trekDetailsItinerary => trekDetailsItinerary.StartDate) // Sort by date (earliest first)
                        .ToList();

                    // Combine both lists
                    var CombineBothFixed = AllFDateBSD.Concat(AllDDateASD).OrderBy(item => item.StartDate).ToList();


                    //                 if (itineraryType == "Customize" || itineraryType == "All" || itineraryType == null)
                    //                 {
                    //                     foreach (var trekDetailsItinerary in CombineBothCustomize)
                    //                     {
                    //                         var startDate = trekDetailsItinerary.StartDate.Value;
                    //                         for (int i = 0; i < trekDetailsItinerary.DaysCount; i++)
                    //                         {
                    //                             var currentDate = startDate.AddDays(i);
                    //                             AllTrekDates.Add(currentDate.ToString("yyyy-MM-dd"));
                    //                         }
                    //                     }

                    //                     var CustomizeIDC = _context.TrekItineraries
                    //.Where(td => td.ItineraryType == "Customize")
                    //.Select(d => new
                    //{
                    //    d.TrekName,
                    //    d.TrekShortName,
                    //    d.TrekId
                    //})
                    //.GroupBy(t => t.TrekId)
                    //.Select(g => g.First())
                    //.ToList();


                    //                     var TNMapping = CustomizeIDC
                    //                .ToDictionary(t => t.TrekId, t => (t.TrekShortName, t.TrekName));



                    //                     foreach (var trekDetailItinerary in CombineBothCustomize)
                    //                     {
                    //                         if (trekDetailItinerary.StartDate == null)
                    //                         {
                    //                             throw new InvalidOperationException("StartDate cannot be null.");
                    //                         }

                    //                         var startDate = trekDetailItinerary.StartDate.Value;

                    //                         var EndDate = trekDetailItinerary.EndDate.Value;




                    //                         Dictionary<string, string> assignedColumn = null;

                    //                         // Check all existing columns for blank space
                    //                         foreach (var column in TrekItineraryColumns)
                    //                         {
                    //                             var isBlank = true;

                    //                             for (int i = 0; i < trekDetailItinerary.DaysCount; i++)
                    //                             {
                    //                                 var currentDate = startDate.AddDays(i);
                    //                                 var dateKey = currentDate.ToString("yyyy-MM-dd");
                    //                                 if (column.ContainsKey(dateKey) && !string.IsNullOrEmpty(column[dateKey]))
                    //                                 {
                    //                                     isBlank = false;
                    //                                     break;
                    //                                 }
                    //                             }

                    //                             if (isBlank)
                    //                             {
                    //                                 assignedColumn = column;
                    //                                 break;
                    //                             }
                    //                         }

                    //                         // Create new column if needed
                    //                         if (assignedColumn == null)
                    //                         {
                    //                             assignedColumn = new Dictionary<string, string>();
                    //                             foreach (var dateKey in AllTrekDates)
                    //                             {
                    //                                 assignedColumn[dateKey] = "";
                    //                             }
                    //                             TrekItineraryColumns.Add(assignedColumn);
                    //                         }
                    //                         var AlltrekName = TNMapping.ContainsKey(trekDetailItinerary.TrekId)
                    //         ? TNMapping[trekDetailItinerary.TrekId] // Extract tuple values
                    //         : ("Unknown", "Unknown Trek");




                    //                         // Add customize itinerary details
                    //                         for (int i = 0; i < trekDetailItinerary.DaysCount; i++)
                    //                         {
                    //                             var currentDate = startDate.AddDays(i);
                    //                             var dateKey = currentDate.ToString("yyyy-MM-dd");
                    //                             var dayKey = $"Day {i + 1}";

                    //                             var itineraryDetail = trekDetailItinerary.ItineraryDetails?.FirstOrDefault(d => d.ItineraryDay == dayKey);
                    //                             var heading = itineraryDetail?.ItineraryHeading ?? "No Heading";

                    //                             if (i == 0)
                    //                             {
                    //                                 heading = $"C - {AlltrekName.Item1}-{AlltrekName.Item2}-{heading}";

                    //                             }

                    //                             assignedColumn[dateKey] = heading;
                    //                         }
                    //                     }

                    //                 }

                    if (itineraryType == "Fixed" || itineraryType == "All" || itineraryType == null)
                    {

                        foreach (var alltrekId in trekIds)
                        {

                            var fixedDatesForTrek = CombineBothFixed
                                .Where(f => f.TrekId == alltrekId)
                                .ToList();
                            foreach (var fixedDate in fixedDatesForTrek)
                            {

                                var existingItinerary = AllTrekItineraries
                                    .FirstOrDefault(i => i.FromDate.Date == fixedDate.StartDate.Date);


                                int daysToProcess = existingItinerary?.ItineraryDetails?.Count
                                    ?? trekItineraryDayCounts.FirstOrDefault(t => t.TrekId == fixedDate.TrekId)?.TotalDays
                                    ?? 0;


                                for (int i = 0; i < daysToProcess; i++)
                                {
                                    var currentDate = fixedDate.StartDate.AddDays(i);
                                    AllTrekDates.Add(currentDate.ToString("yyyy-MM-dd"));
                                }
                            }

                        }

                        //This is for Customize All Trek Data By FromDate and ToDate 




                        var FixedIDC = _context.TrekItineraries
        .Where(td => td.ItineraryType == "Fixed")
        .Select(d => new
        {
            d.TrekName,
            d.TrekShortName,
            d.TrekId
        })
        .GroupBy(t => t.TrekId)
        .Select(g => g.First())
        .ToList();// Fetch as a list


                        var FixedTNMapping = FixedIDC
                            .ToDictionary(t => t.TrekId, t => (t.TrekShortName, t.TrekName));

                        var FixedDepartureId = string.Empty;


                        //This is for Fixed All Trek Data By FromDate and ToDate 


                        foreach (var alltrekId in trekIds)
                        {

                            var fixedDatesForTrek = CombineBothFixed
                                .Where(f => f.TrekId == alltrekId)
                                .ToList();

                            var assignedColumn = AllTrekDates.ToDictionary(dateKey => dateKey, _ => "");
                            var trekSpecificColumns = new List<Dictionary<string, string>> { assignedColumn };
                            foreach (var startDateEntry in fixedDatesForTrek)
                            {
                                var startDate = startDateEntry.StartDate;
                                var TrekId = startDateEntry.TrekId;
                                var DepartureId = startDateEntry.DepartureId;
                                var Batch = startDateEntry.Batch;
                                var AllTrekers = _context.TempParticipants
  .Where(t => t.TrekId == TrekId
           && t.DepartureId == DepartureId
           && (t.BookingStatus == "Pending" || t.BookingStatus == "Transfer")
           && t.PaymentStatus == "Paid")
  .Count();

                                var allTeam = _context.AddTeamToTrek
                .Where(t => t.TrekId == startDateEntry.TrekId && t.DepartureId == startDateEntry.DepartureId)
                .Select(t => new { t.Designation, t.TeamMemberName, t.DepartureId })
                .ToList();

                                bool hasTeamData = allTeam != null && allTeam.Count > 0;
                                //adding departureIds for view
                                foreach (var team in allTeam)
                                {
                                    ListofDepartureIds.Add(team.DepartureId ?? 0);
                                }

                                ViewData["DepartureId"] = ListofDepartureIds;


                                var existingItinerary = AllTrekItineraries
                                    .FirstOrDefault(i => i.FromDate.Date == startDate.Date);



                                int defaultDaysToProcess = trekItineraryDayCounts
                                    .FirstOrDefault(t => t.TrekId == startDateEntry.TrekId)?.TotalDays ?? 0;

                                int daysToProcess = existingItinerary?.ItineraryDetails.Count ?? defaultDaysToProcess;

                                Dictionary<string, string> selectedColumn = null;

                                foreach (var column in trekSpecificColumns)
                                {
                                    bool isBlank = true;

                                    for (int i = 0; i < daysToProcess; i++)
                                    {
                                        var currentDate = startDate.AddDays(i);
                                        var dateKey = currentDate.ToString("yyyy-MM-dd");

                                        if (column.ContainsKey(dateKey) && !string.IsNullOrEmpty(column[dateKey]))
                                        {
                                            isBlank = false;
                                            break;
                                        }
                                    }

                                    if (isBlank)
                                    {
                                        selectedColumn = column;
                                        break;
                                    }
                                }

                                // If no blank column found, create a new one
                                if (selectedColumn == null)
                                {
                                    selectedColumn = AllTrekDates.ToDictionary(dateKey => dateKey, _ => "");
                                    trekSpecificColumns.Add(selectedColumn);
                                }


                                var AlltrekName = FixedTNMapping.ContainsKey(startDateEntry.TrekId)
                                    ? FixedTNMapping[startDateEntry.TrekId]
                                    : ("Unknown", "Unknown Trek");

                                for (int i = 0; i < daysToProcess; i++)
                                {
                                    var currentDate = startDate.AddDays(i);
                                    var dateKey = currentDate.ToString("yyyy-MM-dd");
                                    string heading;
                                    string departureId;

                                    if (existingItinerary != null)
                                    {
                                        var itineraryDetail = existingItinerary.ItineraryDetails.ElementAtOrDefault(i);
                                        heading = itineraryDetail?.ItineraryHeading ?? "No Heading";

                                    }
                                    else
                                    {
                                        var itineraryDetail = itineraryDetails
                                            .Where(i => i.TrekId == startDateEntry.TrekId)
                                            .ElementAtOrDefault(i);
                                        heading = itineraryDetail?.ItineraryHeading ?? "No Heading";
                                    }

                                    if (i == 0)
                                    {
                                        if (i == 0)
                                        {
                                            heading = $"F - ({AllTrekers})-{AlltrekName.Item1}-{AlltrekName.Item2}-{heading}-({Batch})-({DepartureId})";
                                            FixedDepartureId = $"({DepartureId})";
                                        }
                                    }

                                    selectedColumn[dateKey] = heading;
                                }
                            }

                            TrekItineraryColumns.AddRange(trekSpecificColumns);


                        }






                    }
                    // Method to get TrekName by TrekId


                    var AllViewModel = TrekItineraryColumns.Select((column, index) => new OperationItineraryViewModel
                    {
                        ColumnNumber = index + 1,
                        DatesWithHeadings = column,

                    }).ToList();
                    return View(AllViewModel);



                }
                if (trekId == null || trekId.All(string.IsNullOrWhiteSpace))
                {


                    var AllCoustomizeFromDates = customizeTrekDetails
                        .Where(fixedDate => fixedDate.StartDate >= AllsevenDaysAgo && fixedDate.StartDate < today)
                        .ToList();



                    var afterAllCustomizeFromDates = customizeTrekDetails
                        .Where(trekDetailsItinerary => trekDetailsItinerary.StartDate >= today)
                        .OrderBy(trekDetailsItinerary => trekDetailsItinerary.StartDate)
                        .ToList();


                    var combinedAllCustomizeFromDates = AllCoustomizeFromDates.Concat(afterAllCustomizeFromDates).ToList();




                    var AllFixedFromDates = startDates
                   .Where(fixedDate => fixedDate.StartDate >= AllsevenDaysAgo && fixedDate.StartDate < today)
                   .ToList();


                    var afterAllFixedFromDates = startDates
                        .Where(trekDetailsItinerary => trekDetailsItinerary.StartDate >= today)
                        .OrderBy(trekDetailsItinerary => trekDetailsItinerary.StartDate) // Sort by date (earliest first)
                        .ToList();

                    // Combine both lists
                    var combinedAllFixedFromDates = AllFixedFromDates.Concat(afterAllFixedFromDates).ToList();




                    var AllTrekItineraryColumns = new List<Dictionary<string, string>>();


                    if (itineraryType == "Fixed" || itineraryType == "All" || itineraryType == null)
                    {

                        foreach (var fixedDate in combinedAllFixedFromDates)
                        {

                            var existingItinerary = AllTrekItineraries
                                .FirstOrDefault(i => i.FromDate.Date == fixedDate.StartDate.Date);


                            int daysToProcess = existingItinerary?.ItineraryDetails?.Count
                                ?? trekItineraryDayCounts.FirstOrDefault(t => t.TrekId == fixedDate.TrekId)?.TotalDays
                                ?? 0;


                            for (int i = 0; i < daysToProcess; i++)
                            {
                                var currentDate = fixedDate.StartDate.AddDays(i);
                                AllTrekDates.Add(currentDate.ToString("yyyy-MM-dd"));
                            }
                        }











                        var FixedtrekItineraryDayCountsWithNames = _context.TrekItineraries
        .Where(td => td.ItineraryType == "Fixed")
        .Select(d => new
        {
            d.TrekName,
            d.TrekShortName,
            d.TrekId
        })
        .GroupBy(t => t.TrekId)
        .Select(g => g.First())
        .ToList();// Fetch as a list


                        var FixedtrekNameMapping = FixedtrekItineraryDayCountsWithNames
                            .ToDictionary(t => t.TrekId, t => (t.TrekShortName, t.TrekName));

                        var maindepartureId = string.Empty;



                        //Here we Get All The Fixed Data Without Choosing FromDate and ToDate
                        foreach (var startDateEntry in combinedAllFixedFromDates)
                        {
                            var startDate = startDateEntry.StartDate;
                            var TrekId = startDateEntry.TrekId;
                            var DepartureId = startDateEntry.DepartureId;
                            var Batch = startDateEntry.Batch;
                            var AllTrekers = _context.TempParticipants
 .Where(t => t.TrekId == TrekId
          && t.DepartureId == DepartureId
          && (t.BookingStatus == "Pending" || t.BookingStatus == "Transfer")
          && t.PaymentStatus == "Paid")
 .Count();

                            var allTeam = _context.AddTeamToTrek
            .Where(t => t.TrekId == startDateEntry.TrekId && t.DepartureId == startDateEntry.DepartureId)
            .Select(t => new { t.Designation, t.TeamMemberName, t.DepartureId })
            .ToList();

                            bool hasTeamData = allTeam != null && allTeam.Count > 0;
                            //adding departureIds for view
                            foreach (var team in allTeam)
                            {
                                ListofDepartureIds.Add(team.DepartureId ?? 0);
                            }

                            ViewData["DepartureId"] = ListofDepartureIds;


                            var existingItinerary = AllTrekItineraries
                                .FirstOrDefault(i => i.FromDate.Date == startDate.Date);

                            Dictionary<string, string> assignedColumn = null;

                            int defaultDaysToProcess = trekItineraryDayCounts
                                .FirstOrDefault(t => t.TrekId == startDateEntry.TrekId)?.TotalDays ?? 0;

                            int daysToProcess = existingItinerary?.ItineraryDetails.Count ?? defaultDaysToProcess;

                            foreach (var column in AllTrekItineraryColumns)
                            {
                                bool isBlank = true;

                                for (int i = 0; i < daysToProcess; i++)
                                {
                                    var currentDate = startDate.AddDays(i);
                                    var dateKey = currentDate.ToString("yyyy-MM-dd");

                                    if (column.ContainsKey(dateKey) && !string.IsNullOrEmpty(column[dateKey]))
                                    {
                                        isBlank = false;
                                        break;
                                    }
                                }

                                if (isBlank)
                                {
                                    assignedColumn = column;
                                    break;
                                }
                            }

                            if (assignedColumn == null)
                            {
                                assignedColumn = AllTrekDates.ToDictionary(dateKey => dateKey, _ => "");
                                AllTrekItineraryColumns.Add(assignedColumn);
                            }

                            var AlltrekName = FixedtrekNameMapping.ContainsKey(startDateEntry.TrekId)
                            ? FixedtrekNameMapping[startDateEntry.TrekId]
                            : ("Unknown", "Unknown Trek");

                            for (int i = 0; i < daysToProcess; i++)
                            {
                                var currentDate = startDate.AddDays(i);
                                var dateKey = currentDate.ToString("yyyy-MM-dd");
                                string heading;
                                string departureId;

                                if (existingItinerary != null)
                                {
                                    var itineraryDetail = existingItinerary.ItineraryDetails.ElementAtOrDefault(i);
                                    heading = itineraryDetail?.ItineraryHeading ?? "No Heading";

                                }
                                else
                                {
                                    var itineraryDetail = itineraryDetails
                                        .Where(i => i.TrekId == startDateEntry.TrekId)
                                        .ElementAtOrDefault(i);
                                    heading = itineraryDetail?.ItineraryHeading ?? "No Heading";
                                }

                                if (i == 0)
                                {
                                    heading = $"F - ({AllTrekers})-{AlltrekName.Item1}-{AlltrekName.Item2}-{heading}-({Batch})-({DepartureId})";
                                    maindepartureId = $"({DepartureId})";
                                }

                                assignedColumn[dateKey] = heading;
                            }
                        }

                    }

                    if (itineraryType == "Customize" || itineraryType == "All" || itineraryType == null)
                    {

                        foreach (var trekDetailsItinerary in combinedAllCustomizeFromDates)
                        {
                            var startDate = trekDetailsItinerary.StartDate.Value;
                            for (int i = 0; i < trekDetailsItinerary.DaysCount; i++)
                            {
                                var currentDate = startDate.AddDays(i);
                                AllTrekDates.Add(currentDate.ToString("yyyy-MM-dd"));
                            }
                        }


                        var CustomizetrekItineraryDayCountsWithNames = _context.TrekItineraries
    .Where(td => td.ItineraryType == "Customize")
    .Select(d => new
    {
        d.TrekName,
        d.TrekShortName,
        d.TrekId
    })
    .GroupBy(t => t.TrekId)
    .Select(g => g.First())
    .ToList();


                        var trekNameMapping = CustomizetrekItineraryDayCountsWithNames
                   .ToDictionary(t => t.TrekId, t => (t.TrekShortName, t.TrekName));


                        //Here we get All the Customize data without choosing FromDate and ToDate

                        foreach (var trekDetailItinerary in combinedAllCustomizeFromDates)
                        {
                            if (trekDetailItinerary.StartDate == null)
                            {
                                throw new InvalidOperationException("StartDate cannot be null.");
                            }

                            var startDate = trekDetailItinerary.StartDate.Value;
                            var TrekId = trekDetailItinerary.TrekId;




                            var allTeam = _context.AddTeamToTrek
           .Where(t => t.TrekId == trekDetailItinerary.TrekId && t.StartDate.Date == trekDetailItinerary.StartDate.Value.Date && t.Trekitinerary == "Customize")
        .Select(t => new { t.TrekId, t.StartDate })
           .ToList();

                            bool hasTeamData = allTeam != null && allTeam.Count > 0;
                            //adding departureIds for view
                            foreach (var team in allTeam)
                            {
                                ListofTrekIds.Add(team.TrekId);
                                ListOfStartDate.Add(team.StartDate);

                            }


                            ViewData["CustomizeTrekId"] = ListofTrekIds;
                            ViewData["CustmoizeStartDate"] = ListOfStartDate;
                            Dictionary<string, string> assignedColumn = null;

                            // Check all existing columns for blank space
                            foreach (var column in AllTrekItineraryColumns)
                            {
                                var isBlank = true;

                                for (int i = 0; i < trekDetailItinerary.DaysCount; i++)
                                {
                                    var currentDate = startDate.AddDays(i);
                                    var dateKey = currentDate.ToString("yyyy-MM-dd");
                                    if (column.ContainsKey(dateKey) && !string.IsNullOrEmpty(column[dateKey]))
                                    {
                                        isBlank = false;
                                        break;
                                    }
                                }

                                if (isBlank)
                                {
                                    assignedColumn = column;
                                    break;
                                }
                            }

                            // Create new column if needed
                            if (assignedColumn == null)
                            {
                                assignedColumn = new Dictionary<string, string>();
                                foreach (var dateKey in AllTrekDates)
                                {
                                    assignedColumn[dateKey] = "";
                                }
                                AllTrekItineraryColumns.Add(assignedColumn);
                            }
                            var AlltrekName = trekNameMapping.ContainsKey(trekDetailItinerary.TrekId)
            ? trekNameMapping[trekDetailItinerary.TrekId] // Extract tuple values
            : ("Unknown", "Unknown Trek");


                            // Add customize itinerary details
                            for (int i = 0; i < trekDetailItinerary.DaysCount; i++)
                            {
                                var currentDate = startDate.AddDays(i);
                                var dateKey = currentDate.ToString("yyyy-MM-dd");
                                var dayKey = $"Day {i + 1}";

                                var itineraryDetail = trekDetailItinerary.ItineraryDetails?.FirstOrDefault(d => d.ItineraryDay == dayKey);
                                var heading = itineraryDetail?.ItineraryHeading ?? "No Heading";

                                if (i == 0)
                                {
                                    heading = $"C - {AlltrekName.Item1}-{AlltrekName.Item2}-{heading}-{TrekId}";

                                }

                                assignedColumn[dateKey] = heading;
                            }
                        }

                    }




                    // Method to get TrekName by TrekId

                    var ViewModel = AllTrekItineraryColumns.Select((column, index) => new OperationItineraryViewModel
                    {
                        ColumnNumber = index + 1,
                        DatesWithHeadings = column,
                        AllTrekDates = AllTrekDates.ToList(),
                        //DepartureId = ListofDepartureIds,
                    }).ToList();
                    return View(ViewModel);
                }
            }




            var trekItineraries = _context.TrekItineraries
       .Where(t => selectedtrekIds.Contains(t.TrekId))
       .GroupBy(t => new { t.Id, t.StartDate, t.ItineraryType, t.TrekId }) // Group by ItineraryType
     .Select(group => new
     {
         Id = group.Key.Id,
         TrekId = group.Key.TrekId,
         StartDate = group.Key.StartDate,
         ItineraryType = group.Key.ItineraryType,
         DayCount = group.Max(g => g.ItineraryDayCount),
         ItineraryDetails = group.Key.ItineraryType == "Customize"
? group.SelectMany(g => g.ItineraryDetails.Select(i => new ItineraryDetail
{
    ItineraryDay = i.ItineraryDay,
    ItineraryHeading = i.ItineraryHeading
})).ToList()
: new List<ItineraryDetail>() // Empty list for Fixed
     })

       .ToList();
            var customizeItineraries = trekItineraries.Where(t => t.ItineraryType == "Customize").ToList();
            var fixedItineraries = trekItineraries.Where(t => t.ItineraryType == "Fixed").ToList();

            List<DateTime> fixedStartDates = new List<DateTime>();




            var fixedFromDates = _context.TrekDeparture
    .Where(td => selectedtrekIds.Contains(td.TrekId) && (td.HideFrom == "none" || td.HideFrom == "website"))
    .OrderBy(td => td.StartDate) // Corrected order by
    .Select(td => new
    {
        td.StartDate,
        td.TrekId,
        td.DepartureId,
        td.Batch,
    })
    .ToList();



            var validFixedItineraries = fixedFromDates
    .Where(d => d.StartDate != default(DateTime))
    .ToList();


            var allItineraries = _context.TrekDepartureItineraryDetail
      .Where(tdi => selectedtrekIds.Contains(tdi.TrekId))
      .Include(tdi => tdi.TrekDepartureItinerary)
      .ToList();


            var itineraryDaysByFromDate = allItineraries
                .GroupBy(tdi => new { tdi.FromDate, tdi.TrekId })
                .Select(group => new
                {
                    TrekId = group.Key.TrekId,
                    FromDate = group.Key.FromDate,

                    ItineraryDetails = group.SelectMany(tdi => tdi.TrekDepartureItinerary)
                        .Select(it => new { it.ItineraryDay, it.ItineraryHeading, it.TrekId })
                        .ToList()
                })
                .OrderBy(result => result.FromDate)
            .ToList();

            var fixedItineraryDetails = _context.TrekItineraries
                .Where(t => selectedtrekIds.Contains(t.TrekId) && t.ItineraryType == "Fixed")
                .SelectMany(ti => _context.ItineraryDetails
                    .Where(td => td.TrekItineraryId == ti.Id)
                    .Select(td => new
                    {
                        td.ItineraryDay,
                        td.ItineraryHeading,
                        td.TrekId
                    })
                )
                .ToList();



            //Here we Get data By TrekId and FromDate and ToDate

            if (FromDate.HasValue && ToDate.HasValue && trekId != null)

            {

                DateTime FromDateAgo = FromDate.Value.AddDays(-7); // Subtract 7 days from FromDate

                var filteredCoustomizeDates = customizeItineraries
                    .Where(fixedDate => fixedDate.StartDate >= FromDateAgo && fixedDate.StartDate <= FromDate.Value) // Using FromDate.Value
                    .ToList();


                var afterfilteredCustomizeDates = customizeItineraries
                    .Where(trekItinerary => trekItinerary.StartDate >= FromDate && trekItinerary.StartDate <= ToDate)
                    .OrderBy(trekItinerary => trekItinerary.StartDate) // Sort by date (earliest first)
                    .ToList();

                // Combine both lists
                var combinedCustomizeDates = filteredCoustomizeDates.Concat(afterfilteredCustomizeDates).ToList();



                var filteredFixedDates = fixedFromDates
               .Where(fixedDate => fixedDate.StartDate >= FromDateAgo && fixedDate.StartDate <= FromDate)
               .ToList();



                var afterfilteredFixedDates = _context.TrekDeparture
     .Where(td => selectedtrekIds.Contains(td.TrekId)
         && (td.HideFrom == "none" || td.HideFrom == "website")

         && td.StartDate > FromDate
         && td.StartDate <= ToDate)
     .OrderBy(td => td.StartDate) // Corrected order by
     .Select(td => new
     {
         td.StartDate,
         td.TrekId,
         td.DepartureId,
         td.Batch,
     })
     .ToList();

                // Combine both lists
                var combinedFixedDates = filteredFixedDates.Concat(afterfilteredFixedDates).ToList();




                var allFixedDates = new HashSet<string>();
                var itineraryFixedColumns = new List<Dictionary<string, string>>();
                if (itineraryType == "Customize" || itineraryType == "All" || itineraryType == null)
                {


                    foreach (var trekItinerary in combinedCustomizeDates)
                    {
                        var startDate = trekItinerary.StartDate.Value;
                        for (int i = 0; i < trekItinerary.DayCount; i++)
                        {
                            var currentDate = startDate.AddDays(i);
                            allFixedDates.Add(currentDate.ToString("yyyy-MM-dd"));
                        }
                    }



                    foreach (var fixedDate in combinedFixedDates)
                    {
                        var Startdate = fixedDate.StartDate;
                        var TrekId = fixedDate.TrekId;
                        int totalDays = fixedItineraryDetails
         .Where(i => i.TrekId == TrekId)
         .Count();

                        var existingItinerary = itineraryDaysByFromDate
                            .FirstOrDefault(i => i.FromDate.Date == fixedDate.StartDate && i.TrekId == fixedDate.TrekId);
                        var daysToProcess = existingItinerary?.ItineraryDetails.Count ?? totalDays;

                        for (int i = 0; i < daysToProcess; i++)
                        {
                            var currentDate = fixedDate.StartDate.AddDays(i);
                            allFixedDates.Add(currentDate.ToString("yyyy-MM-dd"));
                        }
                    }
                }

                if (itineraryType == "Fixed" || itineraryType == "All" || itineraryType == null)
                {
                    //here we Get Custmoize Data By TrekId and FromDate and Todate
                    foreach (var trekItinerary in combinedCustomizeDates)
                    {
                        var startDate = trekItinerary.StartDate.Value;
                        Dictionary<string, string> assignedColumn = null;

                        var CFixedtrekName = _context.TrekItineraries
    .Where(t => t.TrekId == trekItinerary.TrekId)
    .Select(d => new
    {
        d.TrekName,
        d.TrekShortName
    })
    .FirstOrDefault();
                        foreach (var column in itineraryFixedColumns)
                        {
                            var isBlank = true;


                            for (int i = 0; i < trekItinerary.DayCount; i++)
                            {
                                var currentDate = startDate.AddDays(i);
                                var dateKey = currentDate.ToString("yyyy-MM-dd");
                                if (column.ContainsKey(dateKey) && !string.IsNullOrEmpty(column[dateKey]))
                                {
                                    isBlank = false;
                                    break;
                                }
                            }

                            if (isBlank)
                            {
                                assignedColumn = column;
                                break;
                            }
                        }

                        // Create new column if needed
                        if (assignedColumn == null)
                        {
                            assignedColumn = new Dictionary<string, string>();
                            // Initialize all possible dates
                            foreach (var dateKey in allFixedDates)
                            {
                                assignedColumn[dateKey] = "";
                            }
                            itineraryFixedColumns.Add(assignedColumn);
                        }

                        // Add customize itinerary details
                        for (int i = 0; i < trekItinerary.DayCount; i++)
                        {
                            var currentDate = startDate.AddDays(i);
                            var dateKey = currentDate.ToString("yyyy-MM-dd");
                            var dayKey = $"Day {i + 1}";
                            var itineraryDetail = trekItinerary.ItineraryDetails
                                .FirstOrDefault(d => d.ItineraryDay == dayKey);

                            // If the itinerary detail exists, use the heading, else default to "No Heading"
                            var heading = itineraryDetail?.ItineraryHeading ?? "No Heading";

                            // Initialize a list of strings to hold the formatted headings for each dateKey
                            List<string> formattedHeadings = new List<string>();


                            if (CFixedtrekName != null)  // Ensure data is found
                            {
                                if (i == 0)
                                {
                                    // Format the heading using the retrieved values
                                    string formattedHeading = $"C - {CFixedtrekName.TrekShortName} - {CFixedtrekName.TrekName} - {heading}";
                                    formattedHeadings.Add(formattedHeading);
                                }
                                else
                                {
                                    // For other days, just use the heading, or you can customize it further
                                    formattedHeadings.Add(heading);
                                }
                            }






                            // Join the list of formatted headings into a single string with a delimiter (e.g., new line or comma)
                            assignedColumn[dateKey] = string.Join(", ", formattedHeadings); // Use ", " or any delimiter you prefer
                        }

                    }





                    // here we Get Fixed Data By TrekId FromDate and ToDate
                    foreach (var fixedDate in combinedFixedDates)
                    {
                        var StartDate = fixedDate.StartDate;
                        var TrekId = fixedDate.TrekId;
                        var DepartureId = fixedDate.DepartureId;
                        var Batch = fixedDate.Batch;
                        var existingItinerary = itineraryDaysByFromDate
                            .FirstOrDefault(i => i.FromDate.Date == fixedDate.StartDate && i.TrekId == fixedDate.TrekId);

                        int totalDays = fixedItineraryDetails
            .Where(i => i.TrekId == TrekId)
            .Count();
                        var allTeam = _context.AddTeamToTrek
            .Where(t => t.TrekId == fixedDate.TrekId && t.DepartureId == fixedDate.DepartureId)
            .Select(t => new { t.Designation, t.TeamMemberName, t.DepartureId })
            .ToList();

                        bool hasTeamData = allTeam != null && allTeam.Count > 0;
                        //adding departureIds for view
                        foreach (var team in allTeam)
                        {
                            ListofDepartureIds.Add(team.DepartureId ?? 0);
                        }

                        ViewData["DepartureId"] = ListofDepartureIds;

                        var FtrekName = _context.TrekItineraries
    .Where(t => t.TrekId == fixedDate.TrekId)
    .Select(d => new
    {
        d.TrekName,
        d.TrekShortName
    })
    .FirstOrDefault();

                        var AllTrekers = _context.TempParticipants
            .Where(t => t.DepartureId == fixedDate.DepartureId
                && t.TrekId == TrekId
                && (t.BookingStatus == "Pending" || t.BookingStatus == "Transfer")
                && t.PaymentStatus.ToLower() == "paid")
            .Count();


                        Dictionary<string, string> assignedColumn = null;


                        int daysToProcess = existingItinerary?.ItineraryDetails.Count ?? totalDays;


                        foreach (var column in itineraryFixedColumns)
                        {
                            var isBlank = true;


                            for (int i = 0; i < daysToProcess; i++)
                            {
                                var currentDate = fixedDate.StartDate.AddDays(i);
                                var dateKey = currentDate.ToString("yyyy-MM-dd");
                                if (column.ContainsKey(dateKey) && !string.IsNullOrEmpty(column[dateKey]))
                                {
                                    isBlank = false;
                                    break;
                                }
                            }

                            if (isBlank)
                            {
                                assignedColumn = column;
                                break;
                            }
                        }

                        // Create new column if needed
                        if (assignedColumn == null)
                        {
                            assignedColumn = new Dictionary<string, string>();
                            // Initialize all possible dates
                            foreach (var dateKey in allFixedDates)
                            {
                                assignedColumn[dateKey] = "";
                            }
                            itineraryFixedColumns.Add(assignedColumn);
                        }

                        // Add fixed itinerary details
                        for (int i = 0; i < daysToProcess; i++)
                        {
                            var currentDate = fixedDate.StartDate.AddDays(i);
                            var dateKey = currentDate.ToString("yyyy-MM-dd");
                            string heading;

                            if (existingItinerary != null)
                            {
                                // Filter ItineraryDetails by TrekId
                                var filteredItineraryDetails = existingItinerary.ItineraryDetails
                                    .Where(it => it.TrekId == fixedDate.TrekId)
                                    .ToList();

                                // Use filtered itinerary details
                                var itineraryDetail = filteredItineraryDetails.ElementAtOrDefault(i);
                                heading = itineraryDetail?.ItineraryHeading ?? "No Heading";
                            }
                            else
                            {
                                // Use fixed itinerary details
                                var filteredItineraryDetails = fixedItineraryDetails
               .Where(it => it.TrekId == fixedDate.TrekId)
               .ToList();

                                // Use filtered itinerary details
                                var itineraryDetail = filteredItineraryDetails.ElementAtOrDefault(i);
                                heading = itineraryDetail?.ItineraryHeading ?? "No Heading";
                            }

                            List<string> formattedHeadings = new List<string>();

                            if (FtrekName != null)  // Ensure data is found
                            {
                                if (i == 0)
                                {

                                    string formattedHeading = $"F - ({AllTrekers})-{FtrekName.TrekShortName} - {FtrekName.TrekName} - {heading}-({Batch})-({DepartureId})";
                                    formattedHeadings.Add(formattedHeading);  // Add the formatted heading to the list

                                }
                                else
                                {
                                    // For other days, just use the heading, or you can customize it further
                                    formattedHeadings.Add(heading);
                                }
                            }

                            // Join the list of formatted headings into a single string with a delimiter (e.g., new line or comma)
                            assignedColumn[dateKey] = string.Join(", ", formattedHeadings); // Use ", " or any delimiter you prefer


                        }
                    }

                }
                // Sort dates within each column to ensure proper ordering
                foreach (var column in itineraryFixedColumns)
                {
                    var sortedEntries = allFixedDates
                        .ToDictionary(dateKey => dateKey, dateKey =>
                            column.ContainsKey(dateKey) ? column[dateKey] : "");
                    column.Clear();
                    foreach (var entry in sortedEntries.OrderBy(x => x.Key))
                    {
                        column[entry.Key] = entry.Value;
                    }
                }

                var ViewModel = itineraryFixedColumns.Select((column, index) => new OperationItineraryViewModel
                {
                    ColumnNumber = index + 1,
                    DatesWithHeadings = column,
                    AllTrekDates = allFixedDates.ToList(),
                }).ToList();

                return View(ViewModel);

            }


            var itineraryColumns = new List<Dictionary<string, string>>();




            var allDates = new HashSet<string>();

            var sevenDaysAgo = today.AddDays(-7);
            var filteredCoustomizeFromDates = customizeItineraries
               .Where(fixedDate => fixedDate.StartDate >= sevenDaysAgo && fixedDate.StartDate <= today)
               .ToList();


            var afterfilteredCustomizeFromDates = customizeItineraries
                .Where(trekItinerary => trekItinerary.StartDate > today)
                .OrderBy(trekItinerary => trekItinerary.StartDate) // Sort by date (earliest first)
                .ToList();

            // Combine both lists
            var combinedCustomizeFromDates = filteredCoustomizeFromDates.Concat(afterfilteredCustomizeFromDates).ToList();





            var filteredFixedFromDates = fixedFromDates
                .Where(fixedDate => fixedDate.StartDate >= sevenDaysAgo && fixedDate.StartDate <= today)
                .ToList();



            var afterfilteredFixedFromDates = fixedFromDates
                .Where(fixedDate => fixedDate.StartDate > today)
                .OrderBy(fixedDate => fixedDate.StartDate) // Sort by date (earliest first)
                .ToList();

            // Combine both lists
            var combinedFixedFromDates = filteredFixedFromDates.Concat(afterfilteredFixedFromDates).ToList();



            if (itineraryType == "Customize" || itineraryType == "All" || itineraryType == null)
            {
                //Here we get All the Customize data By TrekId only
                foreach (var trekItinerary in combinedCustomizeFromDates)
                {
                    var startDate = trekItinerary.StartDate.Value;
                    for (int i = 0; i < trekItinerary.DayCount; i++)
                    {
                        var currentDate = startDate.AddDays(i);
                        allDates.Add(currentDate.ToString("yyyy-MM-dd"));
                    }
                }


                foreach (var trekItinerary in combinedCustomizeFromDates)
                {
                    var startDate = trekItinerary.StartDate.Value;
                    Dictionary<string, string> assignedColumn = null;
                    var CFixedtrekName = _context.TrekItineraries
            .Where(t => t.TrekId == trekItinerary.TrekId)
            .Select(d => new
            {
                d.TrekName,
                d.TrekShortName
            })
            .FirstOrDefault();

                    foreach (var column in itineraryColumns)
                    {
                        var isBlank = true;


                        for (int i = 0; i < trekItinerary.DayCount; i++)
                        {
                            var currentDate = startDate.AddDays(i);
                            var dateKey = currentDate.ToString("yyyy-MM-dd");
                            if (column.ContainsKey(dateKey) && !string.IsNullOrEmpty(column[dateKey]))
                            {
                                isBlank = false;
                                break;
                            }
                        }

                        if (isBlank)
                        {
                            assignedColumn = column;
                            break;
                        }
                    }

                    // Create new column if needed
                    if (assignedColumn == null)
                    {
                        assignedColumn = new Dictionary<string, string>();
                        // Initialize all possible dates
                        foreach (var dateKey in allDates)
                        {
                            assignedColumn[dateKey] = "";
                        }
                        itineraryColumns.Add(assignedColumn);
                    }

                    // Add customize itinerary details
                    for (int i = 0; i < trekItinerary.DayCount; i++)
                    {
                        var currentDate = startDate.AddDays(i);
                        var dateKey = currentDate.ToString("yyyy-MM-dd");
                        var dayKey = $"Day {i + 1}";
                        var itineraryDetail = trekItinerary.ItineraryDetails
                            .FirstOrDefault(d => d.ItineraryDay == dayKey);

                        // If the itinerary detail exists, use the heading, else default to "No Heading"
                        var heading = itineraryDetail?.ItineraryHeading ?? "No Heading";

                        // Initialize a list of strings to hold the formatted headings for each dateKey
                        List<string> formattedHeadings = new List<string>();

                        if (CFixedtrekName != null)  // Ensure data is found
                        {
                            if (i == 0)
                            {
                                // Format the heading using the retrieved values
                                string formattedHeading = $"C - {CFixedtrekName.TrekShortName} - {CFixedtrekName.TrekName} - {heading}";
                                formattedHeadings.Add(formattedHeading);
                            }
                            else
                            {
                                // For other days, just use the heading, or you can customize it further
                                formattedHeadings.Add(heading);
                            }
                        }


                        // Join the list of formatted headings into a single string with a delimiter (e.g., new line or comma)
                        assignedColumn[dateKey] = string.Join(", ", formattedHeadings); // Use ", " or any delimiter you prefer
                    }


                }

            }





            if (itineraryType == "Fixed" || itineraryType == "All" || itineraryType == null)
            {

                foreach (var fixedDate in combinedFixedFromDates)
                {
                    var Startdate = fixedDate.StartDate;
                    var TrekId = fixedDate.TrekId;
                    int totalDays = fixedItineraryDetails
         .Where(i => i.TrekId == TrekId)
         .Count();



                    var existingItinerary = itineraryDaysByFromDate
                        .FirstOrDefault(i => i.FromDate.Date == fixedDate.StartDate && i.TrekId == fixedDate.TrekId);
                    var daysToProcess = existingItinerary?.ItineraryDetails.Count ?? totalDays;

                    for (int i = 0; i < daysToProcess; i++)
                    {
                        var currentDate = fixedDate.StartDate.AddDays(i);
                        allDates.Add(currentDate.ToString("yyyy-MM-dd"));
                    }
                }

                Dictionary<int, bool> departureIdStatus = new Dictionary<int, bool>();

                // here we get All the fixed data By the TrekId only
                foreach (var fixedDate in combinedFixedFromDates)
                {
                    var StartDate = fixedDate.StartDate;
                    var TrekId = fixedDate.TrekId;
                    var DepartureId = fixedDate.DepartureId;
                    var Batch = fixedDate.Batch;
                    var existingItinerary = itineraryDaysByFromDate
                        .FirstOrDefault(i => i.FromDate.Date == fixedDate.StartDate && i.TrekId == fixedDate.TrekId);

                    int totalDays = fixedItineraryDetails
         .Where(i => i.TrekId == TrekId)
         .Count();

                    var FtrekName = _context.TrekItineraries
             .Where(t => t.TrekId == fixedDate.TrekId)
             .Select(d => new
             {
                 d.TrekName,
                 d.TrekShortName,
                 d.TrekId
             })
             .FirstOrDefault();

                    var AllTrekers = _context.TempParticipants
  .Where(t => t.TrekId == TrekId
           && t.DepartureId == DepartureId
           && (t.BookingStatus == "Pending" || t.BookingStatus == "Transfer")
           && t.PaymentStatus == "Paid")
  .Count();
                    var allTeam = _context.AddTeamToTrek
            .Where(t => t.TrekId == fixedDate.TrekId && t.DepartureId == fixedDate.DepartureId)
            .Select(t => new { t.Designation, t.TeamMemberName, t.DepartureId })
            .ToList();

                    bool hasTeamData = allTeam != null && allTeam.Count > 0;
                    //adding departureIds for view
                    foreach (var team in allTeam)
                    {
                        ListofDepartureIds.Add(team.DepartureId ?? 0);
                    }

                    ViewData["DepartureId"] = ListofDepartureIds;
                    //// Store the status (true if data exists, false otherwise)
                    //departureIdStatus[fixedDate.DepartureId] = hasTeamData;
                    //ViewData["DepartureIdStatus"] = departureIdStatus;

                    Dictionary<string, string> assignedColumn = null;


                    int daysToProcess = existingItinerary?.ItineraryDetails.Count ?? totalDays;


                    foreach (var column in itineraryColumns)
                    {
                        var isBlank = true;


                        for (int i = 0; i < daysToProcess; i++)
                        {
                            var currentDate = fixedDate.StartDate.AddDays(i);
                            var dateKey = currentDate.ToString("yyyy-MM-dd");
                            if (column.ContainsKey(dateKey) && !string.IsNullOrEmpty(column[dateKey]))
                            {
                                isBlank = false;
                                break;
                            }
                        }

                        if (isBlank)
                        {
                            assignedColumn = column;
                            break;
                        }
                    }

                    // Create new column if needed
                    if (assignedColumn == null)
                    {
                        assignedColumn = new Dictionary<string, string>();
                        // Initialize all possible dates
                        foreach (var dateKey in allDates)
                        {
                            assignedColumn[dateKey] = "";
                        }
                        itineraryColumns.Add(assignedColumn);
                    }

                    // Add fixed itinerary details
                    for (int i = 0; i < daysToProcess; i++)
                    {
                        var currentDate = fixedDate.StartDate.AddDays(i);
                        var dateKey = currentDate.ToString("yyyy-MM-dd");
                        string heading;






                        if (existingItinerary != null)
                        {
                            // Filter ItineraryDetails by TrekId
                            var filteredItineraryDetails = existingItinerary.ItineraryDetails
                                .Where(it => it.TrekId == fixedDate.TrekId)
                                .ToList();

                            // Use filtered itinerary details
                            var itineraryDetail = filteredItineraryDetails.ElementAtOrDefault(i);
                            heading = itineraryDetail?.ItineraryHeading ?? "No Heading";
                        }
                        else
                        {
                            // Use fixed itinerary details
                            var filteredItineraryDetails = fixedItineraryDetails
           .Where(it => it.TrekId == fixedDate.TrekId)
           .ToList();

                            // Use filtered itinerary details
                            var itineraryDetail = filteredItineraryDetails.ElementAtOrDefault(i);
                            heading = itineraryDetail?.ItineraryHeading ?? "No Heading";
                        }

                        List<string> formattedHeadings = new List<string>();
                        if (FtrekName != null)  // Ensure data is found
                        {
                            if (i == 0)
                            {

                                string formattedHeading = $"F - ({AllTrekers})-{FtrekName.TrekShortName} - {FtrekName.TrekName} - {heading}-({Batch})-({DepartureId})";
                                formattedHeadings.Add(formattedHeading);  // Add the formatted heading to the list

                            }
                            else
                            {
                                // For other days, just use the heading, or you can customize it further
                                formattedHeadings.Add(heading);
                            }
                        }


                        // Join the list of formatted headings into a single string with a delimiter (e.g., new line or comma)
                        assignedColumn[dateKey] = string.Join(", ", formattedHeadings); // Use ", " or any delimiter you prefer


                    }


                }

            }
            // Sort dates within each column to ensure proper ordering
            foreach (var column in itineraryColumns)
            {
                var sortedEntries = allDates
                    .ToDictionary(dateKey => dateKey, dateKey =>
                        column.ContainsKey(dateKey) ? column[dateKey] : "");
                column.Clear();
                foreach (var entry in sortedEntries.OrderBy(x => x.Key))
                {
                    column[entry.Key] = entry.Value;
                }
            }

            // Convert to view model
            var viewModel = itineraryColumns.Select((column, index) => new OperationItineraryViewModel
            {
                ColumnNumber = index + 1,
                DatesWithHeadings = column,
                DepartureId = ListofDepartureIds
            }).ToList();

            return View(viewModel);

        }


        [Route("Index")]
        public IActionResult Index(DateTime FromDate, DateTime ToDate)

        {
            List<SelectListItem> trekList = new List<SelectListItem>();

            var ListofDepartureIds = new List<int>();
            var ListofTrekIds = new List<int>();
            var ListOfStartDate = new List<DateTime>();


            // Fetch trek details for the dropdown

            var DeparturetTrekIds = _context.TrekDeparture

    .Select(t => new
    {
        t.TrekId,

    })
    .ToList();

            var TreksIds = DeparturetTrekIds.Select(d => d.TrekId).Distinct().ToList();
            trekList = _context.TrekDetails
  .Where(t => TreksIds.Contains(t.TrekId))
  .OrderBy(t => t.TrekName)
  .Select(t => new SelectListItem
  {
      Value = t.TrekId.ToString(), // Ensures TrekId is converted to string as required by SelectListItem
      Text = t.TrekName // Sets the display text
  })
  .ToList();

            ViewData["TrekList"] = trekList;

            // Get By Default One Month Data 

            var today = DateTime.Today;
            var oneMonthLater = today.AddMonths(1);
            var AllTrekDates = new HashSet<string>();



            var TrekItineraryColumns = new List<Dictionary<string, string>>();


            var trekDetails = _context.TrekItineraries
    .Include(t => t.ItineraryDetails) // Eager loading to fetch related ItineraryDetails
    .Select(t => new
    {
        t.TrekId,
        t.StartDate,
        t.EndDate,
        t.Id,
        t.ItineraryType,
        ItineraryDetails = t.ItineraryDetails // Directly use navigation property
            .Select(d => new
            {
                d.ItineraryDay,
                d.ItineraryHeading,
                d.TrekId
            })
            .ToList(),
        DaysCount = t.ItineraryDetails.Count() // Avoid extra DB call
    })
    .ToList();

            var fixedTrekDetails = trekDetails.Where(t => t.ItineraryType == "Fixed").ToList();
            var customizeTrekDetails = trekDetails.Where(t => t.ItineraryType == "Customize").ToList();


            var FixedTrekIdsdetails = trekDetails
.Where(t => t.ItineraryType == "Fixed")
.Select(t => t.TrekId)
.ToList();

            // Step 1: Get the TrekId and Id from the TrekItinerary table where ItineraryType is "Fixed"
            var FixedTrekIdsdetailsId = trekDetails
 .Where(t => t.ItineraryType == "Fixed")
 .Select(t => t.Id)
 .ToList();
            var AllTrekItineraries = _context.TrekDepartureItineraryDetail
                    .Where(td => FixedTrekIdsdetails.Contains(td.TrekId))
                    .Select(d => new
                    {
                        d.TrekId,
                        d.FromDate,
                        d.ToDate,
                        d.Id,
                        d.Batch,

                        ItineraryDetails = _context.TrekDepartureItinerary
                             .Where(td => EF.Property<int>(td, "TrekDepartureItineraryDetailId") == d.Id)
                            .Select(td => new
                            {
                                td.ItineraryDay,
                                td.ItineraryHeading,
                                td.TrekId

                            })
                            .ToList(),
                        DaysCount = _context.TrekDepartureItinerary
                             .Where(td => EF.Property<int>(td, "TrekDepartureItineraryDetailId") == d.Id)
                            .Count()
                    })
                    .ToList();
            var itineraryDetails = _context.ItineraryDetails
                 .Where(d => FixedTrekIdsdetailsId.Contains(d.TrekItineraryId))
                 .Select(d => new
                 {
                     d.ItineraryHeading,
                     d.ItineraryDay,
                     d.TrekId
                 })
                 .ToList();

            var trekIds = _context.TrekItineraries
.Where(td => td.ItineraryType == "Fixed") // Filter by the same TrekId

.Select(td => td.TrekId)
.ToList();

            var startDates = _context.TrekDeparture
     .Where(td => trekIds.Contains(td.TrekId) && (td.HideFrom == "none" || td.HideFrom == "website"))  // Filter by TrekId in trekIds list
     .OrderBy(d => d.StartDate)  // Sort by StartDate in ascending order
     .Select(d => new
     {
         d.DepartureId,
         d.Batch,
         d.StartDate,
         d.TrekId
     })
     .ToList();

            DateTime Today = DateTime.Today;
            var AllsevenDaysAgo = Today.AddDays(-7);
            var AllCoustomizeFromDates = customizeTrekDetails
                  .Where(fixedDate => fixedDate.StartDate >= AllsevenDaysAgo && fixedDate.StartDate < today)
                  .ToList();



            var afterAllCustomizeFromDates = customizeTrekDetails
                .Where(trekDetailsItinerary => trekDetailsItinerary.StartDate >= today && trekDetailsItinerary.StartDate <= oneMonthLater)
                .OrderBy(trekDetailsItinerary => trekDetailsItinerary.StartDate) // Sort by date (earliest first)
                .ToList();

            // Combine both lists
            var combinedAllCustomizeFromDates = AllCoustomizeFromDates.Concat(afterAllCustomizeFromDates).OrderBy(item => item.TrekId).ToList();




            var AllFixedFromDates = startDates
           .Where(fixedDate => fixedDate.StartDate >= AllsevenDaysAgo && fixedDate.StartDate < today)
           .ToList();

            var afterAllFixedFromDates = startDates
                    .Where(trekDetailsItinerary => trekDetailsItinerary.StartDate >= today && trekDetailsItinerary.StartDate <= oneMonthLater)
                    .OrderBy(trekDetailsItinerary => trekDetailsItinerary.StartDate) // Sort by date (earliest first)
                    .ToList();

            // Combine both lists
            var combinedAllFixedFromDates = AllFixedFromDates.Concat(afterAllFixedFromDates).OrderBy(item => item.TrekId).ToList();

            var trekItineraryDayCounts = _context.TrekItineraries
  .Where(ti => ti.ItineraryType == "Fixed") // Filter for "Fixed" itineraries
  .SelectMany(ti => _context.ItineraryDetails
      .Where(td => td.TrekItineraryId == ti.Id) // Match TrekItineraryId with Id from TrekItineraries
      .Select(td => new
      {
          td.TrekId,

          td.ItineraryDay // Fetch ItineraryDays for counting
      })
  )
  .GroupBy(td => td.TrekId) // Group by TrekId
  .Select(g => new
  {
      TrekId = g.Key,
      TotalDays = g.Count() // Count ItineraryDays for each TrekId
  })
  .ToList();



            foreach (var trekDetailsItinerary in combinedAllCustomizeFromDates)
            {
                var startDate = trekDetailsItinerary.StartDate.Value;
                for (int i = 0; i < trekDetailsItinerary.DaysCount; i++)
                {
                    var currentDate = startDate.AddDays(i);
                    AllTrekDates.Add(currentDate.ToString("yyyy-MM-dd"));
                }
            }

            foreach (var fixedDate in combinedAllFixedFromDates)
            {

                var existingItinerary = AllTrekItineraries
                    .FirstOrDefault(i => i.FromDate.Date == fixedDate.StartDate.Date);


                int daysToProcess = existingItinerary?.ItineraryDetails?.Count
                    ?? trekItineraryDayCounts.FirstOrDefault(t => t.TrekId == fixedDate.TrekId)?.TotalDays
                    ?? 0;


                for (int i = 0; i < daysToProcess; i++)
                {
                    var currentDate = fixedDate.StartDate.AddDays(i);
                    AllTrekDates.Add(currentDate.ToString("yyyy-MM-dd"));
                }
            }





            var CustomizetrekItineraryDayCountsWithNames = _context.TrekItineraries
.Where(td => td.ItineraryType == "Customize")
.Select(d => new
{
    d.TrekName,
    d.TrekShortName,
    d.TrekId
})
.GroupBy(t => t.TrekId)
.Select(g => g.First())
.ToList();


            var trekNameMapping = CustomizetrekItineraryDayCountsWithNames
       .ToDictionary(t => t.TrekId, t => (t.TrekShortName, t.TrekName));




            foreach (var trekDetailItinerary in combinedAllCustomizeFromDates)
            {
                if (trekDetailItinerary.StartDate == null)
                {
                    throw new InvalidOperationException("StartDate cannot be null.");
                }

                var allTeam = _context.AddTeamToTrek
           .Where(t => t.TrekId == trekDetailItinerary.TrekId && t.StartDate.Date == trekDetailItinerary.StartDate.Value.Date && t.Trekitinerary == "Customize")
        .Select(t => new { t.TrekId, t.StartDate })
           .ToList();

                bool hasTeamData = allTeam != null && allTeam.Count > 0;
                //adding departureIds for view
                foreach (var team in allTeam)
                {
                    ListofTrekIds.Add(team.TrekId);
                    ListOfStartDate.Add(team.StartDate);

                }


                ViewData["CustomizeTrekId"] = ListofTrekIds;
                ViewData["CustmoizeStartDate"] = ListOfStartDate;


                var startDate = trekDetailItinerary.StartDate.Value;
                var TrekId = trekDetailItinerary.TrekId;
                Dictionary<string, string> assignedColumn = null;

                // Check all existing columns for blank space
                foreach (var column in TrekItineraryColumns)
                {
                    var isBlank = true;

                    for (int i = 0; i < trekDetailItinerary.DaysCount; i++)
                    {
                        var currentDate = startDate.AddDays(i);
                        var dateKey = currentDate.ToString("yyyy-MM-dd");
                        if (column.ContainsKey(dateKey) && !string.IsNullOrEmpty(column[dateKey]))
                        {
                            isBlank = false;
                            break;
                        }
                    }

                    if (isBlank)
                    {
                        assignedColumn = column;
                        break;
                    }
                }

                // Create new column if needed
                if (assignedColumn == null)
                {
                    assignedColumn = new Dictionary<string, string>();
                    foreach (var dateKey in AllTrekDates)
                    {
                        assignedColumn[dateKey] = "";
                    }
                    TrekItineraryColumns.Add(assignedColumn);
                }
                var AlltrekName = trekNameMapping.ContainsKey(trekDetailItinerary.TrekId)
? trekNameMapping[trekDetailItinerary.TrekId] // Extract tuple values
: ("Unknown", "Unknown Trek");


                // Add customize itinerary details
                for (int i = 0; i < trekDetailItinerary.DaysCount; i++)
                {
                    var currentDate = startDate.AddDays(i);
                    var dateKey = currentDate.ToString("yyyy-MM-dd");
                    var dayKey = $"Day {i + 1}";

                    var itineraryDetail = trekDetailItinerary.ItineraryDetails?.FirstOrDefault(d => d.ItineraryDay == dayKey);
                    var heading = itineraryDetail?.ItineraryHeading ?? "No Heading";

                    if (i == 0)
                    {
                        heading = $"C - {AlltrekName.Item1}-{AlltrekName.Item2}-{heading}-{TrekId}";

                    }

                    assignedColumn[dateKey] = heading;

                }
            }



            var FixedtrekItineraryDayCountsWithNames = _context.TrekItineraries
.Where(td => td.ItineraryType == "Fixed")
.Select(d => new
{
    d.TrekName,
    d.TrekShortName,
    d.TrekId
})
.GroupBy(t => t.TrekId)
.Select(g => g.First())
.ToList();// Fetch as a list


            var FixedtrekNameMapping = FixedtrekItineraryDayCountsWithNames
                .ToDictionary(t => t.TrekId, t => (t.TrekShortName, t.TrekName));

            var maindepartureId = string.Empty;

            foreach (var startDateEntry in combinedAllFixedFromDates)
            {
                var startDate = startDateEntry.StartDate;
                var TrekId = startDateEntry.TrekId;
                var DepartureId = startDateEntry.DepartureId;
                var Batch = startDateEntry.Batch;
                var AllTrekers = _context.TempParticipants
    .Where(t => t.TrekId == TrekId
             && t.DepartureId == DepartureId
             && (t.BookingStatus == "Pending" || t.BookingStatus == "Transfer")
             && t.PaymentStatus == "Paid")
    .Count();

                var allTeam = _context.AddTeamToTrek
            .Where(t => t.TrekId == startDateEntry.TrekId && t.DepartureId == startDateEntry.DepartureId)
            .Select(t => new { t.Designation, t.TeamMemberName, t.DepartureId })
            .ToList();

                bool hasTeamData = allTeam != null && allTeam.Count > 0;
                //adding departureIds for view
                foreach (var team in allTeam)
                {
                    ListofDepartureIds.Add(team.DepartureId ?? 0);
                }

                ViewData["DepartureId"] = ListofDepartureIds;

                var existingItinerary = AllTrekItineraries
                    .FirstOrDefault(i => i.FromDate.Date == startDate.Date);

                Dictionary<string, string> assignedColumn = null;

                int defaultDaysToProcess = trekItineraryDayCounts
                    .FirstOrDefault(t => t.TrekId == startDateEntry.TrekId)?.TotalDays ?? 0;

                int daysToProcess = existingItinerary?.ItineraryDetails.Count ?? defaultDaysToProcess;

                foreach (var column in TrekItineraryColumns)
                {
                    bool isBlank = true;

                    for (int i = 0; i < daysToProcess; i++)
                    {
                        var currentDate = startDate.AddDays(i);
                        var dateKey = currentDate.ToString("yyyy-MM-dd");

                        if (column.ContainsKey(dateKey) && !string.IsNullOrEmpty(column[dateKey]))
                        {
                            isBlank = false;
                            break;
                        }
                    }

                    if (isBlank)
                    {
                        assignedColumn = column;
                        break;
                    }
                }

                if (assignedColumn == null)
                {
                    assignedColumn = AllTrekDates.ToDictionary(dateKey => dateKey, _ => "");
                    TrekItineraryColumns.Add(assignedColumn);
                }

                var AlltrekName = FixedtrekNameMapping.ContainsKey(startDateEntry.TrekId)
                    ? FixedtrekNameMapping[startDateEntry.TrekId]
                    : ("Unknown", "Unknown Trek");

                for (int i = 0; i < daysToProcess; i++)
                {
                    var currentDate = startDate.AddDays(i);
                    var dateKey = currentDate.ToString("yyyy-MM-dd");
                    string heading;
                    string departureId;

                    if (existingItinerary != null)
                    {
                        var itineraryDetail = existingItinerary.ItineraryDetails.ElementAtOrDefault(i);
                        heading = itineraryDetail?.ItineraryHeading ?? "No Heading";

                    }
                    else
                    {
                        var itineraryDetail = itineraryDetails
                            .Where(i => i.TrekId == startDateEntry.TrekId)
                            .ElementAtOrDefault(i);
                        heading = itineraryDetail?.ItineraryHeading ?? "No Heading";
                    }

                    if (i == 0)
                    {
                        heading = $"F - ({AllTrekers})-{AlltrekName.Item1}-{AlltrekName.Item2}-{heading}-({Batch})-({DepartureId})";
                        maindepartureId = $"({DepartureId})";
                    }

                    assignedColumn[dateKey] = heading;
                }
            }




            // Method to get TrekName by TrekId


            var ViewModel = TrekItineraryColumns.Select((column, index) => new OperationItineraryViewModel
            {
                ColumnNumber = index + 1,
                DatesWithHeadings = column,

                // You can also pass Batch and TrekName if needed, for example:
                //Batch = Batch,                 // Pass Batch
                //TrekName = trekName           // Pass TrekName (formatted from AlltrekName)
            }).ToList();

            return View(ViewModel);
        }

        [HttpPost]
        [Route("Index")]
        public IActionResult Index(string[] trekId, string TeamRoll, string Heading, DateTime? FromDate, DateTime? ToDate, string itineraryType)
        {
            int? NullTrekId = null; // Define NullTrekId as nullable int
            var ListofDepartureIds = new List<int>();
            var ListofTrekIds = new List<int>();
            var ListOfStartDate = new List<DateTime>();


            int[] selectedtrekIds = trekId != null && trekId.Any()
                ? trekId.Where(id => !string.IsNullOrWhiteSpace(id)) // Remove empty values
                        .Select(id => Convert.ToInt32(id))
                        .ToArray()
                : null;






            DateTime today = DateTime.Today;
            var DeparturetTrekIds = _context.TrekDeparture

   .Select(t => new
   {
       t.TrekId,

   })
   .ToList();

            var TreksIds = DeparturetTrekIds.Select(d => d.TrekId).Distinct().ToList();
            var trekList = _context.TrekDetails
  .Where(t => TreksIds.Contains(t.TrekId))
  .OrderBy(t => t.TrekName)
  .Select(t => new SelectListItem
  {
      Value = t.TrekId.ToString(), // Ensures TrekId is converted to string as required by SelectListItem
      Text = t.TrekName, // Sets the display text
      Selected = selectedtrekIds != null && selectedtrekIds.Contains(t.TrekId)
  })
  .ToList();

            ViewData["TrekList"] = trekList;
            ViewData["SelectedTrekId"] = trekId;





            var AllDesignation = _context.Users
        .Where(t => t.Position != null)  // Filter out null values
        .Select(t => t.Position)         // Select Position values
        .Distinct()                      // Get distinct values
        .Select(p => new SelectListItem
        {
            Value = p.ToString(),
            Text = p
        })
        .ToList();


            ViewData["AllDesignation"] = AllDesignation;




            //here we chcek AllTrek with Dates and without Dates 
            if (trekId == null || trekId.All(string.IsNullOrWhiteSpace))
            {


                var TeamviewModel = new List<OperationItineraryViewModel>();

                if (!string.IsNullOrEmpty(TeamRoll))
                {
                    var teamMembers = _context.Users
   .Where(t => t.Position == TeamRoll)
   .Select(t => new SelectListItem
   {
       Value = t.FirstName + " " + t.LastName,
       Text = t.FirstName + " " + t.LastName,
   })
   .ToList();

                    return Json(teamMembers);

                }





                var trekDetails = _context.TrekItineraries
    .Include(t => t.ItineraryDetails) // Eager loading to fetch related ItineraryDetails
    .Select(t => new
    {
        t.TrekId,
        t.StartDate,
        t.EndDate,
        t.Id,
        t.ItineraryType,
        ItineraryDetails = t.ItineraryDetails // Directly use navigation property
            .Select(d => new
            {
                d.ItineraryDay,
                d.ItineraryHeading,
                d.TrekId
            })
            .ToList(),
        DaysCount = t.ItineraryDetails.Count() // Avoid extra DB call
    })
    .ToList();


                var customizeTrekDetails = trekDetails.Where(t => t.ItineraryType == "Customize").ToList();

                var fixedTrekDetails = trekDetails.Where(t => t.ItineraryType == "Fixed").ToList();


                var FixedTrekIdsdetails = trekDetails
    .Where(t => t.ItineraryType == "Fixed")
    .Select(t => t.TrekId)
    .ToList();

                // Step 1: Get the TrekId and Id from the TrekItinerary table where ItineraryType is "Fixed"
                var FixedTrekIdsdetailsId = trekDetails
     .Where(t => t.ItineraryType == "Fixed")
     .Select(t => t.Id)
     .ToList();


                // Step 2: Use the retrieved Id values to query the ItineraryDetails table
                var itineraryDetails = _context.ItineraryDetails
                    .Where(d => FixedTrekIdsdetailsId.Contains(d.TrekItineraryId))
                    .Select(d => new
                    {
                        d.ItineraryHeading,
                        d.ItineraryDay,
                        d.TrekId
                    })
                    .ToList();



                var AllTrekFixedItineraries = _context.ItineraryDetails
.Where(td => FixedTrekIdsdetails.Contains(td.TrekId))
.Select(d => new
{
    d.ItineraryDay,
    d.ItineraryHeading
})
.ToList();



                var AllTrekItineraries = _context.TrekDepartureItineraryDetail
                    .Where(td => FixedTrekIdsdetails.Contains(td.TrekId))
                    .Select(d => new
                    {
                        d.TrekId,
                        d.FromDate,
                        d.ToDate,
                        d.Id,
                        d.Batch,

                        ItineraryDetails = _context.TrekDepartureItinerary
                             .Where(td => EF.Property<int>(td, "TrekDepartureItineraryDetailId") == d.Id)
                            .Select(td => new
                            {
                                td.ItineraryDay,
                                td.ItineraryHeading,
                                td.TrekId

                            })
                            .ToList(),
                        DaysCount = _context.TrekDepartureItinerary
                             .Where(td => EF.Property<int>(td, "TrekDepartureItineraryDetailId") == d.Id)
                            .Count()
                    })
                    .ToList();










                var trekIds = _context.TrekItineraries
    .Where(td => td.ItineraryType == "Fixed") // Filter by the same TrekId

    .Select(td => td.TrekId)
    .ToList();
                var startDates = _context.TrekDeparture
  .Where(td => trekIds.Contains(td.TrekId) && (td.HideFrom == "none" || td.HideFrom == "website"))  // Filter by TrekId in trekIds list
  .OrderBy(d => d.StartDate)  // Sort by StartDate in ascending order
  .Select(d => new
  {
      d.DepartureId,
      d.Batch,
      d.StartDate,
      d.TrekId
  })
  .ToList();


                var trekItineraryDayCounts = _context.TrekItineraries
      .Where(ti => ti.ItineraryType == "Fixed") // Filter for "Fixed" itineraries
      .SelectMany(ti => _context.ItineraryDetails
          .Where(td => td.TrekItineraryId == ti.Id) // Match TrekItineraryId with Id from TrekItineraries
          .Select(td => new
          {
              td.TrekId,

              td.ItineraryDay // Fetch ItineraryDays for counting
          })
      )
      .GroupBy(td => td.TrekId) // Group by TrekId
      .Select(g => new
      {
          TrekId = g.Key,
          TotalDays = g.Count() // Count ItineraryDays for each TrekId
      })
      .ToList();




                var TrekName = _context.TrekDetails
           .Where(t => selectedtrekIds.Contains(t.TrekId))
           .Select(t => t.TrekName)
           .FirstOrDefault() ?? "Unknown Trek";

                var TrekItineraryColumns = new List<Dictionary<string, string>>();
                var AllTrekDates = new HashSet<string>();


                DateTime Today = DateTime.Today;
                var AllsevenDaysAgo = Today.AddDays(-7);


                DateTime AllFromDateAgo = (FromDate ?? Today).AddDays(-7);


                //Here we get All Trek Data by FromDate and ToDate

                if (FromDate.HasValue && ToDate.HasValue && (trekId == null || trekId.All(string.IsNullOrWhiteSpace)))


                {



                    // Filter the list for items where StartDate is within the last 7 days
                    var AllCDateBSD = customizeTrekDetails
                        .Where(fixedDate => fixedDate.StartDate >= AllFromDateAgo && fixedDate.StartDate < FromDate)
                        .ToList();


                    var AllCDateASD = customizeTrekDetails
    .Where(trekDetailsItinerary => trekDetailsItinerary.StartDate >= FromDate && trekDetailsItinerary.StartDate <= ToDate)
    .OrderBy(trekDetailsItinerary => trekDetailsItinerary.StartDate) // Sort by date (earliest first)
    .ToList();


                    // Combine both lists
                    var CombineBothCustomize = AllCDateBSD.Concat(AllCDateASD).OrderBy(item => item.TrekId).ToList();




                    var AllFDateBSD = startDates
                   .Where(fixedDate => fixedDate.StartDate >= AllFromDateAgo && fixedDate.StartDate < FromDate)
                   .ToList();


                    var AllDDateASD = startDates
                        .Where(trekDetailsItinerary => trekDetailsItinerary.StartDate >= FromDate && trekDetailsItinerary.StartDate <= ToDate)
                        .OrderBy(trekDetailsItinerary => trekDetailsItinerary.StartDate) // Sort by date (earliest first)
                        .ToList();

                    // Combine both lists
                    var CombineBothFixed = AllFDateBSD.Concat(AllDDateASD).OrderBy(item => item.TrekId).ToList();


                    if (itineraryType == "Customize" || itineraryType == "All" || itineraryType == null)
                    {
                        foreach (var trekDetailsItinerary in CombineBothCustomize)
                        {
                            var startDate = trekDetailsItinerary.StartDate.Value;
                            for (int i = 0; i < trekDetailsItinerary.DaysCount; i++)
                            {
                                var currentDate = startDate.AddDays(i);
                                AllTrekDates.Add(currentDate.ToString("yyyy-MM-dd"));
                            }
                        }

                        var CustomizeIDC = _context.TrekItineraries
   .Where(td => td.ItineraryType == "Customize")
   .Select(d => new
   {
       d.TrekName,
       d.TrekShortName,
       d.TrekId
   })
   .GroupBy(t => t.TrekId)
   .Select(g => g.First())
   .ToList();


                        var TNMapping = CustomizeIDC
                   .ToDictionary(t => t.TrekId, t => (t.TrekShortName, t.TrekName));



                        foreach (var trekDetailItinerary in CombineBothCustomize)
                        {
                            if (trekDetailItinerary.StartDate == null)
                            {
                                throw new InvalidOperationException("StartDate cannot be null.");
                            }

                            var startDate = trekDetailItinerary.StartDate.Value;

                            var EndDate = trekDetailItinerary.EndDate.Value;
                            var AllTrekId = trekDetailItinerary.TrekId;

                            var allTeam = _context.AddTeamToTrek
          .Where(t => t.TrekId == trekDetailItinerary.TrekId && t.StartDate.Date == trekDetailItinerary.StartDate.Value.Date && t.Trekitinerary == "Customize")
       .Select(t => new { t.TrekId, t.StartDate })
          .ToList();

                            bool hasTeamData = allTeam != null && allTeam.Count > 0;
                            //adding departureIds for view
                            foreach (var team in allTeam)
                            {
                                ListofTrekIds.Add(team.TrekId);
                                ListOfStartDate.Add(team.StartDate);

                            }


                            ViewData["CustomizeTrekId"] = ListofTrekIds;
                            ViewData["CustmoizeStartDate"] = ListOfStartDate;

                            Dictionary<string, string> assignedColumn = null;

                            // Check all existing columns for blank space
                            foreach (var column in TrekItineraryColumns)
                            {
                                var isBlank = true;

                                for (int i = 0; i < trekDetailItinerary.DaysCount; i++)
                                {
                                    var currentDate = startDate.AddDays(i);
                                    var dateKey = currentDate.ToString("yyyy-MM-dd");
                                    if (column.ContainsKey(dateKey) && !string.IsNullOrEmpty(column[dateKey]))
                                    {
                                        isBlank = false;
                                        break;
                                    }
                                }

                                if (isBlank)
                                {
                                    assignedColumn = column;
                                    break;
                                }
                            }

                            // Create new column if needed
                            if (assignedColumn == null)
                            {
                                assignedColumn = new Dictionary<string, string>();
                                foreach (var dateKey in AllTrekDates)
                                {
                                    assignedColumn[dateKey] = "";
                                }
                                TrekItineraryColumns.Add(assignedColumn);
                            }
                            var AlltrekName = TNMapping.ContainsKey(trekDetailItinerary.TrekId)
            ? TNMapping[trekDetailItinerary.TrekId] // Extract tuple values
            : ("Unknown", "Unknown Trek");




                            // Add customize itinerary details
                            for (int i = 0; i < trekDetailItinerary.DaysCount; i++)
                            {
                                var currentDate = startDate.AddDays(i);
                                var dateKey = currentDate.ToString("yyyy-MM-dd");
                                var dayKey = $"Day {i + 1}";

                                var itineraryDetail = trekDetailItinerary.ItineraryDetails?.FirstOrDefault(d => d.ItineraryDay == dayKey);
                                var heading = itineraryDetail?.ItineraryHeading ?? "No Heading";

                                if (i == 0)
                                {
                                    heading = $"C - {AlltrekName.Item1}-{AlltrekName.Item2}-{heading}-{AllTrekId}";

                                }

                                assignedColumn[dateKey] = heading;
                            }
                        }

                    }

                    if (itineraryType == "Fixed" || itineraryType == "All" || itineraryType == null)
                    {
                        foreach (var fixedDate in CombineBothFixed)
                        {

                            var existingItinerary = AllTrekItineraries
                                .FirstOrDefault(i => i.FromDate.Date == fixedDate.StartDate.Date);


                            int daysToProcess = existingItinerary?.ItineraryDetails?.Count
                                ?? trekItineraryDayCounts.FirstOrDefault(t => t.TrekId == fixedDate.TrekId)?.TotalDays
                                ?? 0;


                            for (int i = 0; i < daysToProcess; i++)
                            {
                                var currentDate = fixedDate.StartDate.AddDays(i);
                                AllTrekDates.Add(currentDate.ToString("yyyy-MM-dd"));
                            }
                        }



                        //This is for Customize All Trek Data By FromDate and ToDate 




                        var FixedIDC = _context.TrekItineraries
        .Where(td => td.ItineraryType == "Fixed")
        .Select(d => new
        {
            d.TrekName,
            d.TrekShortName,
            d.TrekId
        })
        .GroupBy(t => t.TrekId)
        .Select(g => g.First())
        .ToList();// Fetch as a list


                        var FixedTNMapping = FixedIDC
                            .ToDictionary(t => t.TrekId, t => (t.TrekShortName, t.TrekName));

                        var FixedDepartureId = string.Empty;


                        //This is for Fixed All Trek Data By FromDate and ToDate 
                        foreach (var startDateEntry in CombineBothFixed)
                        {
                            var startDate = startDateEntry.StartDate;
                            var TrekId = startDateEntry.TrekId;
                            var DepartureId = startDateEntry.DepartureId;
                            var Batch = startDateEntry.Batch;
                            var AllTrekers = _context.TempParticipants
                                .Where(t => t.TrekId == TrekId && t.DepartureId == DepartureId && t.BookingStatus == "Pending" && t.PaymentStatus == "Paid")
                                .Count();

                            var allTeam = _context.AddTeamToTrek
            .Where(t => t.TrekId == startDateEntry.TrekId && t.DepartureId == startDateEntry.DepartureId)
            .Select(t => new { t.Designation, t.TeamMemberName, t.DepartureId })
            .ToList();

                            bool hasTeamData = allTeam != null && allTeam.Count > 0;
                            //adding departureIds for view
                            foreach (var team in allTeam)
                            {
                                ListofDepartureIds.Add(team.DepartureId ?? 0);
                            }

                            ViewData["DepartureId"] = ListofDepartureIds;


                            var existingItinerary = AllTrekItineraries
                                .FirstOrDefault(i => i.FromDate.Date == startDate.Date);

                            Dictionary<string, string> assignedColumn = null;

                            int defaultDaysToProcess = trekItineraryDayCounts
                                .FirstOrDefault(t => t.TrekId == startDateEntry.TrekId)?.TotalDays ?? 0;

                            int daysToProcess = existingItinerary?.ItineraryDetails.Count ?? defaultDaysToProcess;

                            foreach (var column in TrekItineraryColumns)
                            {
                                bool isBlank = true;

                                for (int i = 0; i < daysToProcess; i++)
                                {
                                    var currentDate = startDate.AddDays(i);
                                    var dateKey = currentDate.ToString("yyyy-MM-dd");

                                    if (column.ContainsKey(dateKey) && !string.IsNullOrEmpty(column[dateKey]))
                                    {
                                        isBlank = false;
                                        break;
                                    }
                                }

                                if (isBlank)
                                {
                                    assignedColumn = column;
                                    break;
                                }
                            }

                            if (assignedColumn == null)
                            {
                                assignedColumn = AllTrekDates.ToDictionary(dateKey => dateKey, _ => "");
                                TrekItineraryColumns.Add(assignedColumn);
                            }

                            var AlltrekName = FixedTNMapping.ContainsKey(startDateEntry.TrekId)
                                ? FixedTNMapping[startDateEntry.TrekId]
                                : ("Unknown", "Unknown Trek");

                            for (int i = 0; i < daysToProcess; i++)
                            {
                                var currentDate = startDate.AddDays(i);
                                var dateKey = currentDate.ToString("yyyy-MM-dd");
                                string heading;
                                string departureId;

                                if (existingItinerary != null)
                                {
                                    var itineraryDetail = existingItinerary.ItineraryDetails.ElementAtOrDefault(i);
                                    heading = itineraryDetail?.ItineraryHeading ?? "No Heading";

                                }
                                else
                                {
                                    var itineraryDetail = itineraryDetails
                                        .Where(i => i.TrekId == startDateEntry.TrekId)
                                        .ElementAtOrDefault(i);
                                    heading = itineraryDetail?.ItineraryHeading ?? "No Heading";
                                }

                                if (i == 0)
                                {
                                    heading = $"F - ({AllTrekers})-{AlltrekName.Item1}-{AlltrekName.Item2}-{heading}-({Batch})-({DepartureId})";
                                    FixedDepartureId = $"({DepartureId})";
                                }

                                assignedColumn[dateKey] = heading;
                            }
                        }

                    }


                    // Method to get TrekName by TrekId


                    var AllViewModel = TrekItineraryColumns.Select((column, index) => new OperationItineraryViewModel
                    {
                        ColumnNumber = index + 1,
                        DatesWithHeadings = column,
                        DepartureId = ListofDepartureIds,  // Pass DepartureId (now it's in scope)
                                                           // You can also pass Batch and TrekName if needed, for example:
                                                           //Batch = Batch,                 // Pass Batch
                                                           //TrekName = trekName           // Pass TrekName (formatted from AlltrekName)
                    }).ToList();
                    return View(AllViewModel);



                }
                if (trekId == null || trekId.All(string.IsNullOrWhiteSpace))
                {

                    // Filter the list for items where StartDate is within the last 7 days
                    var AllCoustomizeFromDates = customizeTrekDetails
                        .Where(fixedDate => fixedDate.StartDate >= AllsevenDaysAgo && fixedDate.StartDate < today)
                        .ToList();



                    var afterAllCustomizeFromDates = customizeTrekDetails
                        .Where(trekDetailsItinerary => trekDetailsItinerary.StartDate >= today)
                        .OrderBy(trekDetailsItinerary => trekDetailsItinerary.StartDate) // Sort by date (earliest first)
                        .ToList();

                    // Combine both lists
                    var combinedAllCustomizeFromDates = AllCoustomizeFromDates.Concat(afterAllCustomizeFromDates).OrderBy(item => item.TrekId).ToList();




                    var AllFixedFromDates = startDates
                   .Where(fixedDate => fixedDate.StartDate >= AllsevenDaysAgo && fixedDate.StartDate < today)
                   .ToList();


                    var afterAllFixedFromDates = startDates
                        .Where(trekDetailsItinerary => trekDetailsItinerary.StartDate >= today)
                        .OrderBy(trekDetailsItinerary => trekDetailsItinerary.StartDate) // Sort by date (earliest first)
                        .ToList();

                    // Combine both lists
                    var combinedAllFixedFromDates = AllFixedFromDates.Concat(afterAllFixedFromDates).OrderBy(item => item.TrekId).ToList();




                    var AllTrekItineraryColumns = new List<Dictionary<string, string>>();


                    if (itineraryType == "Fixed" || itineraryType == "All" || itineraryType == null)
                    {

                        foreach (var fixedDate in combinedAllFixedFromDates)
                        {

                            var existingItinerary = AllTrekItineraries
                                .FirstOrDefault(i => i.FromDate.Date == fixedDate.StartDate.Date);


                            int daysToProcess = existingItinerary?.ItineraryDetails?.Count
                                ?? trekItineraryDayCounts.FirstOrDefault(t => t.TrekId == fixedDate.TrekId)?.TotalDays
                                ?? 0;


                            for (int i = 0; i < daysToProcess; i++)
                            {
                                var currentDate = fixedDate.StartDate.AddDays(i);
                                AllTrekDates.Add(currentDate.ToString("yyyy-MM-dd"));
                            }
                        }











                        var FixedtrekItineraryDayCountsWithNames = _context.TrekItineraries
        .Where(td => td.ItineraryType == "Fixed")
        .Select(d => new
        {
            d.TrekName,
            d.TrekShortName,
            d.TrekId
        })
        .GroupBy(t => t.TrekId)
        .Select(g => g.First())
        .ToList();// Fetch as a list


                        var FixedtrekNameMapping = FixedtrekItineraryDayCountsWithNames
                            .ToDictionary(t => t.TrekId, t => (t.TrekShortName, t.TrekName));

                        var maindepartureId = string.Empty;



                        //Here we Get All The Fixed Data Without Choosing FromDate and ToDate
                        foreach (var startDateEntry in combinedAllFixedFromDates)
                        {
                            var startDate = startDateEntry.StartDate;
                            var TrekId = startDateEntry.TrekId;
                            var DepartureId = startDateEntry.DepartureId;
                            var Batch = startDateEntry.Batch;
                            var AllTrekers = _context.TempParticipants
                                .Where(t => t.TrekId == TrekId && t.DepartureId == DepartureId && t.BookingStatus == "Pending" && t.PaymentStatus == "Paid")
                                .Count();

                            var allTeam = _context.AddTeamToTrek
            .Where(t => t.TrekId == startDateEntry.TrekId && t.DepartureId == startDateEntry.DepartureId)
            .Select(t => new { t.Designation, t.TeamMemberName, t.DepartureId })
            .ToList();

                            bool hasTeamData = allTeam != null && allTeam.Count > 0;
                            //adding departureIds for view
                            foreach (var team in allTeam)
                            {
                                ListofDepartureIds.Add(team.DepartureId ?? 0);
                            }

                            ViewData["DepartureId"] = ListofDepartureIds;


                            var existingItinerary = AllTrekItineraries
                                .FirstOrDefault(i => i.FromDate.Date == startDate.Date);

                            Dictionary<string, string> assignedColumn = null;

                            int defaultDaysToProcess = trekItineraryDayCounts
                                .FirstOrDefault(t => t.TrekId == startDateEntry.TrekId)?.TotalDays ?? 0;

                            int daysToProcess = existingItinerary?.ItineraryDetails.Count ?? defaultDaysToProcess;

                            foreach (var column in AllTrekItineraryColumns)
                            {
                                bool isBlank = true;

                                for (int i = 0; i < daysToProcess; i++)
                                {
                                    var currentDate = startDate.AddDays(i);
                                    var dateKey = currentDate.ToString("yyyy-MM-dd");

                                    if (column.ContainsKey(dateKey) && !string.IsNullOrEmpty(column[dateKey]))
                                    {
                                        isBlank = false;
                                        break;
                                    }
                                }

                                if (isBlank)
                                {
                                    assignedColumn = column;
                                    break;
                                }
                            }

                            if (assignedColumn == null)
                            {
                                assignedColumn = AllTrekDates.ToDictionary(dateKey => dateKey, _ => "");
                                AllTrekItineraryColumns.Add(assignedColumn);
                            }

                            var AlltrekName = FixedtrekNameMapping.ContainsKey(startDateEntry.TrekId)
                            ? FixedtrekNameMapping[startDateEntry.TrekId]
                            : ("Unknown", "Unknown Trek");

                            for (int i = 0; i < daysToProcess; i++)
                            {
                                var currentDate = startDate.AddDays(i);
                                var dateKey = currentDate.ToString("yyyy-MM-dd");
                                string heading;
                                string departureId;

                                if (existingItinerary != null)
                                {
                                    var itineraryDetail = existingItinerary.ItineraryDetails.ElementAtOrDefault(i);
                                    heading = itineraryDetail?.ItineraryHeading ?? "No Heading";

                                }
                                else
                                {
                                    var itineraryDetail = itineraryDetails
                                        .Where(i => i.TrekId == startDateEntry.TrekId)
                                        .ElementAtOrDefault(i);
                                    heading = itineraryDetail?.ItineraryHeading ?? "No Heading";
                                }

                                if (i == 0)
                                {
                                    heading = $"F - ({AllTrekers})-{AlltrekName.Item1}-{AlltrekName.Item2}-{heading}-({Batch})-({DepartureId})";
                                    maindepartureId = $"({DepartureId})";
                                }

                                assignedColumn[dateKey] = heading;
                            }
                        }

                    }

                    if (itineraryType == "Customize" || itineraryType == "All" || itineraryType == null)
                    {

                        foreach (var trekDetailsItinerary in combinedAllCustomizeFromDates)
                        {
                            var startDate = trekDetailsItinerary.StartDate.Value;
                            for (int i = 0; i < trekDetailsItinerary.DaysCount; i++)
                            {
                                var currentDate = startDate.AddDays(i);
                                AllTrekDates.Add(currentDate.ToString("yyyy-MM-dd"));
                            }
                        }


                        var CustomizetrekItineraryDayCountsWithNames = _context.TrekItineraries
    .Where(td => td.ItineraryType == "Customize")
    .Select(d => new
    {
        d.TrekName,
        d.TrekShortName,
        d.TrekId
    })
    .GroupBy(t => t.TrekId)
    .Select(g => g.First())
    .ToList();


                        var trekNameMapping = CustomizetrekItineraryDayCountsWithNames
                   .ToDictionary(t => t.TrekId, t => (t.TrekShortName, t.TrekName));


                        //Here we get All the Customize data without choosing FromDate and ToDate

                        foreach (var trekDetailItinerary in combinedAllCustomizeFromDates)
                        {
                            if (trekDetailItinerary.StartDate == null)
                            {
                                throw new InvalidOperationException("StartDate cannot be null.");
                            }

                            var startDate = trekDetailItinerary.StartDate.Value;
                            var AllTrekId = trekDetailItinerary.TrekId;
                            var allTeam = _context.AddTeamToTrek
            .Where(t => t.TrekId == trekDetailItinerary.TrekId && t.StartDate.Date == trekDetailItinerary.StartDate.Value.Date && t.Trekitinerary == "Customize")
         .Select(t => new { t.TrekId, t.StartDate })
            .ToList();

                            bool hasTeamData = allTeam != null && allTeam.Count > 0;
                            //adding departureIds for view
                            foreach (var team in allTeam)
                            {
                                ListofTrekIds.Add(team.TrekId);
                                ListOfStartDate.Add(team.StartDate);

                            }


                            ViewData["CustomizeTrekId"] = ListofTrekIds;
                            ViewData["CustmoizeStartDate"] = ListOfStartDate;

                            Dictionary<string, string> assignedColumn = null;

                            // Check all existing columns for blank space
                            foreach (var column in AllTrekItineraryColumns)
                            {
                                var isBlank = true;

                                for (int i = 0; i < trekDetailItinerary.DaysCount; i++)
                                {
                                    var currentDate = startDate.AddDays(i);
                                    var dateKey = currentDate.ToString("yyyy-MM-dd");
                                    if (column.ContainsKey(dateKey) && !string.IsNullOrEmpty(column[dateKey]))
                                    {
                                        isBlank = false;
                                        break;
                                    }
                                }

                                if (isBlank)
                                {
                                    assignedColumn = column;
                                    break;
                                }
                            }

                            // Create new column if needed
                            if (assignedColumn == null)
                            {
                                assignedColumn = new Dictionary<string, string>();
                                foreach (var dateKey in AllTrekDates)
                                {
                                    assignedColumn[dateKey] = "";
                                }
                                AllTrekItineraryColumns.Add(assignedColumn);
                            }
                            var AlltrekName = trekNameMapping.ContainsKey(trekDetailItinerary.TrekId)
            ? trekNameMapping[trekDetailItinerary.TrekId] // Extract tuple values
            : ("Unknown", "Unknown Trek");


                            // Add customize itinerary details
                            for (int i = 0; i < trekDetailItinerary.DaysCount; i++)
                            {
                                var currentDate = startDate.AddDays(i);
                                var dateKey = currentDate.ToString("yyyy-MM-dd");
                                var dayKey = $"Day {i + 1}";

                                var itineraryDetail = trekDetailItinerary.ItineraryDetails?.FirstOrDefault(d => d.ItineraryDay == dayKey);
                                var heading = itineraryDetail?.ItineraryHeading ?? "No Heading";

                                if (i == 0)
                                {
                                    heading = $"C - {AlltrekName.Item1}-{AlltrekName.Item2}-{heading}-{AllTrekId}";

                                }

                                assignedColumn[dateKey] = heading;
                            }
                        }

                    }




                    // Method to get TrekName by TrekId

                    var ViewModel = AllTrekItineraryColumns.Select((column, index) => new OperationItineraryViewModel
                    {
                        ColumnNumber = index + 1,
                        DatesWithHeadings = column,
                        //DepartureId = ListofDepartureIds,
                    }).ToList();
                    return View(ViewModel);
                }
            }

            var trekItineraries = _context.TrekItineraries
       .Where(t => selectedtrekIds.Contains(t.TrekId))
       .GroupBy(t => new { t.Id, t.StartDate, t.ItineraryType, t.TrekId }) // Group by ItineraryType
     .Select(group => new
     {
         Id = group.Key.Id,
         TrekId = group.Key.TrekId,
         StartDate = group.Key.StartDate,
         ItineraryType = group.Key.ItineraryType,
         DayCount = group.Max(g => g.ItineraryDayCount),
         ItineraryDetails = group.Key.ItineraryType == "Customize"
? group.SelectMany(g => g.ItineraryDetails.Select(i => new ItineraryDetail
{
    ItineraryDay = i.ItineraryDay,
    ItineraryHeading = i.ItineraryHeading
})).ToList()
: new List<ItineraryDetail>() // Empty list for Fixed
     })

       .ToList();
            var customizeItineraries = trekItineraries.Where(t => t.ItineraryType == "Customize").ToList();
            var fixedItineraries = trekItineraries.Where(t => t.ItineraryType == "Fixed").ToList();

            List<DateTime> fixedStartDates = new List<DateTime>();




            var fixedFromDates = _context.TrekDeparture
    .Where(td => selectedtrekIds.Contains(td.TrekId) && (td.HideFrom == "none" || td.HideFrom == "website"))
    .OrderBy(td => td.StartDate) // Corrected order by
    .Select(td => new
    {
        td.StartDate,
        td.TrekId,
        td.DepartureId,
        td.Batch,
    })
    .ToList();



            var validFixedItineraries = fixedFromDates
    .Where(d => d.StartDate != default(DateTime))
    .ToList();


            var allItineraries = _context.TrekDepartureItineraryDetail
      .Where(tdi => selectedtrekIds.Contains(tdi.TrekId))
      .Include(tdi => tdi.TrekDepartureItinerary)
      .ToList();


            var itineraryDaysByFromDate = allItineraries
                .GroupBy(tdi => new { tdi.FromDate, tdi.TrekId })
                .Select(group => new
                {
                    TrekId = group.Key.TrekId,
                    FromDate = group.Key.FromDate,

                    ItineraryDetails = group.SelectMany(tdi => tdi.TrekDepartureItinerary)
                        .Select(it => new { it.ItineraryDay, it.ItineraryHeading, it.TrekId })
                        .ToList()
                })
                .OrderBy(result => result.FromDate)
            .ToList();

            var fixedItineraryDetails = _context.TrekItineraries
                .Where(t => selectedtrekIds.Contains(t.TrekId) && t.ItineraryType == "Fixed")
                .SelectMany(ti => _context.ItineraryDetails
                    .Where(td => td.TrekItineraryId == ti.Id)
                    .Select(td => new
                    {
                        td.ItineraryDay,
                        td.ItineraryHeading,
                        td.TrekId
                    })
                )
                .ToList();



            //Here we Get data By TrekId and FromDate and ToDate

            if (FromDate.HasValue && ToDate.HasValue && trekId != null)

            {

                DateTime FromDateAgo = FromDate.Value.AddDays(-7); // Subtract 7 days from FromDate

                var filteredCoustomizeDates = customizeItineraries
                    .Where(fixedDate => fixedDate.StartDate >= FromDateAgo && fixedDate.StartDate <= FromDate.Value) // Using FromDate.Value
                    .ToList();


                var afterfilteredCustomizeDates = customizeItineraries
                    .Where(trekItinerary => trekItinerary.StartDate >= FromDate && trekItinerary.StartDate <= ToDate)
                    .OrderBy(trekItinerary => trekItinerary.StartDate) // Sort by date (earliest first)
                    .ToList();

                // Combine both lists
                var combinedCustomizeDates = filteredCoustomizeDates.Concat(afterfilteredCustomizeDates).OrderBy(item => item.TrekId).ToList();



                var filteredFixedDates = fixedFromDates
               .Where(fixedDate => fixedDate.StartDate >= FromDateAgo && fixedDate.StartDate <= FromDate)
               .ToList();



                var afterfilteredFixedDates = _context.TrekDeparture
     .Where(td => selectedtrekIds.Contains(td.TrekId)
         && (td.HideFrom == "none" || td.HideFrom == "website")

         && td.StartDate > FromDate
         && td.StartDate <= ToDate)
     .OrderBy(td => td.StartDate) // Corrected order by
     .Select(td => new
     {
         td.StartDate,
         td.TrekId,
         td.DepartureId,
         td.Batch,
     })
     .ToList();

                // Combine both lists
                var combinedFixedDates = filteredFixedDates.Concat(afterfilteredFixedDates).OrderBy(item => item.TrekId).ToList();




                var allFixedDates = new HashSet<string>();
                var itineraryFixedColumns = new List<Dictionary<string, string>>();
                if (itineraryType == "Customize" || itineraryType == "All" || itineraryType == null)
                {


                    foreach (var trekItinerary in combinedCustomizeDates)
                    {
                        var startDate = trekItinerary.StartDate.Value;
                        for (int i = 0; i < trekItinerary.DayCount; i++)
                        {
                            var currentDate = startDate.AddDays(i);
                            allFixedDates.Add(currentDate.ToString("yyyy-MM-dd"));
                        }
                    }
                    foreach (var trekItinerary in combinedCustomizeDates)
                    {
                        var startDate = trekItinerary.StartDate.Value;
                        var AllTrekId = trekItinerary.TrekId;
                        Dictionary<string, string> assignedColumn = null;

                        var CFixedtrekName = _context.TrekItineraries
    .Where(t => t.TrekId == trekItinerary.TrekId)
    .Select(d => new
    {
        d.TrekName,
        d.TrekShortName
    })
    .FirstOrDefault();
                        foreach (var column in itineraryFixedColumns)
                        {
                            var isBlank = true;


                            for (int i = 0; i < trekItinerary.DayCount; i++)
                            {
                                var currentDate = startDate.AddDays(i);
                                var dateKey = currentDate.ToString("yyyy-MM-dd");
                                if (column.ContainsKey(dateKey) && !string.IsNullOrEmpty(column[dateKey]))
                                {
                                    isBlank = false;
                                    break;
                                }
                            }

                            if (isBlank)
                            {
                                assignedColumn = column;
                                break;
                            }
                        }

                        // Create new column if needed
                        if (assignedColumn == null)
                        {
                            assignedColumn = new Dictionary<string, string>();
                            // Initialize all possible dates
                            foreach (var dateKey in allFixedDates)
                            {
                                assignedColumn[dateKey] = "";
                            }
                            itineraryFixedColumns.Add(assignedColumn);
                        }

                        // Add customize itinerary details
                        for (int i = 0; i < trekItinerary.DayCount; i++)
                        {
                            var currentDate = startDate.AddDays(i);
                            var dateKey = currentDate.ToString("yyyy-MM-dd");
                            var dayKey = $"Day {i + 1}";
                            var itineraryDetail = trekItinerary.ItineraryDetails
                                .FirstOrDefault(d => d.ItineraryDay == dayKey);

                            // If the itinerary detail exists, use the heading, else default to "No Heading"
                            var heading = itineraryDetail?.ItineraryHeading ?? "No Heading";

                            // Initialize a list of strings to hold the formatted headings for each dateKey
                            List<string> formattedHeadings = new List<string>();


                            if (CFixedtrekName != null)  // Ensure data is found
                            {
                                if (i == 0)
                                {
                                    // Format the heading using the retrieved values
                                    string formattedHeading = $"C - {CFixedtrekName.TrekShortName} - {CFixedtrekName.TrekName} - {heading}-{AllTrekId}";
                                    formattedHeadings.Add(formattedHeading);
                                }
                                else
                                {
                                    // For other days, just use the heading, or you can customize it further
                                    formattedHeadings.Add(heading);
                                }
                            }






                            // Join the list of formatted headings into a single string with a delimiter (e.g., new line or comma)
                            assignedColumn[dateKey] = string.Join(", ", formattedHeadings); // Use ", " or any delimiter you prefer
                        }

                    }


                }

                if (itineraryType == "Fixed" || itineraryType == "All" || itineraryType == null)
                {
                    //here we Get Custmoize Data By TrekId and FromDate and Todate

                    foreach (var fixedDate in combinedFixedDates)
                    {
                        var Startdate = fixedDate.StartDate;
                        var TrekId = fixedDate.TrekId;
                        int totalDays = fixedItineraryDetails
         .Where(i => i.TrekId == TrekId)
         .Count();
                        var allTeam = _context.AddTeamToTrek
          .Where(t => t.TrekId == fixedDate.TrekId && t.StartDate.Date == Startdate.Date && t.Trekitinerary == "Customize")
       .Select(t => new { t.TrekId, t.StartDate })
          .ToList();

                        bool hasTeamData = allTeam != null && allTeam.Count > 0;
                        //adding departureIds for view
                        foreach (var team in allTeam)
                        {
                            ListofTrekIds.Add(team.TrekId);
                            ListOfStartDate.Add(team.StartDate);

                        }


                        ViewData["CustomizeTrekId"] = ListofTrekIds;
                        ViewData["CustmoizeStartDate"] = ListOfStartDate;

                        var existingItinerary = itineraryDaysByFromDate
                            .FirstOrDefault(i => i.FromDate.Date == fixedDate.StartDate && i.TrekId == fixedDate.TrekId);
                        var daysToProcess = existingItinerary?.ItineraryDetails.Count ?? totalDays;

                        for (int i = 0; i < daysToProcess; i++)
                        {
                            var currentDate = fixedDate.StartDate.AddDays(i);
                            allFixedDates.Add(currentDate.ToString("yyyy-MM-dd"));
                        }
                    }





                    // here we Get Fixed Data By TrekId FromDate and ToDate
                    foreach (var fixedDate in combinedFixedDates)
                    {
                        var StartDate = fixedDate.StartDate;
                        var TrekId = fixedDate.TrekId;
                        var DepartureId = fixedDate.DepartureId;
                        var Batch = fixedDate.Batch;
                        var existingItinerary = itineraryDaysByFromDate
                            .FirstOrDefault(i => i.FromDate.Date == fixedDate.StartDate && i.TrekId == fixedDate.TrekId);

                        int totalDays = fixedItineraryDetails
            .Where(i => i.TrekId == TrekId)
            .Count();
                        var allTeam = _context.AddTeamToTrek
            .Where(t => t.TrekId == fixedDate.TrekId && t.DepartureId == fixedDate.DepartureId)
            .Select(t => new { t.Designation, t.TeamMemberName, t.DepartureId })
            .ToList();

                        bool hasTeamData = allTeam != null && allTeam.Count > 0;
                        //adding departureIds for view
                        foreach (var team in allTeam)
                        {
                            ListofDepartureIds.Add(team.DepartureId ?? 0);
                        }

                        ViewData["DepartureId"] = ListofDepartureIds;

                        var FtrekName = _context.TrekItineraries
    .Where(t => t.TrekId == fixedDate.TrekId)
    .Select(d => new
    {
        d.TrekName,
        d.TrekShortName
    })
    .FirstOrDefault();

                        var AllTrekers = _context.TempParticipants
            .Where(t => t.DepartureId == fixedDate.DepartureId
                && t.TrekId == TrekId
                && (t.BookingStatus == "Pending" || t.BookingStatus == "Transfer")
                && t.PaymentStatus.ToLower() == "paid")
            .Count();


                        Dictionary<string, string> assignedColumn = null;


                        int daysToProcess = existingItinerary?.ItineraryDetails.Count ?? totalDays;


                        foreach (var column in itineraryFixedColumns)
                        {
                            var isBlank = true;


                            for (int i = 0; i < daysToProcess; i++)
                            {
                                var currentDate = fixedDate.StartDate.AddDays(i);
                                var dateKey = currentDate.ToString("yyyy-MM-dd");
                                if (column.ContainsKey(dateKey) && !string.IsNullOrEmpty(column[dateKey]))
                                {
                                    isBlank = false;
                                    break;
                                }
                            }

                            if (isBlank)
                            {
                                assignedColumn = column;
                                break;
                            }
                        }

                        // Create new column if needed
                        if (assignedColumn == null)
                        {
                            assignedColumn = new Dictionary<string, string>();
                            // Initialize all possible dates
                            foreach (var dateKey in allFixedDates)
                            {
                                assignedColumn[dateKey] = "";
                            }
                            itineraryFixedColumns.Add(assignedColumn);
                        }

                        // Add fixed itinerary details
                        for (int i = 0; i < daysToProcess; i++)
                        {
                            var currentDate = fixedDate.StartDate.AddDays(i);
                            var dateKey = currentDate.ToString("yyyy-MM-dd");
                            string heading;

                            if (existingItinerary != null)
                            {
                                // Filter ItineraryDetails by TrekId
                                var filteredItineraryDetails = existingItinerary.ItineraryDetails
                                    .Where(it => it.TrekId == fixedDate.TrekId)
                                    .ToList();

                                // Use filtered itinerary details
                                var itineraryDetail = filteredItineraryDetails.ElementAtOrDefault(i);
                                heading = itineraryDetail?.ItineraryHeading ?? "No Heading";
                            }
                            else
                            {
                                // Use fixed itinerary details
                                var filteredItineraryDetails = fixedItineraryDetails
               .Where(it => it.TrekId == fixedDate.TrekId)
               .ToList();

                                // Use filtered itinerary details
                                var itineraryDetail = filteredItineraryDetails.ElementAtOrDefault(i);
                                heading = itineraryDetail?.ItineraryHeading ?? "No Heading";
                            }

                            List<string> formattedHeadings = new List<string>();

                            if (FtrekName != null)  // Ensure data is found
                            {
                                if (i == 0)
                                {

                                    string formattedHeading = $"F - ({AllTrekers})-{FtrekName.TrekShortName} - {FtrekName.TrekName} - {heading}-({Batch})-({DepartureId})";
                                    formattedHeadings.Add(formattedHeading);  // Add the formatted heading to the list

                                }
                                else
                                {
                                    // For other days, just use the heading, or you can customize it further
                                    formattedHeadings.Add(heading);
                                }
                            }

                            // Join the list of formatted headings into a single string with a delimiter (e.g., new line or comma)
                            assignedColumn[dateKey] = string.Join(", ", formattedHeadings); // Use ", " or any delimiter you prefer


                        }
                    }

                }
                // Sort dates within each column to ensure proper ordering
                foreach (var column in itineraryFixedColumns)
                {
                    var sortedEntries = allFixedDates
                        .ToDictionary(dateKey => dateKey, dateKey =>
                            column.ContainsKey(dateKey) ? column[dateKey] : "");
                    column.Clear();
                    foreach (var entry in sortedEntries.OrderBy(x => x.Key))
                    {
                        column[entry.Key] = entry.Value;
                    }
                }

                var ViewModel = itineraryFixedColumns.Select((column, index) => new OperationItineraryViewModel
                {
                    ColumnNumber = index + 1,
                    DatesWithHeadings = column
                }).ToList();

                return View(ViewModel);

            }
            var itineraryColumns = new List<Dictionary<string, string>>();




            var allDates = new HashSet<string>();

            var sevenDaysAgo = today.AddDays(-7);
            var filteredCoustomizeFromDates = customizeItineraries
               .Where(fixedDate => fixedDate.StartDate >= sevenDaysAgo && fixedDate.StartDate <= today)
               .ToList();


            var afterfilteredCustomizeFromDates = customizeItineraries
                .Where(trekItinerary => trekItinerary.StartDate > today)
                .OrderBy(trekItinerary => trekItinerary.StartDate) // Sort by date (earliest first)
                .ToList();

            // Combine both lists
            var combinedCustomizeFromDates = filteredCoustomizeFromDates.Concat(afterfilteredCustomizeFromDates).OrderBy(item => item.TrekId).ToList();





            var filteredFixedFromDates = fixedFromDates
                .Where(fixedDate => fixedDate.StartDate >= sevenDaysAgo && fixedDate.StartDate <= today)
                .ToList();



            var afterfilteredFixedFromDates = fixedFromDates
                .Where(fixedDate => fixedDate.StartDate > today)
                .OrderBy(fixedDate => fixedDate.StartDate) // Sort by date (earliest first)
                .ToList();

            // Combine both lists
            var combinedFixedFromDates = filteredFixedFromDates.Concat(afterfilteredFixedFromDates).OrderBy(item => item.TrekId).ToList();



            if (itineraryType == "Customize" || itineraryType == "All" || itineraryType == null)
            {
                //Here we get All the Customize data By TrekId only
                foreach (var trekItinerary in combinedCustomizeFromDates)
                {
                    var startDate = trekItinerary.StartDate.Value;
                    for (int i = 0; i < trekItinerary.DayCount; i++)
                    {
                        var currentDate = startDate.AddDays(i);
                        allDates.Add(currentDate.ToString("yyyy-MM-dd"));
                    }
                }


                foreach (var trekItinerary in combinedCustomizeFromDates)
                {
                    var startDate = trekItinerary.StartDate.Value;
                    var AllTrekId = trekItinerary.TrekId;
                    Dictionary<string, string> assignedColumn = null;
                    var CFixedtrekName = _context.TrekItineraries
            .Where(t => t.TrekId == trekItinerary.TrekId)
            .Select(d => new
            {
                d.TrekName,
                d.TrekShortName
            })
            .FirstOrDefault();
                    var allTeam = _context.AddTeamToTrek
          .Where(t => t.TrekId == trekItinerary.TrekId && t.StartDate.Date == trekItinerary.StartDate.Value.Date && t.Trekitinerary == "Customize")
       .Select(t => new { t.TrekId, t.StartDate })
          .ToList();

                    bool hasTeamData = allTeam != null && allTeam.Count > 0;
                    //adding departureIds for view
                    foreach (var team in allTeam)
                    {
                        ListofTrekIds.Add(team.TrekId);
                        ListOfStartDate.Add(team.StartDate);

                    }


                    ViewData["CustomizeTrekId"] = ListofTrekIds;
                    ViewData["CustmoizeStartDate"] = ListOfStartDate;


                    foreach (var column in itineraryColumns)
                    {
                        var isBlank = true;


                        for (int i = 0; i < trekItinerary.DayCount; i++)
                        {
                            var currentDate = startDate.AddDays(i);
                            var dateKey = currentDate.ToString("yyyy-MM-dd");
                            if (column.ContainsKey(dateKey) && !string.IsNullOrEmpty(column[dateKey]))
                            {
                                isBlank = false;
                                break;
                            }
                        }

                        if (isBlank)
                        {
                            assignedColumn = column;
                            break;
                        }
                    }

                    // Create new column if needed
                    if (assignedColumn == null)
                    {
                        assignedColumn = new Dictionary<string, string>();
                        // Initialize all possible dates
                        foreach (var dateKey in allDates)
                        {
                            assignedColumn[dateKey] = "";
                        }
                        itineraryColumns.Add(assignedColumn);
                    }

                    // Add customize itinerary details
                    for (int i = 0; i < trekItinerary.DayCount; i++)
                    {
                        var currentDate = startDate.AddDays(i);
                        var dateKey = currentDate.ToString("yyyy-MM-dd");
                        var dayKey = $"Day {i + 1}";
                        var itineraryDetail = trekItinerary.ItineraryDetails
                            .FirstOrDefault(d => d.ItineraryDay == dayKey);

                        // If the itinerary detail exists, use the heading, else default to "No Heading"
                        var heading = itineraryDetail?.ItineraryHeading ?? "No Heading";

                        // Initialize a list of strings to hold the formatted headings for each dateKey
                        List<string> formattedHeadings = new List<string>();

                        if (CFixedtrekName != null)  // Ensure data is found
                        {
                            if (i == 0)
                            {
                                // Format the heading using the retrieved values
                                string formattedHeading = $"C - {CFixedtrekName.TrekShortName} - {CFixedtrekName.TrekName} - {heading}-{AllTrekId}";
                                formattedHeadings.Add(formattedHeading);
                            }
                            else
                            {
                                // For other days, just use the heading, or you can customize it further
                                formattedHeadings.Add(heading);
                            }
                        }


                        // Join the list of formatted headings into a single string with a delimiter (e.g., new line or comma)
                        assignedColumn[dateKey] = string.Join(", ", formattedHeadings); // Use ", " or any delimiter you prefer
                    }


                }

            }





            if (itineraryType == "Fixed" || itineraryType == "All" || itineraryType == null)
            {

                foreach (var fixedDate in combinedFixedFromDates)
                {
                    var Startdate = fixedDate.StartDate;
                    var TrekId = fixedDate.TrekId;
                    int totalDays = fixedItineraryDetails
         .Where(i => i.TrekId == TrekId)
         .Count();



                    var existingItinerary = itineraryDaysByFromDate
                        .FirstOrDefault(i => i.FromDate.Date == fixedDate.StartDate && i.TrekId == fixedDate.TrekId);
                    var daysToProcess = existingItinerary?.ItineraryDetails.Count ?? totalDays;

                    for (int i = 0; i < daysToProcess; i++)
                    {
                        var currentDate = fixedDate.StartDate.AddDays(i);
                        allDates.Add(currentDate.ToString("yyyy-MM-dd"));
                    }
                }

                Dictionary<int, bool> departureIdStatus = new Dictionary<int, bool>();

                // here we get All the fixed data By the TrekId only
                foreach (var fixedDate in combinedFixedFromDates)
                {
                    var StartDate = fixedDate.StartDate;
                    var TrekId = fixedDate.TrekId;
                    var DepartureId = fixedDate.DepartureId;
                    var Batch = fixedDate.Batch;
                    var existingItinerary = itineraryDaysByFromDate
                        .FirstOrDefault(i => i.FromDate.Date == fixedDate.StartDate && i.TrekId == fixedDate.TrekId);

                    int totalDays = fixedItineraryDetails
         .Where(i => i.TrekId == TrekId)
         .Count();

                    var FtrekName = _context.TrekItineraries
             .Where(t => t.TrekId == fixedDate.TrekId)
             .Select(d => new
             {
                 d.TrekName,
                 d.TrekShortName,
                 d.TrekId
             })
             .FirstOrDefault();

                    var AllTrekers = _context.TempParticipants
        .Where(t => t.DepartureId == fixedDate.DepartureId
            && t.TrekId == TrekId
            && (t.BookingStatus == "Pending" || t.BookingStatus == "Transfer")
            && t.PaymentStatus.ToLower() == "paid")
        .Count();
                    var allTeam = _context.AddTeamToTrek
            .Where(t => t.TrekId == fixedDate.TrekId && t.DepartureId == fixedDate.DepartureId)
            .Select(t => new { t.Designation, t.TeamMemberName, t.DepartureId })
            .ToList();

                    bool hasTeamData = allTeam != null && allTeam.Count > 0;
                    //adding departureIds for view
                    foreach (var team in allTeam)
                    {
                        ListofDepartureIds.Add(team.DepartureId ?? 0);
                    }

                    ViewData["DepartureId"] = ListofDepartureIds;
                    //// Store the status (true if data exists, false otherwise)
                    //departureIdStatus[fixedDate.DepartureId] = hasTeamData;
                    //ViewData["DepartureIdStatus"] = departureIdStatus;

                    Dictionary<string, string> assignedColumn = null;


                    int daysToProcess = existingItinerary?.ItineraryDetails.Count ?? totalDays;


                    foreach (var column in itineraryColumns)
                    {
                        var isBlank = true;


                        for (int i = 0; i < daysToProcess; i++)
                        {
                            var currentDate = fixedDate.StartDate.AddDays(i);
                            var dateKey = currentDate.ToString("yyyy-MM-dd");
                            if (column.ContainsKey(dateKey) && !string.IsNullOrEmpty(column[dateKey]))
                            {
                                isBlank = false;
                                break;
                            }
                        }

                        if (isBlank)
                        {
                            assignedColumn = column;
                            break;
                        }
                    }

                    // Create new column if needed
                    if (assignedColumn == null)
                    {
                        assignedColumn = new Dictionary<string, string>();
                        // Initialize all possible dates
                        foreach (var dateKey in allDates)
                        {
                            assignedColumn[dateKey] = "";
                        }
                        itineraryColumns.Add(assignedColumn);
                    }

                    // Add fixed itinerary details
                    for (int i = 0; i < daysToProcess; i++)
                    {
                        var currentDate = fixedDate.StartDate.AddDays(i);
                        var dateKey = currentDate.ToString("yyyy-MM-dd");
                        string heading;






                        if (existingItinerary != null)
                        {
                            // Filter ItineraryDetails by TrekId
                            var filteredItineraryDetails = existingItinerary.ItineraryDetails
                                .Where(it => it.TrekId == fixedDate.TrekId)
                                .ToList();

                            // Use filtered itinerary details
                            var itineraryDetail = filteredItineraryDetails.ElementAtOrDefault(i);
                            heading = itineraryDetail?.ItineraryHeading ?? "No Heading";
                        }
                        else
                        {
                            // Use fixed itinerary details
                            var filteredItineraryDetails = fixedItineraryDetails
           .Where(it => it.TrekId == fixedDate.TrekId)
           .ToList();

                            // Use filtered itinerary details
                            var itineraryDetail = filteredItineraryDetails.ElementAtOrDefault(i);
                            heading = itineraryDetail?.ItineraryHeading ?? "No Heading";
                        }

                        List<string> formattedHeadings = new List<string>();
                        if (FtrekName != null)  // Ensure data is found
                        {
                            if (i == 0)
                            {

                                string formattedHeading = $"F - ({AllTrekers})-{FtrekName.TrekShortName} - {FtrekName.TrekName} - {heading}-({Batch})-({DepartureId})";
                                formattedHeadings.Add(formattedHeading);  // Add the formatted heading to the list

                            }
                            else
                            {
                                // For other days, just use the heading, or you can customize it further
                                formattedHeadings.Add(heading);
                            }
                        }


                        // Join the list of formatted headings into a single string with a delimiter (e.g., new line or comma)
                        assignedColumn[dateKey] = string.Join(", ", formattedHeadings); // Use ", " or any delimiter you prefer


                    }


                }

            }
            // Sort dates within each column to ensure proper ordering
            foreach (var column in itineraryColumns)
            {
                var sortedEntries = allDates
                    .ToDictionary(dateKey => dateKey, dateKey =>
                        column.ContainsKey(dateKey) ? column[dateKey] : "");
                column.Clear();
                foreach (var entry in sortedEntries.OrderBy(x => x.Key))
                {
                    column[entry.Key] = entry.Value;
                }
            }

            // Convert to view model
            var viewModel = itineraryColumns.Select((column, index) => new OperationItineraryViewModel
            {
                ColumnNumber = index + 1,
                DatesWithHeadings = column,
                DepartureId = ListofDepartureIds
            }).ToList();

            return View(viewModel);

        }


        //        [HttpPost]
        //        [Route("Index")]
        //        public IActionResult Index(string[] trekId, string TeamRoll, string Heading, DateTime? FromDate, DateTime? ToDate,string itineraryType)
        //        {
        //            int? NullTrekId = null; // Define NullTrekId as nullable int
        //            var ListofDepartureIds = new List<int>();

        //            int[] selectedtrekIds = trekId != null && trekId.Any()
        //                ? trekId.Where(id => !string.IsNullOrWhiteSpace(id)) // Remove empty values
        //                        .Select(id => Convert.ToInt32(id))
        //                        .ToArray()
        //                : null;






        //            DateTime today = DateTime.Today;
        //            var trekList = _context.TrekDetails.Select(t => new SelectListItem
        //            {
        //                Value = t.TrekId.ToString(),
        //                Text = t.TrekName,
        //                Selected = selectedtrekIds != null && selectedtrekIds.Contains(t.TrekId) // Check if the trekId is in the list of selected ids
        //            }).ToList();

        //            // Pass the list to the ViewData for use in the dropdown
        //            ViewData["TrekList"] = trekList;
        //            ViewData["SelectedTrekId"] = trekId;



        //            var AllDesignation = _context.Users
        //        .Where(t => t.Position != null)  // Filter out null values
        //        .Select(t => t.Position)         // Select Position values
        //        .Distinct()                      // Get distinct values
        //        .Select(p => new SelectListItem
        //        {
        //            Value = p.ToString(),
        //            Text = p
        //        })
        //        .ToList();


        //            ViewData["AllDesignation"] = AllDesignation;




        //            //here we chcek AllTrek with Dates and without Dates 
        //            if (trekId == null || trekId.All(string.IsNullOrWhiteSpace))
        //            {


        //                var TeamviewModel = new List<OperationItineraryViewModel>();

        //                if (!string.IsNullOrEmpty(TeamRoll))
        //                {
        //                    var teamMembers = _context.Users
        //   .Where(t => t.Position == TeamRoll)
        //   .Select(t => new SelectListItem
        //   {
        //       Value = t.FirstName + " " + t.LastName,
        //       Text = t.FirstName + " " + t.LastName,
        //   })
        //   .ToList();

        //                    return Json(teamMembers);

        //                }





        //                var trekDetails = _context.TrekItineraries
        //    .Include(t => t.ItineraryDetails) // Eager loading to fetch related ItineraryDetails
        //    .Select(t => new
        //    {
        //        t.TrekId,
        //        t.StartDate,
        //        t.EndDate,
        //        t.Id,
        //        t.ItineraryType,
        //        ItineraryDetails = t.ItineraryDetails // Directly use navigation property
        //            .Select(d => new
        //            {
        //                d.ItineraryDay,
        //                d.ItineraryHeading,
        //                d.TrekId
        //            })
        //            .ToList(),
        //        DaysCount = t.ItineraryDetails.Count() // Avoid extra DB call
        //    })
        //    .ToList();


        //                var customizeTrekDetails = trekDetails.Where(t => t.ItineraryType == "Customize").ToList();

        //                var fixedTrekDetails = trekDetails.Where(t => t.ItineraryType == "Fixed").ToList();


        //                var FixedTrekIdsdetails = trekDetails
        //    .Where(t => t.ItineraryType == "Fixed")
        //    .Select(t => t.TrekId)
        //    .ToList();

        //                // Step 1: Get the TrekId and Id from the TrekItinerary table where ItineraryType is "Fixed"
        //                var FixedTrekIdsdetailsId = trekDetails
        //     .Where(t => t.ItineraryType == "Fixed")
        //     .Select(t => t.Id)
        //     .ToList();


        //                // Step 2: Use the retrieved Id values to query the ItineraryDetails table
        //                var itineraryDetails = _context.ItineraryDetails
        //                    .Where(d => FixedTrekIdsdetailsId.Contains(d.TrekItineraryId))
        //                    .Select(d => new
        //                    {
        //                        d.ItineraryHeading,
        //                        d.ItineraryDay,
        //                        d.TrekId
        //                    })
        //                    .ToList();



        //                var AllTrekFixedItineraries = _context.ItineraryDetails
        //.Where(td => FixedTrekIdsdetails.Contains(td.TrekId))
        //.Select(d => new
        //{
        //    d.ItineraryDay,
        //    d.ItineraryHeading
        //})
        //.ToList();



        //                var AllTrekItineraries = _context.TrekDepartureItineraryDetail
        //                    .Where(td => FixedTrekIdsdetails.Contains(td.TrekId))
        //                    .Select(d => new
        //                    {
        //                        d.TrekId,
        //                        d.FromDate,
        //                        d.ToDate,
        //                        d.Id,
        //                        d.Batch,

        //                        ItineraryDetails = _context.TrekDepartureItinerary
        //                             .Where(td => EF.Property<int>(td, "TrekDepartureItineraryDetailId") == d.Id)
        //                            .Select(td => new
        //                            {
        //                                td.ItineraryDay,
        //                                td.ItineraryHeading,
        //                                td.TrekId

        //                            })
        //                            .ToList(),
        //                        DaysCount = _context.TrekDepartureItinerary
        //                             .Where(td => EF.Property<int>(td, "TrekDepartureItineraryDetailId") == d.Id)
        //                            .Count()
        //                    })
        //                    .ToList();










        //                var trekIds = _context.TrekItineraries
        //    .Where(td => td.ItineraryType == "Fixed") // Filter by the same TrekId

        //    .Select(td => td.TrekId)
        //    .ToList();
        //                var startDates = _context.TrekDeparture
        //  .Where(td => trekIds.Contains(td.TrekId) && (td.HideFrom == "none" || td.HideFrom == "website"))  // Filter by TrekId in trekIds list
        //  .OrderBy(d => d.StartDate)  // Sort by StartDate in ascending order
        //  .Select(d => new
        //  {
        //      d.DepartureId,
        //      d.Batch,
        //      d.StartDate,
        //      d.TrekId
        //  })
        //  .ToList();


        //                var trekItineraryDayCounts = _context.TrekItineraries
        //      .Where(ti => ti.ItineraryType == "Fixed") // Filter for "Fixed" itineraries
        //      .SelectMany(ti => _context.ItineraryDetails
        //          .Where(td => td.TrekItineraryId == ti.Id) // Match TrekItineraryId with Id from TrekItineraries
        //          .Select(td => new
        //          {
        //              td.TrekId,

        //              td.ItineraryDay // Fetch ItineraryDays for counting
        //          })
        //      )
        //      .GroupBy(td => td.TrekId) // Group by TrekId
        //      .Select(g => new
        //      {
        //          TrekId = g.Key,
        //          TotalDays = g.Count() // Count ItineraryDays for each TrekId
        //      })
        //      .ToList();




        //                var TrekName = _context.TrekDetails
        //           .Where(t => selectedtrekIds.Contains(t.TrekId))
        //           .Select(t => t.TrekName)
        //           .FirstOrDefault() ?? "Unknown Trek";

        //                var TrekItineraryColumns = new List<Dictionary<string, string>>();
        //                var AllTrekDates = new HashSet<string>();


        //                DateTime Today = DateTime.Today;
        //                var AllsevenDaysAgo = Today.AddDays(-7);


        //                DateTime AllFromDateAgo = (FromDate ?? Today).AddDays(-7);


        //                //Here we get All Trek Data by FromDate and ToDate

        //                if (FromDate.HasValue && ToDate.HasValue && (trekId == null || trekId.All(string.IsNullOrWhiteSpace)))


        //                {



        //                    // Filter the list for items where StartDate is within the last 7 days
        //                    var AllCDateBSD = customizeTrekDetails
        //                        .Where(fixedDate => fixedDate.StartDate >= AllFromDateAgo && fixedDate.StartDate < FromDate)
        //                        .ToList();


        //                    var AllCDateASD = customizeTrekDetails
        //    .Where(trekDetailsItinerary => trekDetailsItinerary.StartDate >= FromDate && trekDetailsItinerary.StartDate <= ToDate)
        //    .OrderBy(trekDetailsItinerary => trekDetailsItinerary.StartDate) // Sort by date (earliest first)
        //    .ToList();


        //                    // Combine both lists
        //                    var CombineBothCustomize = AllCDateBSD.Concat(AllCDateASD).ToList();




        //                    var AllFDateBSD = startDates
        //                   .Where(fixedDate => fixedDate.StartDate >= AllFromDateAgo && fixedDate.StartDate < FromDate)
        //                   .ToList();


        //                    var AllDDateASD = startDates
        //                        .Where(trekDetailsItinerary => trekDetailsItinerary.StartDate >= FromDate && trekDetailsItinerary.StartDate <= ToDate)
        //                        .OrderBy(trekDetailsItinerary => trekDetailsItinerary.StartDate) // Sort by date (earliest first)
        //                        .ToList();

        //                    // Combine both lists
        //                    var CombineBothFixed = AllFDateBSD.Concat(AllDDateASD).ToList();


        //                    if (itineraryType == "Customize" || itineraryType == "All" || itineraryType == null)
        //                    {
        //                        foreach (var trekDetailsItinerary in CombineBothCustomize)
        //                        {
        //                            var startDate = trekDetailsItinerary.StartDate.Value;
        //                            for (int i = 0; i < trekDetailsItinerary.DaysCount; i++)
        //                            {
        //                                var currentDate = startDate.AddDays(i);
        //                                AllTrekDates.Add(currentDate.ToString("yyyy-MM-dd"));
        //                            }
        //                        }

        //                        var CustomizeIDC = _context.TrekItineraries
        //   .Where(td => td.ItineraryType == "Customize")
        //   .Select(d => new
        //   {
        //       d.TrekName,
        //       d.TrekShortName,
        //       d.TrekId
        //   })
        //   .GroupBy(t => t.TrekId)
        //   .Select(g => g.First())
        //   .ToList();


        //                        var TNMapping = CustomizeIDC
        //                   .ToDictionary(t => t.TrekId, t => (t.TrekShortName, t.TrekName));



        //                        foreach (var trekDetailItinerary in CombineBothCustomize)
        //                        {
        //                            if (trekDetailItinerary.StartDate == null)
        //                            {
        //                                throw new InvalidOperationException("StartDate cannot be null.");
        //                            }

        //                            var startDate = trekDetailItinerary.StartDate.Value;
        //                            Dictionary<string, string> assignedColumn = null;

        //                            // Check all existing columns for blank space
        //                            foreach (var column in TrekItineraryColumns)
        //                            {
        //                                var isBlank = true;

        //                                for (int i = 0; i < trekDetailItinerary.DaysCount; i++)
        //                                {
        //                                    var currentDate = startDate.AddDays(i);
        //                                    var dateKey = currentDate.ToString("yyyy-MM-dd");
        //                                    if (column.ContainsKey(dateKey) && !string.IsNullOrEmpty(column[dateKey]))
        //                                    {
        //                                        isBlank = false;
        //                                        break;
        //                                    }
        //                                }

        //                                if (isBlank)
        //                                {
        //                                    assignedColumn = column;
        //                                    break;
        //                                }
        //                            }

        //                            // Create new column if needed
        //                            if (assignedColumn == null)
        //                            {
        //                                assignedColumn = new Dictionary<string, string>();
        //                                foreach (var dateKey in AllTrekDates)
        //                                {
        //                                    assignedColumn[dateKey] = "";
        //                                }
        //                                TrekItineraryColumns.Add(assignedColumn);
        //                            }
        //                            var AlltrekName = TNMapping.ContainsKey(trekDetailItinerary.TrekId)
        //            ? TNMapping[trekDetailItinerary.TrekId] // Extract tuple values
        //            : ("Unknown", "Unknown Trek");


        //                            // Add customize itinerary details
        //                            for (int i = 0; i < trekDetailItinerary.DaysCount; i++)
        //                            {
        //                                var currentDate = startDate.AddDays(i);
        //                                var dateKey = currentDate.ToString("yyyy-MM-dd");
        //                                var dayKey = $"Day {i + 1}";

        //                                var itineraryDetail = trekDetailItinerary.ItineraryDetails?.FirstOrDefault(d => d.ItineraryDay == dayKey);
        //                                var heading = itineraryDetail?.ItineraryHeading ?? "No Heading";

        //                                if (i == 0)
        //                                {
        //                                    heading = $"C - {AlltrekName.Item1}-{AlltrekName.Item2}-{heading}";

        //                                }

        //                                assignedColumn[dateKey] = heading;
        //                            }
        //                        }

        //                    }

        //                    if (itineraryType == "Fixed" || itineraryType == "All" || itineraryType == null)
        //                    {
        //                        foreach (var fixedDate in CombineBothFixed)
        //                        {

        //                            var existingItinerary = AllTrekItineraries
        //                                .FirstOrDefault(i => i.FromDate.Date == fixedDate.StartDate.Date);


        //                            int daysToProcess = existingItinerary?.ItineraryDetails?.Count
        //                                ?? trekItineraryDayCounts.FirstOrDefault(t => t.TrekId == fixedDate.TrekId)?.TotalDays
        //                                ?? 0;


        //                            for (int i = 0; i < daysToProcess; i++)
        //                            {
        //                                var currentDate = fixedDate.StartDate.AddDays(i);
        //                                AllTrekDates.Add(currentDate.ToString("yyyy-MM-dd"));
        //                            }
        //                        }



        //                        //This is for Customize All Trek Data By FromDate and ToDate 




        //                        var FixedIDC = _context.TrekItineraries
        //        .Where(td => td.ItineraryType == "Fixed")
        //        .Select(d => new
        //        {
        //            d.TrekName,
        //            d.TrekShortName,
        //            d.TrekId
        //        })
        //        .GroupBy(t => t.TrekId)
        //        .Select(g => g.First())
        //        .ToList();// Fetch as a list


        //                        var FixedTNMapping = FixedIDC
        //                            .ToDictionary(t => t.TrekId, t => (t.TrekShortName, t.TrekName));

        //                        var FixedDepartureId = string.Empty;


        //                        //This is for Fixed All Trek Data By FromDate and ToDate 
        //                        foreach (var startDateEntry in CombineBothFixed)
        //                        {
        //                            var startDate = startDateEntry.StartDate;
        //                            var TrekId = startDateEntry.TrekId;
        //                            var DepartureId = startDateEntry.DepartureId;
        //                            var Batch = startDateEntry.Batch;
        //                            var AllTrekers = _context.TempParticipants
        //                                .Where(t => t.TrekId == TrekId && t.DepartureId == DepartureId && t.BookingStatus == "Pending" && t.PaymentStatus == "Paid")
        //                                .Count();

        //                            var allTeam = _context.AddTeamToTrek
        //            .Where(t => t.TrekId == startDateEntry.TrekId && t.DepartureId == startDateEntry.DepartureId)
        //            .Select(t => new { t.Designation, t.TeamMemberName, t.DepartureId })
        //            .ToList();

        //                            bool hasTeamData = allTeam != null && allTeam.Count > 0;
        //                            //adding departureIds for view
        //                            foreach (var team in allTeam)
        //                            {
        //                                ListofDepartureIds.Add(team.DepartureId ?? 0);
        //                            }

        //                            ViewData["DepartureId"] = ListofDepartureIds;


        //                            var existingItinerary = AllTrekItineraries
        //                                .FirstOrDefault(i => i.FromDate.Date == startDate.Date);

        //                            Dictionary<string, string> assignedColumn = null;

        //                            int defaultDaysToProcess = trekItineraryDayCounts
        //                                .FirstOrDefault(t => t.TrekId == startDateEntry.TrekId)?.TotalDays ?? 0;

        //                            int daysToProcess = existingItinerary?.ItineraryDetails.Count ?? defaultDaysToProcess;

        //                            foreach (var column in TrekItineraryColumns)
        //                            {
        //                                bool isBlank = true;

        //                                for (int i = 0; i < daysToProcess; i++)
        //                                {
        //                                    var currentDate = startDate.AddDays(i);
        //                                    var dateKey = currentDate.ToString("yyyy-MM-dd");

        //                                    if (column.ContainsKey(dateKey) && !string.IsNullOrEmpty(column[dateKey]))
        //                                    {
        //                                        isBlank = false;
        //                                        break;
        //                                    }
        //                                }

        //                                if (isBlank)
        //                                {
        //                                    assignedColumn = column;
        //                                    break;
        //                                }
        //                            }

        //                            if (assignedColumn == null)
        //                            {
        //                                assignedColumn = AllTrekDates.ToDictionary(dateKey => dateKey, _ => "");
        //                                TrekItineraryColumns.Add(assignedColumn);
        //                            }

        //                            var AlltrekName = FixedTNMapping.ContainsKey(startDateEntry.TrekId)
        //                                ? FixedTNMapping[startDateEntry.TrekId]
        //                                : ("Unknown", "Unknown Trek");

        //                            for (int i = 0; i < daysToProcess; i++)
        //                            {
        //                                var currentDate = startDate.AddDays(i);
        //                                var dateKey = currentDate.ToString("yyyy-MM-dd");
        //                                string heading;
        //                                string departureId;

        //                                if (existingItinerary != null)
        //                                {
        //                                    var itineraryDetail = existingItinerary.ItineraryDetails.ElementAtOrDefault(i);
        //                                    heading = itineraryDetail?.ItineraryHeading ?? "No Heading";

        //                                }
        //                                else
        //                                {
        //                                    var itineraryDetail = itineraryDetails
        //                                        .Where(i => i.TrekId == startDateEntry.TrekId)
        //                                        .ElementAtOrDefault(i);
        //                                    heading = itineraryDetail?.ItineraryHeading ?? "No Heading";
        //                                }

        //                                if (i == 0)
        //                                {
        //                                    heading = $"F - ({AllTrekers})-{AlltrekName.Item1}-{AlltrekName.Item2}-{heading}-({Batch})-({DepartureId})";
        //                                    FixedDepartureId = $"({DepartureId})";
        //                                }

        //                                assignedColumn[dateKey] = heading;
        //                            }
        //                        }

        //                    }


        //                    // Method to get TrekName by TrekId


        //                    var AllViewModel = TrekItineraryColumns.Select((column, index) => new OperationItineraryViewModel
        //                    {
        //                        ColumnNumber = index + 1,
        //                        DatesWithHeadings = column,
        //                        DepartureId = ListofDepartureIds,  // Pass DepartureId (now it's in scope)
        //                                                         // You can also pass Batch and TrekName if needed, for example:
        //                                                         //Batch = Batch,                 // Pass Batch
        //                                                         //TrekName = trekName           // Pass TrekName (formatted from AlltrekName)
        //                    }).ToList();
        //                    return View(AllViewModel);



        //                }


        //                // Filter the list for items where StartDate is within the last 7 days
        //                var AllCoustomizeFromDates = customizeTrekDetails
        //                    .Where(fixedDate => fixedDate.StartDate >= AllsevenDaysAgo && fixedDate.StartDate < today)
        //                    .ToList();



        //                var afterAllCustomizeFromDates = customizeTrekDetails
        //                    .Where(trekDetailsItinerary => trekDetailsItinerary.StartDate >= today)
        //                    .OrderBy(trekDetailsItinerary => trekDetailsItinerary.StartDate) // Sort by date (earliest first)
        //                    .ToList();

        //                // Combine both lists
        //                var combinedAllCustomizeFromDates = AllCoustomizeFromDates.Concat(afterAllCustomizeFromDates).ToList();




        //                var AllFixedFromDates = startDates
        //               .Where(fixedDate => fixedDate.StartDate >= AllsevenDaysAgo && fixedDate.StartDate < today)
        //               .ToList();


        //                var afterAllFixedFromDates = startDates
        //                    .Where(trekDetailsItinerary => trekDetailsItinerary.StartDate >= today)
        //                    .OrderBy(trekDetailsItinerary => trekDetailsItinerary.StartDate) // Sort by date (earliest first)
        //                    .ToList();
        //                var AllTrekItineraryColumns = new List<Dictionary<string, string>>();
        //                // Combine both lists
        //                var combinedAllFixedFromDates = AllFixedFromDates.Concat(afterAllFixedFromDates).ToList();
        //                if (itineraryType == "Fixed" || itineraryType == "All" || itineraryType == null)
        //                {

        //                    foreach (var fixedDate in combinedAllFixedFromDates)
        //                    {

        //                        var existingItinerary = AllTrekItineraries
        //                            .FirstOrDefault(i => i.FromDate.Date == fixedDate.StartDate.Date);


        //                        int daysToProcess = existingItinerary?.ItineraryDetails?.Count
        //                            ?? trekItineraryDayCounts.FirstOrDefault(t => t.TrekId == fixedDate.TrekId)?.TotalDays
        //                            ?? 0;


        //                        for (int i = 0; i < daysToProcess; i++)
        //                        {
        //                            var currentDate = fixedDate.StartDate.AddDays(i);
        //                            AllTrekDates.Add(currentDate.ToString("yyyy-MM-dd"));
        //                        }
        //                    }











        //                    var FixedtrekItineraryDayCountsWithNames = _context.TrekItineraries
        //    .Where(td => td.ItineraryType == "Fixed")
        //    .Select(d => new
        //    {
        //        d.TrekName,
        //        d.TrekShortName,
        //        d.TrekId
        //    })
        //    .GroupBy(t => t.TrekId)
        //    .Select(g => g.First())
        //    .ToList();// Fetch as a list


        //                    var FixedtrekNameMapping = FixedtrekItineraryDayCountsWithNames
        //                        .ToDictionary(t => t.TrekId, t => (t.TrekShortName, t.TrekName));

        //                    var maindepartureId = string.Empty;



        //                    //Here we Get All The Fixed Data Without Choosing FromDate and ToDate
        //                    foreach (var startDateEntry in combinedAllFixedFromDates)
        //                    {
        //                        var startDate = startDateEntry.StartDate;
        //                        var TrekId = startDateEntry.TrekId;
        //                        var DepartureId = startDateEntry.DepartureId;
        //                        var Batch = startDateEntry.Batch;
        //                        var AllTrekers = _context.TempParticipants
        //                            .Where(t => t.TrekId == TrekId && t.DepartureId == DepartureId && t.BookingStatus == "Pending" && t.PaymentStatus == "Paid")
        //                            .Count();

        //                        var allTeam = _context.AddTeamToTrek
        //        .Where(t => t.TrekId == startDateEntry.TrekId && t.DepartureId == startDateEntry.DepartureId)
        //        .Select(t => new { t.Designation, t.TeamMemberName, t.DepartureId })
        //        .ToList();

        //                        bool hasTeamData = allTeam != null && allTeam.Count > 0;
        //                        //adding departureIds for view
        //                        foreach (var team in allTeam)
        //                        {
        //                            ListofDepartureIds.Add(team.DepartureId ?? 0);
        //                        }

        //                        ViewData["DepartureId"] = ListofDepartureIds;


        //                        var existingItinerary = AllTrekItineraries
        //                            .FirstOrDefault(i => i.FromDate.Date == startDate.Date);

        //                        Dictionary<string, string> assignedColumn = null;

        //                        int defaultDaysToProcess = trekItineraryDayCounts
        //                            .FirstOrDefault(t => t.TrekId == startDateEntry.TrekId)?.TotalDays ?? 0;

        //                        int daysToProcess = existingItinerary?.ItineraryDetails.Count ?? defaultDaysToProcess;

        //                        foreach (var column in AllTrekItineraryColumns)
        //                        {
        //                            bool isBlank = true;

        //                            for (int i = 0; i < daysToProcess; i++)
        //                            {
        //                                var currentDate = startDate.AddDays(i);
        //                                var dateKey = currentDate.ToString("yyyy-MM-dd");

        //                                if (column.ContainsKey(dateKey) && !string.IsNullOrEmpty(column[dateKey]))
        //                                {
        //                                    isBlank = false;
        //                                    break;
        //                                }
        //                            }

        //                            if (isBlank)
        //                            {
        //                                assignedColumn = column;
        //                                break;
        //                            }
        //                        }

        //                        if (assignedColumn == null)
        //                        {
        //                            assignedColumn = AllTrekDates.ToDictionary(dateKey => dateKey, _ => "");
        //                            AllTrekItineraryColumns.Add(assignedColumn);
        //                        }

        //                        var AlltrekName = FixedtrekNameMapping.ContainsKey(startDateEntry.TrekId)
        //                        ? FixedtrekNameMapping[startDateEntry.TrekId]
        //                        : ("Unknown", "Unknown Trek");

        //                        for (int i = 0; i < daysToProcess; i++)
        //                        {
        //                            var currentDate = startDate.AddDays(i);
        //                            var dateKey = currentDate.ToString("yyyy-MM-dd");
        //                            string heading;
        //                            string departureId;

        //                            if (existingItinerary != null)
        //                            {
        //                                var itineraryDetail = existingItinerary.ItineraryDetails.ElementAtOrDefault(i);
        //                                heading = itineraryDetail?.ItineraryHeading ?? "No Heading";

        //                            }
        //                            else
        //                            {
        //                                var itineraryDetail = itineraryDetails
        //                                    .Where(i => i.TrekId == startDateEntry.TrekId)
        //                                    .ElementAtOrDefault(i);
        //                                heading = itineraryDetail?.ItineraryHeading ?? "No Heading";
        //                            }

        //                            if (i == 0)
        //                            {
        //                                heading = $"F - ({AllTrekers})-{AlltrekName.Item1}-{AlltrekName.Item2}-{heading}-({Batch})-({DepartureId})";
        //                                maindepartureId = $"({DepartureId})";
        //                            }

        //                            assignedColumn[dateKey] = heading;
        //                        }
        //                    }

        //                }
        //                if (itineraryType == "Customize" || itineraryType == "All" || itineraryType == null)
        //                {

        //                    foreach (var trekDetailsItinerary in combinedAllCustomizeFromDates)
        //                    {
        //                        var startDate = trekDetailsItinerary.StartDate.Value;
        //                        for (int i = 0; i < trekDetailsItinerary.DaysCount; i++)
        //                        {
        //                            var currentDate = startDate.AddDays(i);
        //                            AllTrekDates.Add(currentDate.ToString("yyyy-MM-dd"));
        //                        }
        //                    }


        //                    var CustomizetrekItineraryDayCountsWithNames = _context.TrekItineraries
        //.Where(td => td.ItineraryType == "Customize")
        //.Select(d => new
        //{
        //    d.TrekName,
        //    d.TrekShortName,
        //    d.TrekId
        //})
        //.GroupBy(t => t.TrekId)
        //.Select(g => g.First())
        //.ToList();


        //                    var trekNameMapping = CustomizetrekItineraryDayCountsWithNames
        //               .ToDictionary(t => t.TrekId, t => (t.TrekShortName, t.TrekName));


        //                    //Here we get All the Customize data without choosing FromDate and ToDate

        //                    foreach (var trekDetailItinerary in combinedAllCustomizeFromDates)
        //                    {
        //                        if (trekDetailItinerary.StartDate == null)
        //                        {
        //                            throw new InvalidOperationException("StartDate cannot be null.");
        //                        }

        //                        var startDate = trekDetailItinerary.StartDate.Value;
        //                        Dictionary<string, string> assignedColumn = null;

        //                        // Check all existing columns for blank space
        //                        foreach (var column in TrekItineraryColumns)
        //                        {
        //                            var isBlank = true;

        //                            for (int i = 0; i < trekDetailItinerary.DaysCount; i++)
        //                            {
        //                                var currentDate = startDate.AddDays(i);
        //                                var dateKey = currentDate.ToString("yyyy-MM-dd");
        //                                if (column.ContainsKey(dateKey) && !string.IsNullOrEmpty(column[dateKey]))
        //                                {
        //                                    isBlank = false;
        //                                    break;
        //                                }
        //                            }

        //                            if (isBlank)
        //                            {
        //                                assignedColumn = column;
        //                                break;
        //                            }
        //                        }

        //                        // Create new column if needed
        //                        if (assignedColumn == null)
        //                        {
        //                            assignedColumn = new Dictionary<string, string>();
        //                            foreach (var dateKey in AllTrekDates)
        //                            {
        //                                assignedColumn[dateKey] = "";
        //                            }
        //                            TrekItineraryColumns.Add(assignedColumn);
        //                        }
        //                        var AlltrekName = trekNameMapping.ContainsKey(trekDetailItinerary.TrekId)
        //        ? trekNameMapping[trekDetailItinerary.TrekId] // Extract tuple values
        //        : ("Unknown", "Unknown Trek");


        //                        // Add customize itinerary details
        //                        for (int i = 0; i < trekDetailItinerary.DaysCount; i++)
        //                        {
        //                            var currentDate = startDate.AddDays(i);
        //                            var dateKey = currentDate.ToString("yyyy-MM-dd");
        //                            var dayKey = $"Day {i + 1}";

        //                            var itineraryDetail = trekDetailItinerary.ItineraryDetails?.FirstOrDefault(d => d.ItineraryDay == dayKey);
        //                            var heading = itineraryDetail?.ItineraryHeading ?? "No Heading";

        //                            if (i == 0)
        //                            {
        //                                heading = $"C - {AlltrekName.Item1}-{AlltrekName.Item2} - {heading}";

        //                            }

        //                            assignedColumn[dateKey] = heading;
        //                        }
        //                    }

        //                }




        //                // Method to get TrekName by TrekId


        //                var ViewModel = TrekItineraryColumns.Select((column, index) => new OperationItineraryViewModel
        //                {
        //                    ColumnNumber = index + 1,
        //                    DatesWithHeadings = column,
        //                    //DepartureId = ListofDepartureIds,
        //                }).ToList();
        //                return View(ViewModel);
        //            }




        //            var trekItineraries = _context.TrekItineraries
        //       .Where(t => selectedtrekIds.Contains(t.TrekId))
        //       .GroupBy(t => new { t.Id, t.StartDate, t.ItineraryType, t.TrekId }) // Group by ItineraryType
        //     .Select(group => new
        //     {
        //         Id = group.Key.Id,
        //         TrekId = group.Key.TrekId,
        //         StartDate = group.Key.StartDate,
        //         ItineraryType = group.Key.ItineraryType,
        //         DayCount = group.Max(g => g.ItineraryDayCount),
        //         ItineraryDetails = group.Key.ItineraryType == "Customize"
        //? group.SelectMany(g => g.ItineraryDetails.Select(i => new ItineraryDetail
        //{
        //    ItineraryDay = i.ItineraryDay,
        //    ItineraryHeading = i.ItineraryHeading
        //})).ToList()
        //: new List<ItineraryDetail>() // Empty list for Fixed
        //     })

        //       .ToList();
        //            var customizeItineraries = trekItineraries.Where(t => t.ItineraryType == "Customize").ToList();
        //            var fixedItineraries = trekItineraries.Where(t => t.ItineraryType == "Fixed").ToList();

        //            List<DateTime> fixedStartDates = new List<DateTime>();




        //            var fixedFromDates = _context.TrekDeparture
        //    .Where(td => selectedtrekIds.Contains(td.TrekId) && (td.HideFrom == "none" || td.HideFrom == "website"))
        //    .OrderBy(td => td.StartDate) // Corrected order by
        //    .Select(td => new
        //    {
        //        td.StartDate,
        //        td.TrekId,
        //        td.DepartureId,
        //        td.Batch,
        //    })
        //    .ToList();



        //            var validFixedItineraries = fixedFromDates
        //    .Where(d => d.StartDate != default(DateTime))
        //    .ToList();


        //            var allItineraries = _context.TrekDepartureItineraryDetail
        //      .Where(tdi => selectedtrekIds.Contains(tdi.TrekId))
        //      .Include(tdi => tdi.TrekDepartureItinerary)
        //      .ToList();


        //            var itineraryDaysByFromDate = allItineraries
        //                .GroupBy(tdi => new { tdi.FromDate, tdi.TrekId })
        //                .Select(group => new
        //                {
        //                    TrekId = group.Key.TrekId,
        //                    FromDate = group.Key.FromDate,

        //                    ItineraryDetails = group.SelectMany(tdi => tdi.TrekDepartureItinerary)
        //                        .Select(it => new { it.ItineraryDay, it.ItineraryHeading, it.TrekId })
        //                        .ToList()
        //                })
        //                .OrderBy(result => result.FromDate)
        //            .ToList();

        //            var fixedItineraryDetails = _context.TrekItineraries
        //                .Where(t => selectedtrekIds.Contains(t.TrekId) && t.ItineraryType == "Fixed")
        //                .SelectMany(ti => _context.ItineraryDetails
        //                    .Where(td => td.TrekItineraryId == ti.Id)
        //                    .Select(td => new
        //                    {
        //                        td.ItineraryDay,
        //                        td.ItineraryHeading,
        //                        td.TrekId
        //                    })
        //                )
        //                .ToList();



        //       //Here we Get data By TrekId and FromDate and ToDate

        //            if (FromDate.HasValue && ToDate.HasValue && trekId != null)

        //            {

        //                DateTime FromDateAgo = FromDate.Value.AddDays(-7); // Subtract 7 days from FromDate

        //                var filteredCoustomizeDates = customizeItineraries
        //                    .Where(fixedDate => fixedDate.StartDate >= FromDateAgo && fixedDate.StartDate <= FromDate.Value) // Using FromDate.Value
        //                    .ToList();


        //                var afterfilteredCustomizeDates = customizeItineraries
        //                    .Where(trekItinerary => trekItinerary.StartDate >= FromDate && trekItinerary.StartDate <= ToDate)
        //                    .OrderBy(trekItinerary => trekItinerary.StartDate) // Sort by date (earliest first)
        //                    .ToList();

        //                // Combine both lists
        //                var combinedCustomizeDates = filteredCoustomizeDates.Concat(afterfilteredCustomizeDates).ToList();



        //                var filteredFixedDates = fixedFromDates
        //               .Where(fixedDate => fixedDate.StartDate >= FromDateAgo && fixedDate.StartDate <= FromDate)
        //               .ToList();



        //                var afterfilteredFixedDates = _context.TrekDeparture
        //     .Where(td => selectedtrekIds.Contains(td.TrekId)
        //         && (td.HideFrom == "none" || td.HideFrom == "website")

        //         && td.StartDate > FromDate
        //         && td.StartDate <= ToDate)
        //     .OrderBy(td => td.StartDate) // Corrected order by
        //     .Select(td => new
        //     {
        //         td.StartDate,
        //         td.TrekId,
        //         td.DepartureId,
        //         td.Batch,
        //     })
        //     .ToList();

        //                // Combine both lists
        //                var combinedFixedDates = filteredFixedDates.Concat(afterfilteredFixedDates).ToList();




        //                var allFixedDates = new HashSet<string>();
        //                var itineraryFixedColumns = new List<Dictionary<string, string>>();
        //                if (itineraryType == "Customize" || itineraryType == "All" || itineraryType == null)
        //                {


        //                    foreach (var trekItinerary in combinedCustomizeDates)
        //                    {
        //                        var startDate = trekItinerary.StartDate.Value;
        //                        for (int i = 0; i < trekItinerary.DayCount; i++)
        //                        {
        //                            var currentDate = startDate.AddDays(i);
        //                            allFixedDates.Add(currentDate.ToString("yyyy-MM-dd"));
        //                        }
        //                    }



        //                    foreach (var fixedDate in combinedFixedDates)
        //                    {
        //                        var Startdate = fixedDate.StartDate;
        //                        var TrekId = fixedDate.TrekId;
        //                        int totalDays = fixedItineraryDetails
        //         .Where(i => i.TrekId == TrekId)
        //         .Count();

        //                        var existingItinerary = itineraryDaysByFromDate
        //                            .FirstOrDefault(i => i.FromDate.Date == fixedDate.StartDate && i.TrekId == fixedDate.TrekId);
        //                        var daysToProcess = existingItinerary?.ItineraryDetails.Count ?? totalDays;

        //                        for (int i = 0; i < daysToProcess; i++)
        //                        {
        //                            var currentDate = fixedDate.StartDate.AddDays(i);
        //                            allFixedDates.Add(currentDate.ToString("yyyy-MM-dd"));
        //                        }
        //                    }
        //                }

        //                if (itineraryType == "Fixed" || itineraryType == "All" || itineraryType == null)
        //                {
        //                    //here we Get Custmoize Data By TrekId and FromDate and Todate
        //                    foreach (var trekItinerary in combinedCustomizeDates)
        //                    {
        //                        var startDate = trekItinerary.StartDate.Value;
        //                        Dictionary<string, string> assignedColumn = null;

        //                        var CFixedtrekName = _context.TrekItineraries
        //    .Where(t => t.TrekId == trekItinerary.TrekId)
        //    .Select(d => new
        //    {
        //        d.TrekName,
        //        d.TrekShortName
        //    })
        //    .FirstOrDefault();
        //                        foreach (var column in itineraryFixedColumns)
        //                        {
        //                            var isBlank = true;


        //                            for (int i = 0; i < trekItinerary.DayCount; i++)
        //                            {
        //                                var currentDate = startDate.AddDays(i);
        //                                var dateKey = currentDate.ToString("yyyy-MM-dd");
        //                                if (column.ContainsKey(dateKey) && !string.IsNullOrEmpty(column[dateKey]))
        //                                {
        //                                    isBlank = false;
        //                                    break;
        //                                }
        //                            }

        //                            if (isBlank)
        //                            {
        //                                assignedColumn = column;
        //                                break;
        //                            }
        //                        }

        //                        // Create new column if needed
        //                        if (assignedColumn == null)
        //                        {
        //                            assignedColumn = new Dictionary<string, string>();
        //                            // Initialize all possible dates
        //                            foreach (var dateKey in allFixedDates)
        //                            {
        //                                assignedColumn[dateKey] = "";
        //                            }
        //                            itineraryFixedColumns.Add(assignedColumn);
        //                        }

        //                        // Add customize itinerary details
        //                        for (int i = 0; i < trekItinerary.DayCount; i++)
        //                        {
        //                            var currentDate = startDate.AddDays(i);
        //                            var dateKey = currentDate.ToString("yyyy-MM-dd");
        //                            var dayKey = $"Day {i + 1}";
        //                            var itineraryDetail = trekItinerary.ItineraryDetails
        //                                .FirstOrDefault(d => d.ItineraryDay == dayKey);

        //                            // If the itinerary detail exists, use the heading, else default to "No Heading"
        //                            var heading = itineraryDetail?.ItineraryHeading ?? "No Heading";

        //                            // Initialize a list of strings to hold the formatted headings for each dateKey
        //                            List<string> formattedHeadings = new List<string>();


        //                            if (CFixedtrekName != null)  // Ensure data is found
        //                            {
        //                                if (i == 0)
        //                                {
        //                                    // Format the heading using the retrieved values
        //                                    string formattedHeading = $"C - {CFixedtrekName.TrekShortName} - {CFixedtrekName.TrekName} - {heading}";
        //                                    formattedHeadings.Add(formattedHeading);
        //                                }
        //                                else
        //                                {
        //                                    // For other days, just use the heading, or you can customize it further
        //                                    formattedHeadings.Add(heading);
        //                                }
        //                            }






        //                            // Join the list of formatted headings into a single string with a delimiter (e.g., new line or comma)
        //                            assignedColumn[dateKey] = string.Join(", ", formattedHeadings); // Use ", " or any delimiter you prefer
        //                        }

        //                    }





        //                    // here we Get Fixed Data By TrekId FromDate and ToDate
        //                    foreach (var fixedDate in combinedFixedDates)
        //                    {
        //                        var StartDate = fixedDate.StartDate;
        //                        var TrekId = fixedDate.TrekId;
        //                        var DepartureId = fixedDate.DepartureId;
        //                        var Batch = fixedDate.Batch;
        //                        var existingItinerary = itineraryDaysByFromDate
        //                            .FirstOrDefault(i => i.FromDate.Date == fixedDate.StartDate && i.TrekId == fixedDate.TrekId);

        //                        int totalDays = fixedItineraryDetails
        //            .Where(i => i.TrekId == TrekId)
        //            .Count();
        //                        var allTeam = _context.AddTeamToTrek
        //            .Where(t => t.TrekId == fixedDate.TrekId && t.DepartureId == fixedDate.DepartureId)
        //            .Select(t => new { t.Designation, t.TeamMemberName, t.DepartureId })
        //            .ToList();

        //                        bool hasTeamData = allTeam != null && allTeam.Count > 0;
        //                        //adding departureIds for view
        //                        foreach (var team in allTeam)
        //                        {
        //                            ListofDepartureIds.Add(team.DepartureId ?? 0);
        //                        }

        //                        ViewData["DepartureId"] = ListofDepartureIds;

        //                        var FtrekName = _context.TrekItineraries
        //    .Where(t => t.TrekId == fixedDate.TrekId)
        //    .Select(d => new
        //    {
        //        d.TrekName,
        //        d.TrekShortName
        //    })
        //    .FirstOrDefault();

        //                        var AllTrekers = _context.TempParticipants
        //            .Where(t => t.DepartureId == fixedDate.DepartureId
        //                && t.TrekId == TrekId
        //                && (t.BookingStatus == "Pending" || t.BookingStatus == "Transfer")
        //                && t.PaymentStatus.ToLower() == "paid")
        //            .Count();


        //                        Dictionary<string, string> assignedColumn = null;


        //                        int daysToProcess = existingItinerary?.ItineraryDetails.Count ?? totalDays;


        //                        foreach (var column in itineraryFixedColumns)
        //                        {
        //                            var isBlank = true;


        //                            for (int i = 0; i < daysToProcess; i++)
        //                            {
        //                                var currentDate = fixedDate.StartDate.AddDays(i);
        //                                var dateKey = currentDate.ToString("yyyy-MM-dd");
        //                                if (column.ContainsKey(dateKey) && !string.IsNullOrEmpty(column[dateKey]))
        //                                {
        //                                    isBlank = false;
        //                                    break;
        //                                }
        //                            }

        //                            if (isBlank)
        //                            {
        //                                assignedColumn = column;
        //                                break;
        //                            }
        //                        }

        //                        // Create new column if needed
        //                        if (assignedColumn == null)
        //                        {
        //                            assignedColumn = new Dictionary<string, string>();
        //                            // Initialize all possible dates
        //                            foreach (var dateKey in allFixedDates)
        //                            {
        //                                assignedColumn[dateKey] = "";
        //                            }
        //                            itineraryFixedColumns.Add(assignedColumn);
        //                        }

        //                        // Add fixed itinerary details
        //                        for (int i = 0; i < daysToProcess; i++)
        //                        {
        //                            var currentDate = fixedDate.StartDate.AddDays(i);
        //                            var dateKey = currentDate.ToString("yyyy-MM-dd");
        //                            string heading;

        //                            if (existingItinerary != null)
        //                            {
        //                                // Filter ItineraryDetails by TrekId
        //                                var filteredItineraryDetails = existingItinerary.ItineraryDetails
        //                                    .Where(it => it.TrekId == fixedDate.TrekId)
        //                                    .ToList();

        //                                // Use filtered itinerary details
        //                                var itineraryDetail = filteredItineraryDetails.ElementAtOrDefault(i);
        //                                heading = itineraryDetail?.ItineraryHeading ?? "No Heading";
        //                            }
        //                            else
        //                            {
        //                                // Use fixed itinerary details
        //                                var filteredItineraryDetails = fixedItineraryDetails
        //               .Where(it => it.TrekId == fixedDate.TrekId)
        //               .ToList();

        //                                // Use filtered itinerary details
        //                                var itineraryDetail = filteredItineraryDetails.ElementAtOrDefault(i);
        //                                heading = itineraryDetail?.ItineraryHeading ?? "No Heading";
        //                            }

        //                            List<string> formattedHeadings = new List<string>();

        //                            if (FtrekName != null)  // Ensure data is found
        //                            {
        //                                if (i == 0)
        //                                {

        //                                    string formattedHeading = $"F - ({AllTrekers})-{FtrekName.TrekShortName} - {FtrekName.TrekName} - {heading}-({Batch})-({DepartureId})";
        //                                    formattedHeadings.Add(formattedHeading);  // Add the formatted heading to the list

        //                                }
        //                                else
        //                                {
        //                                    // For other days, just use the heading, or you can customize it further
        //                                    formattedHeadings.Add(heading);
        //                                }
        //                            }

        //                            // Join the list of formatted headings into a single string with a delimiter (e.g., new line or comma)
        //                            assignedColumn[dateKey] = string.Join(", ", formattedHeadings); // Use ", " or any delimiter you prefer


        //                        }
        //                    }

        //                }
        //                // Sort dates within each column to ensure proper ordering
        //                foreach (var column in itineraryFixedColumns)
        //                {
        //                    var sortedEntries = allFixedDates
        //                        .ToDictionary(dateKey => dateKey, dateKey =>
        //                            column.ContainsKey(dateKey) ? column[dateKey] : "");
        //                    column.Clear();
        //                    foreach (var entry in sortedEntries.OrderBy(x => x.Key))
        //                    {
        //                        column[entry.Key] = entry.Value;
        //                    }
        //                }

        //                var ViewModel = itineraryFixedColumns.Select((column, index) => new OperationItineraryViewModel
        //                {
        //                    ColumnNumber = index + 1,
        //                    DatesWithHeadings = column
        //                }).ToList();

        //                return View(ViewModel);

        //            }


        //            var itineraryColumns = new List<Dictionary<string, string>>();




        //            var allDates = new HashSet<string>();

        //            var sevenDaysAgo = today.AddDays(-7);
        //            var filteredCoustomizeFromDates = customizeItineraries
        //               .Where(fixedDate => fixedDate.StartDate >= sevenDaysAgo && fixedDate.StartDate <= today)
        //               .ToList();


        //            var afterfilteredCustomizeFromDates = customizeItineraries
        //                .Where(trekItinerary => trekItinerary.StartDate > today)
        //                .OrderBy(trekItinerary => trekItinerary.StartDate) // Sort by date (earliest first)
        //                .ToList();

        //            // Combine both lists
        //            var combinedCustomizeFromDates = filteredCoustomizeFromDates.Concat(afterfilteredCustomizeFromDates).ToList();





        //            var filteredFixedFromDates = fixedFromDates
        //                .Where(fixedDate => fixedDate.StartDate >= sevenDaysAgo && fixedDate.StartDate <= today)
        //                .ToList();



        //            var afterfilteredFixedFromDates = fixedFromDates
        //                .Where(fixedDate => fixedDate.StartDate > today)
        //                .OrderBy(fixedDate => fixedDate.StartDate) // Sort by date (earliest first)
        //                .ToList();

        //            // Combine both lists
        //            var combinedFixedFromDates = filteredFixedFromDates.Concat(afterfilteredFixedFromDates).ToList();



        //            if (itineraryType == "Customize" || itineraryType == "All" || itineraryType == null)
        //            {
        //                //Here we get All the Customize data By TrekId only
        //                foreach (var trekItinerary in combinedCustomizeFromDates)
        //                {
        //                    var startDate = trekItinerary.StartDate.Value;
        //                    for (int i = 0; i < trekItinerary.DayCount; i++)
        //                    {
        //                        var currentDate = startDate.AddDays(i);
        //                        allDates.Add(currentDate.ToString("yyyy-MM-dd"));
        //                    }
        //                }


        //                foreach (var trekItinerary in combinedCustomizeFromDates)
        //                {
        //                    var startDate = trekItinerary.StartDate.Value;
        //                    Dictionary<string, string> assignedColumn = null;
        //                    var CFixedtrekName = _context.TrekItineraries
        //            .Where(t => t.TrekId == trekItinerary.TrekId)
        //            .Select(d => new
        //            {
        //                d.TrekName,
        //                d.TrekShortName
        //            })
        //            .FirstOrDefault();

        //                    foreach (var column in itineraryColumns)
        //                    {
        //                        var isBlank = true;


        //                        for (int i = 0; i < trekItinerary.DayCount; i++)
        //                        {
        //                            var currentDate = startDate.AddDays(i);
        //                            var dateKey = currentDate.ToString("yyyy-MM-dd");
        //                            if (column.ContainsKey(dateKey) && !string.IsNullOrEmpty(column[dateKey]))
        //                            {
        //                                isBlank = false;
        //                                break;
        //                            }
        //                        }

        //                        if (isBlank)
        //                        {
        //                            assignedColumn = column;
        //                            break;
        //                        }
        //                    }

        //                    // Create new column if needed
        //                    if (assignedColumn == null)
        //                    {
        //                        assignedColumn = new Dictionary<string, string>();
        //                        // Initialize all possible dates
        //                        foreach (var dateKey in allDates)
        //                        {
        //                            assignedColumn[dateKey] = "";
        //                        }
        //                        itineraryColumns.Add(assignedColumn);
        //                    }

        //                    // Add customize itinerary details
        //                    for (int i = 0; i < trekItinerary.DayCount; i++)
        //                    {
        //                        var currentDate = startDate.AddDays(i);
        //                        var dateKey = currentDate.ToString("yyyy-MM-dd");
        //                        var dayKey = $"Day {i + 1}";
        //                        var itineraryDetail = trekItinerary.ItineraryDetails
        //                            .FirstOrDefault(d => d.ItineraryDay == dayKey);

        //                        // If the itinerary detail exists, use the heading, else default to "No Heading"
        //                        var heading = itineraryDetail?.ItineraryHeading ?? "No Heading";

        //                        // Initialize a list of strings to hold the formatted headings for each dateKey
        //                        List<string> formattedHeadings = new List<string>();

        //                        if (CFixedtrekName != null)  // Ensure data is found
        //                        {
        //                            if (i == 0)
        //                            {
        //                                // Format the heading using the retrieved values
        //                                string formattedHeading = $"C - {CFixedtrekName.TrekShortName} - {CFixedtrekName.TrekName} - {heading}";
        //                                formattedHeadings.Add(formattedHeading);
        //                            }
        //                            else
        //                            {
        //                                // For other days, just use the heading, or you can customize it further
        //                                formattedHeadings.Add(heading);
        //                            }
        //                        }


        //                        // Join the list of formatted headings into a single string with a delimiter (e.g., new line or comma)
        //                        assignedColumn[dateKey] = string.Join(", ", formattedHeadings); // Use ", " or any delimiter you prefer
        //                    }


        //                }

        //            }





        //            if (itineraryType == "Fixed" || itineraryType == "All" || itineraryType == null)
        //            {

        //                foreach (var fixedDate in combinedFixedFromDates)
        //                {
        //                    var Startdate = fixedDate.StartDate;
        //                    var TrekId = fixedDate.TrekId;
        //                    int totalDays = fixedItineraryDetails
        //         .Where(i => i.TrekId == TrekId)
        //         .Count();



        //                    var existingItinerary = itineraryDaysByFromDate
        //                        .FirstOrDefault(i => i.FromDate.Date == fixedDate.StartDate && i.TrekId == fixedDate.TrekId);
        //                    var daysToProcess = existingItinerary?.ItineraryDetails.Count ?? totalDays;

        //                    for (int i = 0; i < daysToProcess; i++)
        //                    {
        //                        var currentDate = fixedDate.StartDate.AddDays(i);
        //                        allDates.Add(currentDate.ToString("yyyy-MM-dd"));
        //                    }
        //                }

        //                Dictionary<int, bool> departureIdStatus = new Dictionary<int, bool>();

        //                // here we get All the fixed data By the TrekId only
        //                foreach (var fixedDate in combinedFixedFromDates)
        //                {
        //                    var StartDate = fixedDate.StartDate;
        //                    var TrekId = fixedDate.TrekId;
        //                    var DepartureId = fixedDate.DepartureId;
        //                    var Batch = fixedDate.Batch;
        //                    var existingItinerary = itineraryDaysByFromDate
        //                        .FirstOrDefault(i => i.FromDate.Date == fixedDate.StartDate && i.TrekId == fixedDate.TrekId);

        //                    int totalDays = fixedItineraryDetails
        //         .Where(i => i.TrekId == TrekId)
        //         .Count();

        //                    var FtrekName = _context.TrekItineraries
        //             .Where(t => t.TrekId == fixedDate.TrekId)
        //             .Select(d => new
        //             {
        //                 d.TrekName,
        //                 d.TrekShortName,
        //                 d.TrekId
        //             })
        //             .FirstOrDefault();

        //                    var AllTrekers = _context.TempParticipants
        //        .Where(t => t.DepartureId == fixedDate.DepartureId
        //            && t.TrekId == TrekId
        //            && (t.BookingStatus == "Pending" || t.BookingStatus == "Transfer")
        //            && t.PaymentStatus.ToLower() == "paid")
        //        .Count();
        //                    var allTeam = _context.AddTeamToTrek
        //            .Where(t => t.TrekId == fixedDate.TrekId && t.DepartureId == fixedDate.DepartureId)
        //            .Select(t => new { t.Designation, t.TeamMemberName, t.DepartureId })
        //            .ToList();

        //                    bool hasTeamData = allTeam != null && allTeam.Count > 0;
        //                    //adding departureIds for view
        //                    foreach (var team in allTeam)
        //                    {
        //                        ListofDepartureIds.Add(team.DepartureId ?? 0);
        //                    }

        //                    ViewData["DepartureId"] = ListofDepartureIds;
        //                    //// Store the status (true if data exists, false otherwise)
        //                    //departureIdStatus[fixedDate.DepartureId] = hasTeamData;
        //                    //ViewData["DepartureIdStatus"] = departureIdStatus;

        //                    Dictionary<string, string> assignedColumn = null;


        //                    int daysToProcess = existingItinerary?.ItineraryDetails.Count ?? totalDays;


        //                    foreach (var column in itineraryColumns)
        //                    {
        //                        var isBlank = true;


        //                        for (int i = 0; i < daysToProcess; i++)
        //                        {
        //                            var currentDate = fixedDate.StartDate.AddDays(i);
        //                            var dateKey = currentDate.ToString("yyyy-MM-dd");
        //                            if (column.ContainsKey(dateKey) && !string.IsNullOrEmpty(column[dateKey]))
        //                            {
        //                                isBlank = false;
        //                                break;
        //                            }
        //                        }

        //                        if (isBlank)
        //                        {
        //                            assignedColumn = column;
        //                            break;
        //                        }
        //                    }

        //                    // Create new column if needed
        //                    if (assignedColumn == null)
        //                    {
        //                        assignedColumn = new Dictionary<string, string>();
        //                        // Initialize all possible dates
        //                        foreach (var dateKey in allDates)
        //                        {
        //                            assignedColumn[dateKey] = "";
        //                        }
        //                        itineraryColumns.Add(assignedColumn);
        //                    }

        //                    // Add fixed itinerary details
        //                    for (int i = 0; i < daysToProcess; i++)
        //                    {
        //                        var currentDate = fixedDate.StartDate.AddDays(i);
        //                        var dateKey = currentDate.ToString("yyyy-MM-dd");
        //                        string heading;






        //                        if (existingItinerary != null)
        //                        {
        //                            // Filter ItineraryDetails by TrekId
        //                            var filteredItineraryDetails = existingItinerary.ItineraryDetails
        //                                .Where(it => it.TrekId == fixedDate.TrekId)
        //                                .ToList();

        //                            // Use filtered itinerary details
        //                            var itineraryDetail = filteredItineraryDetails.ElementAtOrDefault(i);
        //                            heading = itineraryDetail?.ItineraryHeading ?? "No Heading";
        //                        }
        //                        else
        //                        {
        //                            // Use fixed itinerary details
        //                            var filteredItineraryDetails = fixedItineraryDetails
        //           .Where(it => it.TrekId == fixedDate.TrekId)
        //           .ToList();

        //                            // Use filtered itinerary details
        //                            var itineraryDetail = filteredItineraryDetails.ElementAtOrDefault(i);
        //                            heading = itineraryDetail?.ItineraryHeading ?? "No Heading";
        //                        }

        //                        List<string> formattedHeadings = new List<string>();
        //                        if (FtrekName != null)  // Ensure data is found
        //                        {
        //                            if (i == 0)
        //                            {

        //                                string formattedHeading = $"F - ({AllTrekers})-{FtrekName.TrekShortName} - {FtrekName.TrekName} - {heading}-({Batch})-({DepartureId})";
        //                                formattedHeadings.Add(formattedHeading);  // Add the formatted heading to the list

        //                            }
        //                            else
        //                            {
        //                                // For other days, just use the heading, or you can customize it further
        //                                formattedHeadings.Add(heading);
        //                            }
        //                        }


        //                        // Join the list of formatted headings into a single string with a delimiter (e.g., new line or comma)
        //                        assignedColumn[dateKey] = string.Join(", ", formattedHeadings); // Use ", " or any delimiter you prefer


        //                    }


        //                }

        //            }
        //            // Sort dates within each column to ensure proper ordering
        //            foreach (var column in itineraryColumns)
        //            {
        //                var sortedEntries = allDates
        //                    .ToDictionary(dateKey => dateKey, dateKey =>
        //                        column.ContainsKey(dateKey) ? column[dateKey] : "");
        //                column.Clear();
        //                foreach (var entry in sortedEntries.OrderBy(x => x.Key))
        //                {
        //                    column[entry.Key] = entry.Value;
        //                }
        //            }

        //            // Convert to view model
        //            var viewModel = itineraryColumns.Select((column, index) => new OperationItineraryViewModel
        //            {
        //                ColumnNumber = index + 1,
        //                DatesWithHeadings = column,
        //                DepartureId=ListofDepartureIds
        //            }).ToList();

        //            return View(viewModel);

        //        }


        [HttpGet]
        [Route("GetTeamMembers")]
        public IActionResult GetTeamMembers(string position)
        {
            var today = DateTime.Today;



            var assignedMembersDetails = _context.AddTeamToTrek
                .Where(att => att.Designation == position && att.ToDate >= today)
                .Select(att => new
                {
                    TeamMemberName = att.TeamMemberName,
                    TrekName = att.TrekName,
                    FromDate = att.FromDate,
                    ToDate = att.ToDate
                })
                .ToList();
            var latestAssignmentsByMember = assignedMembersDetails
       .Where(x => x.FromDate > today)
       .GroupBy(x => x.TeamMemberName)
       .Select(g => g.OrderBy(x => x.FromDate).First())
       .OrderBy(x => x.FromDate)
       .ToList();

            dynamic teamMembers = null;

            if (position == "TL" || position == "TL-A" || position == "Guide" || position == "BCM")
            {
                var tlPositions = new[] { "TL", "TL-A", "Guide", "BCM" };

                teamMembers = _context.Users
                    .Where(t => tlPositions.Contains(t.Position) && t.EmployeeStatus !="Resign")
                    .AsEnumerable()
                    .Select(t => new
                    {
                        position = t.Position,
                        id = t.Id,
                        employeeId = t.EmployeeId,
                        FullName = t.FirstName + " " + t.LastName,
                        Text = t.FirstName + " " + t.LastName + " " + t.EmployeeId +
                               GetAssignmentDetailsString(
                                   latestAssignmentsByMember.Where(am => am.TeamMemberName == t.FirstName + " " + t.LastName)
                               ),
                        IsAssigned = latestAssignmentsByMember.Any(am => am.TeamMemberName == t.FirstName + " " + t.LastName)
                    })
                    .OrderBy(t => t.FullName)
                    .Select(t => new
                    {
                        Value = t.id,
                        t.employeeId,
                        t.FullName,
                        t.Text,
                        t.IsAssigned,
                        t.position
                    })
                    .ToList();
            }

            else if (position == "Chef" || position == "Chef-A" || position == "Helper")
            {
                var chefPositions = new[] { "Chef", "Chef-A", "Helper" };

                teamMembers = _context.Users
                    .Where(t => chefPositions.Contains(t.Position) && t.EmployeeStatus != "Resign")
                    .AsEnumerable()
                    .Select(t => new
                    {
                        position = t.Position,
                        id = t.Id,
                        employeeId = t.EmployeeId,
                        FullName = t.FirstName + " " + t.LastName,
                        Text = t.FirstName + " " + t.LastName + " " + t.EmployeeId +
                               GetAssignmentDetailsString(
                                   latestAssignmentsByMember.Where(am => am.TeamMemberName == t.FirstName + " " + t.LastName)
                               ),
                        IsAssigned = latestAssignmentsByMember.Any(am => am.TeamMemberName == t.FirstName + " " + t.LastName)
                    })
                    .OrderBy(t => t.FullName)
                    .Select(t => new
                    {
                        Value = t.id,
                        t.employeeId,
                        t.FullName,
                        t.Text,
                        t.IsAssigned,
                        t.position
                    })
                 .ToList();
            }

            return Json(teamMembers);
        }

        private string GetAssignmentDetailsString(IEnumerable<object> assignments)
        {
            if (!assignments.Any()) return "";

            var details = assignments.Select(a =>
            {
                var assignment = (dynamic)a;
                return $" ({assignment.TrekName}: {assignment.FromDate:dd/MM} - {assignment.ToDate:dd/MM})";
            });

            return string.Join(", ", details);
        }

        [HttpPost]

        [Route("GetTeamMembers")]
        public IActionResult GetTeamMembers(string Heading, DateTime Date, int departureId, string selectedTeamMemberName, string selectedRole, string FreelancingDetails)
        {
            // If Freelancing is selected, use the input field value and append "(Freelancing)"
            string freelancingField = null;
            string TeamMemberName = null;
            if (!string.IsNullOrEmpty(FreelancingDetails))
            {
                freelancingField = "Freelancing"; // Store the details separately
                TeamMemberName = $"{FreelancingDetails} (Freelancing)";
            }
            string trekType = Heading.StartsWith("F") ? "Fixed" :
                  Heading.StartsWith("C") ? "Customize" : "Unknown";
            // Process the submitted data
            if (string.IsNullOrEmpty(selectedTeamMemberName))
            {
                ModelState.AddModelError("TeamMember", "Please select a team member.");
                return View(); // Return to the same view with an error message
            }

            string firstName = null, lastName = null, selectedUser = null;
            string teamFullName = null; string employeeId = null;
            string[] nameParts = selectedTeamMemberName.Split(' ');
            if (selectedTeamMemberName == "Fixed")
            {
                teamFullName = selectedTeamMemberName;


            }
            else if (selectedTeamMemberName == "Freelancing")
            {
                teamFullName = TeamMemberName;
            }
            else
            {
                selectedUser = _context.Users
        .Where(t => t.Id == selectedTeamMemberName)
        .Select(t => t.Email)
        .FirstOrDefault();

                firstName = _context.Users
     .Where(t => t.Id == selectedTeamMemberName)
     .Select(t => t.FirstName)
     .FirstOrDefault();

                lastName = _context.Users
     .Where(t => t.Id == selectedTeamMemberName)
     .Select(t => t.LastName)
     .FirstOrDefault();

                employeeId = _context.Users
     .Where(t => t.Id == selectedTeamMemberName)
     .Select(t => t.Id)
     .FirstOrDefault();
                teamFullName = firstName + " " + lastName;


            }



            string trekName = "Unknown";
            if (!string.IsNullOrEmpty(Heading))
            {
                string[] parts = Heading.Split('-');

                if (parts.Length > 2) // Ensure at least 4 parts exist
                {
                    trekName = parts[2]; // Get the part after the 3rd hyphen
                }
            }

            string DepartureId = "Unknown";
            int departureIdNumber = 0;

            if (!string.IsNullOrEmpty(Heading))
            {
                string[] parts = Heading.Split('-');
                if (parts.Length > 7)
                {
                    string numberPart = parts[7].Trim('(', ')'); // Remove parentheses
                    if (int.TryParse(numberPart, out departureIdNumber))
                    {
                        // Successfully converted to integer
                        // departureIdNumber now contains the numeric value
                    }
                    else
                    {
                        // Conversion failed - keep default value of 0
                    }
                }
            }
            var batch = _context.TrekDeparture
    .Where(td => td.DepartureId == departureIdNumber)
    .Select(td => td.Batch)
    .FirstOrDefault();




            string FulltrekName = "Unknown";

            if (!string.IsNullOrEmpty(Heading))
            {
                string[] parts = Heading.Split('-');

                if (parts.Length > 3) // Ensure at least 4 parts exist
                {
                    FulltrekName = parts[3]; // Get the part after the 3rd hyphen
                }
            }
            FulltrekName = FulltrekName.Trim();
            var TrekId = 0;
            if (Heading.Contains("C -"))
            {
                TrekId = _context.TrekDetails
               .Where(td => td.TrekName == trekName)
               .Select(td => td.TrekId)
               .FirstOrDefault();
            }
            else
            {


                TrekId = _context.TrekDetails
                   .Where(td => td.TrekName == FulltrekName)
                   .Select(td => td.TrekId)
                   .FirstOrDefault();
            }

            List<dynamic> days;
            if (trekType == "Customize")
            {
                days = _context.TrekItineraries
                    .Where(td => td.StartDate == Date && td.TrekId == TrekId && td.ItineraryType == "Customize")
                    .Select(td => new { td.FromDate, td.ToDate })
                       .AsEnumerable()
    .Cast<dynamic>()
                    .ToList();
            }
            else
            {
                days = _context.TrekItineraries
                    .Where(td => td.TrekId == TrekId && td.ItineraryType == "Fixed")
                    .Select(td => new { td.FromDate, td.ToDate })
                       .AsEnumerable()
    .Cast<dynamic>()
                    .ToList();
            }

            if (days.Any())
            {
                var firstDay = days.First();

                int fromDay = int.Parse(Regex.Replace(firstDay.FromDate, @"\b(Day|day|dy)\s*", ""));
                int toDay = int.Parse(Regex.Replace(firstDay.ToDate, @"\b(Day|day|dy)\s*", ""));

                DateTime calculatedFromDate = Date.AddDays(fromDay - 1);
                DateTime calculatedToDate = Date.AddDays(toDay - 1);

                // Save the data including the Freelancing field
                var newTeamMember = new AddedTeamMember
                {
                    TrekId = TrekId,
                    TrekName = trekName,
                    DepartureId = departureIdNumber,
                    Trekitinerary = trekType,
                    Batch = batch,
                    StartDate = Date,
                    FromDate = calculatedFromDate,
                    ToDate = calculatedToDate,
                    TeamMemberName = teamFullName, // Includes "(Freelancing)" if applicable
                    Designation = selectedRole,
                    FreelancingField = freelancingField, // Store the actual Freelancing details separately
                    FormSubmission = "Pending",
                    Approval = "Pending",
                    Email = selectedUser,
                    UserId = employeeId
                };

                _context.AddTeamToTrek.Add(newTeamMember);
                _context.SaveChanges();





            }


            return Json(new { success = true, message = "Team member added successfully." });
        }

        [HttpPost]
        [Route("GetViewTeam")]
        public JsonResult GetViewTeam([FromBody] ViewTeamRequest request)
        {
            string trekName = "Unknown";
            if (!string.IsNullOrEmpty(request.Heading))
            {
                string[] parts = request.Heading.Split('-');

                if (parts.Length > 2) // Ensure at least 4 parts exist
                {
                    trekName = parts[2]; // Get the part after the 3rd hyphen
                }
            }

            string DepartureId = "Unknown";
            int departureIdNumber = 0;
            var allTeam = new List<object>();
            if (request.Heading.Contains("F -"))
            {



                if (!string.IsNullOrEmpty(request.Heading))
                {
                    string[] parts = request.Heading.Split('-');
                    if (parts.Length > 7)
                    {
                        string numberPart = parts[7].Trim('(', ')'); // Remove parentheses
                        if (int.TryParse(numberPart, out departureIdNumber))
                        {
                            // Successfully converted to integer
                            // departureIdNumber now contains the numeric value
                        }
                        else
                        {
                            // Conversion failed - keep default value of 0
                        }
                    }
                }

                allTeam = _context.AddTeamToTrek
      .Where(t => t.TrekName == trekName && t.DepartureId == departureIdNumber)
      .Select(t => new
      {
          t.Designation,
          t.TeamMemberName
      })
      .ToList<object>();
            }


            else
            {

                allTeam = _context.AddTeamToTrek
      .Where(t => t.TrekName == trekName && t.StartDate == request.Date)
      .Select(t => new
      {
          t.Designation,
          t.TeamMemberName
      })
      .ToList<object>();
            }
            return Json(new
            {
                success = true,
                message = "Data received successfully",
                trekName = trekName,
                receivedDate = request.Date,
                teamMembers = allTeam // Returning the filtered data
            });
        }



        [HttpPost]
        [Route("DeleteMember")]
        public IActionResult DeleteMember([FromBody] DeleteMemberRequest model)
        {
            if (model == null)
            {
                return Json(new { success = false, message = "Invalid request." });
            }

            string trekName = "Unknown";
            if (!string.IsNullOrEmpty(model.Heading))
            {
                string[] parts = model.Heading.Split('-');

                if (parts.Length > 2) // Ensure at least 4 parts exist
                {
                    trekName = parts[2]; // Get the part after the 3rd hyphen
                }
            }


            var member = _context.AddTeamToTrek // Ensure this is the correct DbSet
                .FirstOrDefault(m => m.TrekName == trekName
                    && m.StartDate == model.Date
                    && m.Designation == model.Designation
                    && m.TeamMemberName == model.TeamMemberName);

            if (member != null)
            {
                _context.AddTeamToTrek.Remove(member); // Corrected DbSet for removal
                _context.SaveChanges();

                return Json(new { success = true, message = "Member deleted successfully." });
            }

            return Json(new { success = false, message = "Member not found." });
        }


        [HttpGet]
        [Route("FeedbackEntryPoint")]
        public IActionResult FeedbackEntryPoint()
        {
            if (User.Identity.IsAuthenticated)
            {
                var emailClaim = User.Claims.FirstOrDefault(c =>
                    c.Type == ClaimTypes.Name || c.Type == "Email" || c.Type == "EmailAddress" || c.Type == ClaimTypes.Email);

                string userEmail = emailClaim?.Value;
                Console.WriteLine($"User Email: {userEmail}");

                if (!string.IsNullOrEmpty(userEmail))
                {
                    var user = _context.Users
                        .Where(u => u.Email == userEmail)
                        .Select(u => new
                        {
                            u.Email,
                            u.FirstName,
                            u.LastName
                        })
                        .FirstOrDefault();

                    if (user != null)
                    {
                        ViewBag.UserEmail = user.Email;
                        ViewBag.FirstName = user.FirstName;
                        ViewBag.LastName = user.LastName;

                        string fullName = user.FirstName + " " + user.LastName;

                        // Check if TeamMemberName matches the concatenated FirstName and LastName
                        var trekDetails = _context.AddTeamToTrek
                            .Where(a => a.TeamMemberName == fullName && a.FormSubmission == "pending")

                            .Select(a => new
                            {
                                a.TrekName,
                                a.StartDate,
                                a.ToDate,
                                a.TrekId,
                                a.DepartureId,
                                a.Designation,
                                a.Trekitinerary
                            })
                            .ToList();





                        return View(trekDetails);
                    }
                }
            }
            return View();
        }

        [HttpGet]
        [Route("TeamFeedbackForm")]
        public IActionResult TeamFeedbackForm()
        {
            if (User.Identity.IsAuthenticated)
            {
                var emailClaim = User.Claims.FirstOrDefault(c =>
                    c.Type == ClaimTypes.Name || c.Type == "Email" || c.Type == "EmailAddress" || c.Type == ClaimTypes.Email);

                string userEmail = emailClaim?.Value;
                Console.WriteLine($"User Email: {userEmail}");

                if (!string.IsNullOrEmpty(userEmail))
                {
                    var user = _context.Users
    .Where(u => u.Email == userEmail)
    .Select(u => new
    {
        u.Email,
        u.FirstName,
        u.LastName,
        u.Id
    })
    .FirstOrDefault();

                    if (user != null)
                    {
                        ViewBag.UserEmail = user.Email;
                        ViewBag.FirstName = user.FirstName;
                        ViewBag.LastName = user.LastName;
                        string userId = user.Id;
                        string fullName = user.FirstName + " " + user.LastName;

                        // Check if TeamMemberName matches the concatenated FirstName and LastName
                        var trekDetails = _context.AddTeamToTrek
                         .Where(a => a.UserId == userId && a.FormSubmission == "Pending")

                                                .Select(a => new
                            {
                                a.TrekName,
                                a.StartDate,
                                a.ToDate,
                                a.TrekId,
                                a.DepartureId,
                                a.Designation,
                                a.Trekitinerary
                            })
                            .ToList();




                        var departureIds = trekDetails.Select(a => a.DepartureId).Distinct().ToList();
                        var trekIds = trekDetails.Select(a => a.TrekId).Distinct().ToList();
                        var designation = trekDetails.Select(a => a.Designation).Distinct().ToList();
                        var StartDates = trekDetails.Select(a => a.StartDate).Distinct().ToList();
                        var ToDates = trekDetails.Select(a => a.ToDate).Distinct().ToList();
                        var Trekitinerary = trekDetails.Select(a => a.Trekitinerary).Distinct().ToList();


                        var ChefPendingForm = _context.AddTeamToTrek
      .Where(a => StartDates.Contains(a.StartDate)
                  && ToDates.Contains(a.ToDate)
                  && a.Designation == "Chef"
                  && a.FormSubmission == "pending"
                 )
      .Select(a => new
      {
          a.StartDate,
          a.ToDate,
          a.DepartureId,
          a.TrekId,
          a.Trekitinerary,
          a.TrekName
      })
      .ToList();


                        var ChefForm = _context.AddTeamToTrek
      .Where(a => StartDates.Contains(a.StartDate)
                  && ToDates.Contains(a.ToDate)
                  && a.Designation == "Chef"
                  && a.FormSubmission == "Submitted")
      .Select(a => new
      {
          a.StartDate,
          a.ToDate,
          a.DepartureId,
          a.TrekId,
          a.Trekitinerary,
          a.TrekName
      })
      .ToList();

                        // Store as List of Tuples for easy checking in the view  var chefFormDatesList = ChefForm.Select(a => Tuple.Create(a.StartDate, a.ToDate)).ToList();

                        if (ChefForm != null)
                        {

                            ViewBag.ChefTrekId = ChefForm.Select(t => t.TrekId).ToList();
                            ViewBag.ChefAllDepartureIds = ChefForm.Select(t => t.DepartureId ?? 0).ToList();

                            ViewBag.ChefStartDate = ChefForm.Select(t => t.StartDate).ToList();
                            ViewBag.ChefToDate = ChefForm.Select(t => t.ToDate).ToList();
                            ViewBag.ChefTrekitinerary = ChefForm.Select(t => t.Trekitinerary).ToList();
                            ViewBag.ChefTrekName = ChefForm.Select(t => t.TrekName).ToList();
                        }

                        if (ChefPendingForm != null)
                        {
                            ViewBag.PendingChefStartDate = ChefPendingForm.Select(t => t.StartDate).ToList();
                            ViewBag.PendingToDate = ChefPendingForm.Select(t => t.ToDate).ToList();
                            ViewBag.PendingChefTrekId = ChefPendingForm.Select(t => t.TrekId).ToList();
                            ViewBag.PendingDepartureIds = ChefPendingForm.Select(t => t.DepartureId ?? 0).ToList();
                            ViewBag.PendingChefTrekitinerary = ChefPendingForm.Select(t => t.Trekitinerary).ToList();
                            ViewBag.PendingChefTrekName = ChefPendingForm.Select(t => t.TrekName).ToList();
                        }
                        if (trekDetails != null && trekDetails.Count > 0)
                        {

                            ViewBag.TrekNames = trekDetails.Select(t => t.TrekName).ToList(); // Storing TrekNames separately
                            ViewBag.TrekId = trekDetails.Select(t => t.TrekId).ToList();
                            ViewBag.AllDepartureIds = trekDetails.Select(t => t.DepartureId ?? 0).ToList();

                            ViewBag.StartDate = trekDetails.Select(t => t.StartDate).ToList();
                            ViewBag.ToDate = trekDetails.Select(t => t.ToDate).ToList();
                            ViewBag.Designation = trekDetails.Select(t => t.Designation).ToList();
                            ViewBag.Trekitinerary = trekDetails.Select(t => t.Trekitinerary).ToList();
                        }



                    }
                }
            }

            return View();
        }







        [HttpGet]
        [Route("BaseCampManagerForm")]
        public IActionResult BaseCampManagerForm(int departureId, int trekId, string trekName, DateTime startDate, DateTime toDate, string firstName, string lastName, string email, string designation)
        {
            var viewModel = new BaseCampManagerViewModel();


            if (User.Identity.IsAuthenticated)
            {
                var emailClaim = User.Claims.FirstOrDefault(c =>
                    c.Type == ClaimTypes.Name || c.Type == "Email" || c.Type == "EmailAddress" || c.Type == ClaimTypes.Email);

                string userEmail = emailClaim?.Value;
                Console.WriteLine($"User Email: {userEmail}");

                if (!string.IsNullOrEmpty(userEmail))
                {
                    var user = _context.Users
                        .Where(u => u.Email == userEmail)
                        .Select(u => new
                        {
                            u.Email,
                            u.FirstName,
                            u.LastName,
                            u.Id
                        })
                        .FirstOrDefault();

                    if (user != null)
                    {

                        ViewBag.userId = user.Id;

                    }
                }
            }



            ViewBag.DepartureId = departureId;
            ViewBag.TrekId = trekId;

            ViewBag.StartDate = startDate;
            ViewBag.ToDate = toDate;
            ViewBag.FirstName = firstName;
            ViewBag.LastName = lastName;
            ViewBag.Email = email;
            ViewBag.Designation = designation;

            if (trekName != null)
            {
                ViewBag.TrekName = trekName;
            }

            if (departureId != 0 && trekId != 0)
            {


                var batch = _context.TrekDeparture.FirstOrDefault(t => t.TrekId == trekId && t.DepartureId == departureId);
                if (batch != null)
                {
                    ViewBag.Batch = batch.Batch;
                }
            }



            if (startDate <= toDate)
            {
                DateTime StartDate = startDate.AddDays(-5);
                DateTime ToDate = toDate.AddDays(5);

                // Generating a list of DateTime objects
                viewModel.AvailableDates = Enumerable.Range(0, (toDate - startDate).Days + 1)
                    .Select(offset => startDate.AddDays(offset)) // Ensure this returns DateTime
                    .ToList();
            }



            return View(viewModel);
        }



        [HttpPost]
        public IActionResult SaveBaseCampManagerForm(BaseCampManagerViewModel model)
        {
            var TeamMemberName = model.FirstName + " " + model.LastName;

            var addedTeamMemberId = 0;
            if (model.DepartureId != 0 && model.TrekId != 0)
            {
                addedTeamMemberId = _context.AddTeamToTrek
               .Where(a => a.TrekId == model.TrekId && a.DepartureId == model.DepartureId && a.UserId==model.id && a.Designation == "BCM")
               .Select(a => a.Id)
               .FirstOrDefault();
            }
            else
            {
                addedTeamMemberId = _context.AddTeamToTrek
               .Where(a => a.TrekName == model.TrekName && a.StartDate == model.StartDate && a.ToDate == model.EndDate && a.UserId==model.id && a.Designation == "BCM")
               .Select(a => a.Id)
               .FirstOrDefault();
            }





            var trekBaseCampManager = new TrekBaseCampManager
            {
                TrekName = model.TrekName,
                FromDate = model.StartDate,
                ToDate = model.EndDate,
                DepartureId = model.DepartureId,
                Batch = model.Batch,
                MaleTrekker = model.MaleTrekker,
                FemaleTrekker = model.FemaleTrekker,
                OnlineOffloadMode = model.OnlineOffloadMode,
                OflineOffloadMode = model.OflineOffloadMode,
                TeaxtArea = model.TeaxtArea,
                Email = model.Email,
                TrekId = model.TrekId,
                FirstName = model.FirstName,
                LastName = model.LastName,
                TeamId = addedTeamMemberId

            };

            // Save BaseCampManager first to generate Id
            _context.TrekBaseCampManager.Add(trekBaseCampManager);
            _context.SaveChanges();

            // Assign the generated Id to related entities
            trekBaseCampManager.HelperFreelancers = model.HelperFreelancers?.Select(h => new HelperFreelancer
            {
                Name = h.Name,
                Vendor = h.Vendor,
                Days = h.Days,
                BaseCampManagerId = trekBaseCampManager.Id  // Assigning Foreign Key
            }).ToList() ?? new List<HelperFreelancer>();

            trekBaseCampManager.GuideFreelancers = model.GuideFreelancer?.Select(g => new GuideFreelancer
            {
                Number = g.Number,
                Vendor = g.Vendor,
                Days = g.Days,
                BaseCampManagerId = trekBaseCampManager.Id  // Assigning Foreign Key
            }).ToList() ?? new List<GuideFreelancer>();

            trekBaseCampManager.Mules = model.Mules?.Select(g => new Mule
            {
                Number = g.Number,
                Days = g.Days,
                BaseCampManagerId = trekBaseCampManager.Id  // Assigning Foreign Key
            }).ToList() ?? new List<Mule>();

            trekBaseCampManager.Porters = model.Porters?.Select(g => new Porter
            {
                Number = g.Number,
                Vendor = g.Vendor,
                Days = g.Days,
                BaseCampManagerId = trekBaseCampManager.Id  // Assigning Foreign Key
            }).ToList() ?? new List<Porter>();


            trekBaseCampManager.MainTransportArrival = model.MainTransportArrivals?.Select(g => new MainTransportArrival
            {
                Number = g.Number,
                Vendor = g.Vendor,
                BaseCampManagerId = trekBaseCampManager.Id  // Assigning Foreign Key
            }).ToList() ?? new List<MainTransportArrival>();


            trekBaseCampManager.MainTransportArrivalDeparture = model.MainTransportDepartures?.Select(g => new MainTransportArrivalDeparture
            {
                Number = g.Number,
                Vendor = g.Vendor,
                BaseCampManagerId = trekBaseCampManager.Id  // Assigning Foreign Key
            }).ToList() ?? new List<MainTransportArrivalDeparture>();

            trekBaseCampManager.LocalTransportArrival = model.LocalTransportArrivals?.Select(g => new LocalTransportArrival
            {
                Number = g.Number,
                Vendor = g.Vendor,
                BaseCampManagerId = trekBaseCampManager.Id  // Assigning Foreign Key
            }).ToList() ?? new List<LocalTransportArrival>();


            trekBaseCampManager.LocalTransportArrivalDeparture = model.LocalTransportDepartures?.Select(g => new LocalTransportArrivalDeparture
            {
                Number = g.Number,
                Vendor = g.Vendor,
                BaseCampManagerId = trekBaseCampManager.Id  // Assigning Foreign Key
            }).ToList() ?? new List<LocalTransportArrivalDeparture>();

            trekBaseCampManager.RoomArrivals = model.RoomArrivals?.Select(g => new RoomArrival
            {
                Date = g.Date,
                Number = g.Number,
                Vendor = g.Vendor,
                BaseCampManagerId = trekBaseCampManager.Id  // Assigning Foreign Key
            }).ToList() ?? new List<RoomArrival>();

            trekBaseCampManager.RoomArrivalsDepartures = model.RoomArrivalsDepartures?.Select(g => new RoomArrivalsDeparture
            {
                Date = g.Date,
                Number = g.Number,
                Vendor = g.Vendor,
                BaseCampManagerId = trekBaseCampManager.Id  // Assigning Foreign Key
            }).ToList() ?? new List<RoomArrivalsDeparture>();


            _context.SaveChanges();



            var teamMemberName = model.FirstName + " " + model.LastName;

            var addTeamToTrek = _context.AddTeamToTrek
                .FirstOrDefault(t => t.Id == addedTeamMemberId &&
                                     
                                         t.Designation == "BCM" &&
                                     t.FormSubmission == "Pending"
                                     );

            if (addTeamToTrek != null)
            {
                addTeamToTrek.FormSubmission = "Submitted";
                addTeamToTrek.FormSubmissionDate = DateTime.Now;
                _context.SaveChanges();
            }

            return RedirectToAction("TeamFeedbackForm");



        }

        [HttpGet]
        [Route("TLForm")]
        public IActionResult TLForm(int departureId, int trekId, string trekName, DateTime startDate, DateTime toDate, string firstName, string lastName, string email, string designation)
        {
            var viewModel = new TLFormViewModel
            {

                Participants = new List<ParticipantNamesModel>()
            };



            if (User.Identity.IsAuthenticated)
            {
                var emailClaim = User.Claims.FirstOrDefault(c =>
                    c.Type == ClaimTypes.Name || c.Type == "Email" || c.Type == "EmailAddress" || c.Type == ClaimTypes.Email);

                string userEmail = emailClaim?.Value;
                Console.WriteLine($"User Email: {userEmail}");

                if (!string.IsNullOrEmpty(userEmail))
                {
                    var user = _context.Users
                        .Where(u => u.Email == userEmail)
                        .Select(u => new
                        {
                            u.Email,
                            u.FirstName,
                            u.LastName,
                            u.Id
                        })
                        .FirstOrDefault();

                    if (user != null)
                    {

                        ViewBag.userId = user.Id;

                    }
                }
            }


            ViewBag.DepartureId = departureId;
            ViewBag.TrekId = trekId;

            ViewBag.StartDate = startDate;
            ViewBag.ToDate = toDate;
            ViewBag.FirstName = firstName;
            ViewBag.LastName = lastName;
            ViewBag.Email = email;
            ViewBag.Designation = designation;



            if (trekName != null)
            {
                ViewBag.TrekName = trekName;
            }

            if (departureId != 0 && trekId != 0)
            {
                viewModel.Participants = _context.TempParticipants
            .Where(p => p.DepartureId == departureId
                && p.TrekId == trekId

                && (p.BookingStatus == "Pending" || p.BookingStatus == "Transfer")
                && p.PaymentStatus == "Paid")
            .Select(p => new ParticipantNamesModel
            {
                FirstName = p.FirstName,
                LastName = p.LastName,
                ParticipantId = p.TempParticipantId

            })
            .ToList();
            }
            else
            {
                viewModel.Participants = _context.TempParticipants
              .Where(p => p.StartDate == startDate && p.EndDate == toDate
                  && p.TrekName == trekName
                  && (p.BookingStatus == "Pending" || p.BookingStatus == "Transfer")
                  && p.PaymentStatus == "Paid")
              .Select(p => new ParticipantNamesModel
              {
                  FirstName = p.FirstName,
                  LastName = p.LastName,
                  ParticipantId = p.TempParticipantId

              })
              .ToList();
            }


            return View(viewModel);



        }

        [HttpPost]
        [Route("SaveTLForm")]
        public IActionResult SaveTLForm(TLFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Re-populate Participants list so the form renders correctly on return
                if (model.DepartureId != 0 && model.TrekId != 0)
                {
                    model.Participants = _context.TempParticipants
                        .Where(p => p.DepartureId == model.DepartureId
                            && p.TrekId == model.TrekId
                            && (p.BookingStatus == "Pending" || p.BookingStatus == "Transfer")
                            && p.PaymentStatus == "Paid")
                        .Select(p => new ParticipantNamesModel
                        {
                            FirstName = p.FirstName,
                            LastName = p.LastName,
                            ParticipantId = p.TempParticipantId
                        })
                        .ToList();
                }
                else
                {
                    model.Participants = _context.TempParticipants
                        .Where(p => p.StartDate == model.StartDate && p.EndDate == model.EndDate
                            && p.TrekName == model.TrekName
                            && (p.BookingStatus == "Pending" || p.BookingStatus == "Transfer")
                            && p.PaymentStatus == "Paid")
                        .Select(p => new ParticipantNamesModel
                        {
                            FirstName = p.FirstName,
                            LastName = p.LastName,
                            ParticipantId = p.TempParticipantId
                        })
                        .ToList();
                }

                ViewBag.TrekName  = model.TrekName;
                ViewBag.TrekId    = model.TrekId;
                ViewBag.DepartureId = model.DepartureId;
                ViewBag.StartDate = model.StartDate;
                ViewBag.ToDate    = model.EndDate;
                ViewBag.FirstName = model.FirstName;
                ViewBag.LastName  = model.LastName;
                ViewBag.Email     = model.Email;
                ViewBag.Designation = model.Designation;
                ViewBag.Batch     = model.TrekName;
                ViewBag.userId    = model.id;

                return View("TLForm", model);
            }

            // Manual validation: all TrekkersStatus rows must have a status selected
            if (model.TrekkersStatus != null && model.TrekkersStatus.Any(t => string.IsNullOrWhiteSpace(t.TrekkerStatus)))
            {
                ModelState.AddModelError("TrekkersStatus", "Please select status for all trekkers.");

                if (model.DepartureId != 0 && model.TrekId != 0)
                {
                    model.Participants = _context.TempParticipants
                        .Where(p => p.DepartureId == model.DepartureId && p.TrekId == model.TrekId
                            && (p.BookingStatus == "Pending" || p.BookingStatus == "Transfer")
                            && p.PaymentStatus == "Paid")
                        .Select(p => new ParticipantNamesModel { FirstName = p.FirstName, LastName = p.LastName, ParticipantId = p.TempParticipantId })
                        .ToList();
                }
                else
                {
                    model.Participants = _context.TempParticipants
                        .Where(p => p.StartDate == model.StartDate && p.EndDate == model.EndDate
                            && p.TrekName == model.TrekName
                            && (p.BookingStatus == "Pending" || p.BookingStatus == "Transfer")
                            && p.PaymentStatus == "Paid")
                        .Select(p => new ParticipantNamesModel { FirstName = p.FirstName, LastName = p.LastName, ParticipantId = p.TempParticipantId })
                        .ToList();
                }

                ViewBag.TrekName    = model.TrekName;
                ViewBag.TrekId      = model.TrekId;
                ViewBag.DepartureId = model.DepartureId;
                ViewBag.StartDate   = model.StartDate;
                ViewBag.ToDate      = model.EndDate;
                ViewBag.FirstName   = model.FirstName;
                ViewBag.LastName    = model.LastName;
                ViewBag.Email       = model.Email;
                ViewBag.Designation = model.Designation;
                ViewBag.Batch       = model.TrekName;
                ViewBag.userId      = model.id;

                return View("TLForm", model);
            }

            var TeamMemberName = model.FirstName + " " + model.LastName;


            var addedTeamMemberId = 0;
            if (model.DepartureId != 0 && model.TrekId != 0)
            {
                addedTeamMemberId = _context.AddTeamToTrek
               .Where(a => a.TrekId == model.TrekId && a.DepartureId == model.DepartureId && a.UserId==model.id && a.Designation == "TL")
               .Select(a => a.Id)
               .FirstOrDefault();
            }
            else
            {
                addedTeamMemberId = _context.AddTeamToTrek
               .Where(a => a.TrekName == model.TrekName && a.StartDate == model.StartDate && a.ToDate == model.EndDate && a.UserId==model.id && a.Designation == "TL")
               .Select(a => a.Id)
               .FirstOrDefault();
            }

            // Save main form details
            var tlForm = new TLForm
            {
                TrekName = model.TrekName,
                TextArea = model.TextArea,
                TrekId = model.TrekId,
                DepartureId = model.DepartureId,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                FirstName = model.FirstName,
                LastName = model.LastName,
                TLBriefing = model.TLBriefing,
                AmsCPRLecture = model.AmsCPRLecture,
                FloraFaunaLecture = model.FloraFaunaLecture,
                SOSSignalLecture = model.SOSSignalLecture,
                TerminologyLecture = model.TerminologyLecture,
                RandomTopicLecture = model.RandomTopicLecture,
                BPCheck = model.BPCheck,
                OxygenCheck = model.OxygenCheck,
                HealthFeedbackMorning = model.HealthFeedbackMorning,
                HealthFeedbackAfterDinner = model.HealthFeedbackAfterDinner,
                Debriefing = model.Debriefing,
                OxygenCylinderAndRopeOnsummit = model.OxygenCylinderAndRopeOnsummit,
                MealsAndEveningTea = model.MealsAndEveningTea,
                CampCleanDrive = model.CampCleanDrive,
                HotLunchServed = model.HotLunchServed,
                HotTeaServed = model.HotTeaServed,
                GReviewCount = model.GReviewCount,

                HealthCheckupVandP = model.HealthCheckupVandP,
                ExercisePandV = model.ExercisePandV,
                LecturePandV = model.LecturePandV,
                WhileTrekkingPandV = model.WhileTrekkingPandV,
                SceneryPandV = model.SceneryPandV,
                FoodPandV = model.FoodPandV,
                SubmitPandV = model.SubmitPandV,
                CampSitePandV = model.CampSitePandV,
                CampCleaningPandV = model.CampCleaningPandV,
                TrekkerActivityPandV = model.TrekkerActivityPandV,
                InterviewVideo = model.InterviewVideo,
                Email = model.Email,
                TeamId = addedTeamMemberId


            };

            _context.TLForm.Add(tlForm);
            _context.SaveChanges();
            tlForm.TrekkersStatus = model.TrekkersStatus?.Select(g => new TrekkersStatus
            {
                FirstName = g.FirstName,
                LastName = g.LastName,
                TrekkerStatus = g.TrekkerStatus,
                ParticipantId = g.ParticipantId,

                TLFormId = tlForm.Id // Assigning Foreign Key
            }).ToList() ?? new List<TrekkersStatus>();
            // Save renting details if available
            if (model.CampManagmentRatingTL != null && model.CampManagmentRatingTL.Any())
            {
                foreach (var rentingDetail in model.CampManagmentRatingTL)



                {
                    var renting = new CampManagmentRatingTL
                    {
                        TLFormId = tlForm.Id, // Link it to TLForm
                        toiletRating = rentingDetail.toiletRating,
                        DiningTentRating = rentingDetail.DiningTentRating,
                        KitchenTentRating = rentingDetail.KitchenTentRating,
                        hygieneRating = rentingDetail.hygieneRating,
                        TableServingRating = rentingDetail.TableServingRating
                    };

                    _context.CampManagmentRatingTL.Add(renting);
                }
            }



            _context.SaveChanges();

            var teamMemberName = model.FirstName + " " + model.LastName;

            var addTeamToTrek = _context.AddTeamToTrek
                .FirstOrDefault(t => t.Id == addedTeamMemberId &&
                                    
                                     t.Designation == "TL" &&
                                     t.FormSubmission == "Pending");

            if (addTeamToTrek != null)
            {
                addTeamToTrek.FormSubmission = "Submitted";
                addTeamToTrek.FormSubmissionDate = DateTime.Now;
                _context.SaveChanges();
            }



            return RedirectToAction("TeamFeedbackForm");

        }
        [HttpGet]
        [Route("ChefForm")]
        public IActionResult ChefForm(int departureId, int trekId, string trekName, DateTime startDate, DateTime toDate, string firstName, string lastName, string email, string designation)

        {


            if (User.Identity.IsAuthenticated)
            {
                var emailClaim = User.Claims.FirstOrDefault(c =>
                    c.Type == ClaimTypes.Name || c.Type == "Email" || c.Type == "EmailAddress" || c.Type == ClaimTypes.Email);

                string userEmail = emailClaim?.Value;
                Console.WriteLine($"User Email: {userEmail}");

                if (!string.IsNullOrEmpty(userEmail))
                {
                    var user = _context.Users
                        .Where(u => u.Email == userEmail)
                        .Select(u => new
                        {
                            u.Email,
                            u.FirstName,
                            u.LastName,
                            u.Id
                        })
                        .FirstOrDefault();

                    if (user != null)
                    {

                        ViewBag.userId = user.Id;

                    }
                }
            }


            ViewBag.DepartureId = departureId;
            ViewBag.TrekId = trekId;

            ViewBag.StartDate = startDate;
            ViewBag.ToDate = toDate;







            if (departureId == 0 && trekId == 0)
            {
                var CustomizeTLDetails = _context.AddTeamToTrek
                .Where(a => startDate == a.StartDate && toDate == a.ToDate && (a.Designation == "Chef") && a.TrekName == trekName && a.FormSubmission == "Pending")
                .Select(a => new
                {
                    a.TeamMemberName,
                    a.Designation

                })
                .ToList();
                if (CustomizeTLDetails != null && CustomizeTLDetails.Count > 0)
                {
                    if (CustomizeTLDetails.Any(a => a.TeamMemberName == "Fixed"))
                    {
                        ViewBag.Designation = CustomizeTLDetails.Select(t => t.Designation);
                    }
                    else
                    {
                        ViewBag.Designation = CustomizeTLDetails.Select(t => t.Designation);
                        ViewBag.TeamMemberName = CustomizeTLDetails.Select(t => t.TeamMemberName);

                        // Get the first team member (or default to null)
                        var firstTeamMember = CustomizeTLDetails.FirstOrDefault();

                        if (firstTeamMember != null)
                        {
                            string[] nameParts = firstTeamMember.TeamMemberName?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? new string[0];

                            ViewBag.FirstName = nameParts.Length > 2
                                ? string.Join(" ", nameParts.Take(nameParts.Length - 1))
                                : (nameParts.Length > 0 ? nameParts[0] : "");

                            ViewBag.LastName = nameParts.Length > 1 ? nameParts[^1] : "";
                        }
                    }

                }
            }
            else
            {
                var TLDetails = _context.AddTeamToTrek
                .Where(a => departureId == a.DepartureId && trekId == a.TrekId && (a.Designation == "Chef"))
                .Select(a => new
                {
                    a.TeamMemberName,

                    a.Designation

                })
                .ToList();


                if (TLDetails != null && TLDetails.Count > 0)
                {
                    if (TLDetails.Any(a => a.TeamMemberName == "Fixed"))
                    {
                        ViewBag.Designation = TLDetails.Select(t => t.Designation);
                    }
                    else
                    {
                        ViewBag.Designation = TLDetails.Select(t => t.Designation);
                        ViewBag.TeamMemberName = TLDetails.Select(t => t.TeamMemberName);

                        // Get the first team member (or default to null)
                        var firstTeamMember = TLDetails.FirstOrDefault();

                        if (firstTeamMember != null)
                        {
                            string[] nameParts = firstTeamMember.TeamMemberName?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? new string[0];

                            ViewBag.FirstName = nameParts.Length > 2
                                ? string.Join(" ", nameParts.Take(nameParts.Length - 1))
                                : (nameParts.Length > 0 ? nameParts[0] : "");

                            ViewBag.LastName = nameParts.Length > 1 ? nameParts[^1] : "";
                        }
                    }

                }
            }







            int itineraryDaysCount = 0;
            var trek = _context.TrekDetails.FirstOrDefault(t => t.TrekId == trekId);
            if (trek != null)
            {
                ViewBag.TrekName = trek.TrekName;
            }
            else
            {
                ViewBag.TrekName = trekName;
            }

            // Calculate days count
            if (startDate != null && toDate != null)
            {
                itineraryDaysCount = (toDate - startDate).Days + 1;
            }




            ViewBag.ItineraryDaysCount = itineraryDaysCount; // Pass count using ViewBag

            return View();
        }


        [HttpPost]
        [Route("SaveRatings")]
        public IActionResult SaveRatings(IFormCollection form)
        {
            try
            {
                // Basic validation for required trek information
                if (!form.ContainsKey("TrekId") || !form.ContainsKey("DepartureId"))
                {
                    return BadRequest("Missing trek information.");
                }

                // Safely parse trek details with error checking
                if (!int.TryParse(form["TrekId"].ToString(), out int trekId) ||
                    !int.TryParse(form["DepartureId"].ToString(), out int departureId))
                {
                    return BadRequest("Invalid trek ID or departure ID.");
                }

                var trekName = form["TrekName"].ToString();

                // Parse dates with validation
                if (!DateTime.TryParse(form["StartDate"].ToString(), out DateTime startDate) ||
                    !DateTime.TryParse(form["EndDate"].ToString(), out DateTime endDate))
                {
                    return BadRequest("Invalid date format.");
                }
                if (!int.TryParse(form["ItineraryDaysCount"].ToString(), out int itineraryDaysCount)) { }
                var TrekName = form.ContainsKey("TrekName") ? form["TrekName"].ToString() : string.Empty;

                var firstName = form.ContainsKey("FirstName") ? form["FirstName"].ToString() : string.Empty;

                var lastName = form.ContainsKey("LastName") ? form["LastName"].ToString() : string.Empty;
                var email = form.ContainsKey("Email") ? form["Email"].ToString() : string.Empty;
                var id = form.ContainsKey("id") ? form["id"].ToString() : string.Empty;

                var TeamMemberName = firstName + " " + lastName;


                var addedTeamMemberId = 0;
                if (departureId != 0 && trekId != 0)
                {
                    addedTeamMemberId = _context.AddTeamToTrek
                   .Where(a => a.TrekId == trekId && a.DepartureId == departureId && a.Designation == "Chef")
                   .Select(a => a.Id)
                   .FirstOrDefault();
                }
                else
                {
                    addedTeamMemberId = _context.AddTeamToTrek
                   .Where(a => a.TrekName == TrekName && a.StartDate.Date == startDate.Date && a.ToDate.Date == endDate.Date && a.Designation == "Chef")
                   .Select(a => a.Id)
                   .FirstOrDefault();
                }









                var ratings = new List<ChefFormRatings>();

                int timingRating = 0, hygieneRating = 0, tasteRating = 0,
                    asPerMenuRating = 0, involvementRating = 0;
                // Process each day's ratings
                for (int i = 1; i <= itineraryDaysCount; i++)
                {

                    var chefNameKey = $"ChefName[{i}].Name";
                    var timingRatingKey = $"Timing-Rating[{i}]";
                    var hygineRatingKey = $"Hygiene-Rating[{i}]";
                    var tasteRatingKey = $"Taste-Rating[{i}]";
                    var asperMenuRatingKey = $"AsPerMenu-Rating[{i}]";
                    var involvementRatingKey = $"Involvement-Rating[{i}]";

                    if (!form.ContainsKey(chefNameKey)) continue;

                    var chefName = form[chefNameKey].ToString();
                    if (form.ContainsKey(timingRatingKey) && int.TryParse(form[timingRatingKey], out int tempTimingRating))
                    {
                        timingRating = tempTimingRating;
                    }


                    if (form.ContainsKey(hygineRatingKey) && int.TryParse(form[hygineRatingKey], out int tempHygineRating))
                    {
                        hygieneRating = tempHygineRating;
                    }

                    if (form.ContainsKey(tasteRatingKey) && int.TryParse(form[tasteRatingKey], out int tempTasteRating))
                    {
                        tasteRating = tempTasteRating;
                    }

                    if (form.ContainsKey(asperMenuRatingKey) && int.TryParse(form[asperMenuRatingKey], out int tempasperMenuRating))
                    {
                        asPerMenuRating = tempasperMenuRating;
                    }

                    if (form.ContainsKey(involvementRatingKey) && int.TryParse(form[involvementRatingKey], out int tempinvolvementRating))
                    {
                        involvementRating = tempinvolvementRating;
                    }
                    if (string.IsNullOrEmpty(chefName)) continue;

                    // Initialize rating values


                    // Safely parse each rating with validation
                    //var isValid = true;

                    //if (!TryGetRating(form, $"TimingRating[{i}]", out timingRating)) isValid = false;
                    //if (!TryGetRating(form, $"hygieneRating[{i}]", out hygieneRating)) isValid = false;
                    //if (!TryGetRating(form, $"TasteRating[{i}]", out tasteRating)) isValid = false;
                    //if (!TryGetRating(form, $"AsPerMenuRating[{i}]", out asPerMenuRating)) isValid = false;
                    //if (!TryGetRating(form, $"InvolvementRating[{i}]", out involvementRating)) isValid = false;

                    /* if (!isValid) continue;*/ // Skip this day if any rating is invalid

                    // Create new rating entry
                    var rating = new ChefFormRatings
                    {
                        TrekId = trekId,
                        FirstName = firstName,
                        LastName = lastName,
                        Email = email,

                        DepartureId = departureId,
                        TrekName = trekName,
                        FromDate = startDate,
                        ToDate = endDate,
                        ChefName = chefName,
                        CampDays = i.ToString(),
                        TimingRating = timingRating,
                        HygieneRating = hygieneRating,
                        TasteRating = tasteRating,
                        AsPerMenuRating = asPerMenuRating,
                        InvolvementRating = involvementRating,
                        TeamId = addedTeamMemberId

                    };

                    ratings.Add(rating);
                }

                //if (!ratings.Any())
                //{
                //    return BadRequest("No valid ratings to save.");
                //}
                _context.ChefFormRatings.AddRange(ratings);
                _context.SaveChanges();

                var teamMemberName = firstName + " " + lastName;

                var addTeamToTrek = _context.AddTeamToTrek
                    .FirstOrDefault(t =>
                                         t.TrekId == trekId &&

                                         t.Id == addedTeamMemberId &&
                                         t.Designation == "Chef" &&
                                         t.FormSubmission == "Pending");

                if (addTeamToTrek != null)
                {
                    addTeamToTrek.FormSubmission = "Submitted";
                    addTeamToTrek.FormSubmissionDate = DateTime.Now;
                    _context.SaveChanges();
                }
                TempData["Success"] = "Ratings saved successfully!";
                return RedirectToAction("TeamFeedbackForm");
            }
            catch (Exception ex)
            {
                // Log the exception
                return BadRequest($"Error saving ratings: {ex.Message}");
            }
        }

        // Helper method to safely parse ratings
        private bool TryGetRating(IFormCollection form, string key, out int rating)
        {
            rating = 0;
            if (!form.ContainsKey(key)) return false;

            var value = form[key].ToString();
            if (string.IsNullOrEmpty(value)) return false;

            return int.TryParse(value, out rating) && rating >= 1 && rating <= 5;
        }







        [HttpGet]
        [Route("TeamApprovalForm")]
        public IActionResult TeamApprovalForm()
        {
            var pendingTrekDetails = _context.AddTeamToTrek
                .Where(a => a.FormSubmission == "Pending" && (a.Designation == "TL" || a.Designation == "Chef" || a.Designation == "BCM"))
                .Select(a => new TeamPendingApprovalsViewModel
                {
                    TrekName = a.TrekName,
                    StartDate = a.StartDate,
                    ToDate = a.ToDate,
                    TrekId = a.TrekId,
                    DepartureId = a.DepartureId,
                    Designation = a.Designation,
                    TeamMemberName = a.TeamMemberName,
                    Id = a.Id
                })
                .ToList();
            ViewBag.PendingTrekCount = pendingTrekDetails.Count;

            var submittedTrekDetails = _context.AddTeamToTrek
                .Where(a => a.FormSubmission == "submitted" && a.Approval == "Pending")
                .Select(a => new TeamSubmittedApprovalsViewModel
                {
                    TrekName = a.TrekName,
                    StartDate = a.StartDate,
                    ToDate = a.ToDate,
                    TrekId = a.TrekId,
                    DepartureId = a.DepartureId,
                    Designation = a.Designation,
                    TeamMemberName = a.TeamMemberName,
                    Id = a.Id

                })
                .ToList();



            //var BaseApprovedDetails = (from trek in _context.AddTeamToTrek
            //                           join BCMForm in _context.TrekBaseCampManager
            //                           on new { trek.TrekId, trek.DepartureId } equals new { BCMForm.TrekId, BCMForm.DepartureId }
            //                           where trek.FormSubmission == "submitted"
            //                                 && (trek.Designation == "BCM")
            //                                 && BCMForm.Approval == "Approve"
            //                           select new
            //                           {
            //                               trek.TrekId,
            //                               trek.DepartureId,
            //                               trek.Designation,
            //                               trek.TeamMemberName
            //                           })
            //                  .GroupBy(t => new { t.TrekId, t.DepartureId, t.TeamMemberName })
            //                  .Select(g => g.First())  // Pick only one unique record per group
            //                  .ToList();

            //var TLApprovedDetails = (from trek in _context.AddTeamToTrek
            //                           join TLForm in _context.TrekBaseCampManager
            //                           on new { trek.TrekId, trek.DepartureId } equals new { TLForm.TrekId, TLForm.DepartureId }
            //                           where trek.FormSubmission == "submitted"
            //                                 && (trek.Designation == "TL" || trek.Designation == "ATL")
            //                                 && TLForm.Approval == "Approve"
            //                           select new
            //                           {
            //                               trek.TrekId,
            //                               trek.DepartureId,
            //                               trek.Designation,
            //                               trek.TeamMemberName
            //                           })
            //                  .GroupBy(t => new { t.TrekId, t.DepartureId, t.TeamMemberName })
            //                  .Select(g => g.First())  // Pick only one unique record per group
            //                  .ToList();

            //var chefApprovedDetails = (from trek in _context.AddTeamToTrek
            //                           join chefRating in _context.ChefFormRatings
            //                           on new { trek.TrekId, trek.DepartureId } equals new { chefRating.TrekId, chefRating.DepartureId }
            //                           where trek.FormSubmission == "submitted"
            //                                 && (trek.Designation == "Chef" || trek.Designation == "Chef-A")
            //                                 && chefRating.Approval == "Approve"
            //                           select new
            //                           {
            //                               trek.TrekId,
            //                               trek.DepartureId,
            //                               trek.Designation,
            //                               trek.TeamMemberName
            //                           })
            //             .GroupBy(t => new { t.TrekId, t.DepartureId, t.TeamMemberName })
            //             .Select(g => g.First())  // Pick only one unique record per group
            //             .ToList();

            ViewBag.submittedTrekDetails = submittedTrekDetails.Count;
            var viewModel = new TeamFeedbackApprovalViewModel
            {
                PendingApprovals = pendingTrekDetails,
                SubmittedApprovals = submittedTrekDetails
            };

            return View(viewModel);
        }


        [HttpGet]
        [Route("TeamApprovalIndex")]
        public IActionResult TeamApprovalIndex()
        {
            var pendingTrekDetails = _context.AddTeamToTrek
                .Where(a => a.FormSubmission == "Pending" && (a.Designation == "TL" || a.Designation == "BCM") && a.ToDate.Date <= DateTime.Now.Date)
                .Select(a => new TeamPendingApprovalsViewModel
                {
                    TrekName = a.TrekName,
                    StartDate = a.StartDate,
                    ToDate = a.ToDate,
                    TrekId = a.TrekId,
                    DepartureId = a.DepartureId,
                    Designation = a.Designation,
                    TeamMemberName = a.TeamMemberName,
                    Id = a.Id,
                    Batch=a.Batch
                })
                .ToList();

            ViewBag.PendingTrekCount = pendingTrekDetails.Count;

            var submittedTrekDetails = _context.AddTeamToTrek
                .Where(a => a.FormSubmission == "Submitted" && a.Approval == "Pending" && (a.Designation == "TL" || a.Designation == "BCM"))
                .Select(a => new TeamSubmittedApprovalsViewModel
                {
                    TrekName = a.TrekName.Trim(),
                    StartDate = a.StartDate,
                    ToDate = a.ToDate,
                    TrekId = a.TrekId,
                    DepartureId = a.DepartureId,
                    Designation = a.Designation,
                    TeamMemberName = a.TeamMemberName,
                    Id = a.Id,
                    FormSubmissionDate = a.FormSubmissionDate,

                    Batch=a.Batch
                    

                })
                .ToList();



            //var BaseApprovedDetails = (from trek in _context.AddTeamToTrek
            //                           join BCMForm in _context.TrekBaseCampManager
            //                           on new { trek.TrekId, trek.DepartureId } equals new { BCMForm.TrekId, BCMForm.DepartureId }
            //                           where trek.FormSubmission == "submitted"
            //                                 && (trek.Designation == "BCM")
            //                                 && BCMForm.Approval == "Approve"
            //                           select new
            //                           {
            //                               trek.TrekId,
            //                               trek.DepartureId,
            //                               trek.Designation,
            //                               trek.TeamMemberName
            //                           })
            //                  .GroupBy(t => new { t.TrekId, t.DepartureId, t.TeamMemberName })
            //                  .Select(g => g.First())  // Pick only one unique record per group
            //                  .ToList();

            //var TLApprovedDetails = (from trek in _context.AddTeamToTrek
            //                           join TLForm in _context.TrekBaseCampManager
            //                           on new { trek.TrekId, trek.DepartureId } equals new { TLForm.TrekId, TLForm.DepartureId }
            //                           where trek.FormSubmission == "submitted"
            //                                 && (trek.Designation == "TL" || trek.Designation == "ATL")
            //                                 && TLForm.Approval == "Approve"
            //                           select new
            //                           {
            //                               trek.TrekId,
            //                               trek.DepartureId,
            //                               trek.Designation,
            //                               trek.TeamMemberName
            //                           })
            //                  .GroupBy(t => new { t.TrekId, t.DepartureId, t.TeamMemberName })
            //                  .Select(g => g.First())  // Pick only one unique record per group
            //                  .ToList();

            //var chefApprovedDetails = (from trek in _context.AddTeamToTrek
            //                           join chefRating in _context.ChefFormRatings
            //                           on new { trek.TrekId, trek.DepartureId } equals new { chefRating.TrekId, chefRating.DepartureId }
            //                           where trek.FormSubmission == "submitted"
            //                                 && (trek.Designation == "Chef" || trek.Designation == "Chef-A")
            //                                 && chefRating.Approval == "Approve"
            //                           select new
            //                           {
            //                               trek.TrekId,
            //                               trek.DepartureId,
            //                               trek.Designation,
            //                               trek.TeamMemberName
            //                           })
            //             .GroupBy(t => new { t.TrekId, t.DepartureId, t.TeamMemberName })
            //             .Select(g => g.First())  // Pick only one unique record per group
            //             .ToList();




            var chefTrekDetails = _context.AddTeamToTrek
               .Where(a => a.FormSubmission == "Submitted" && a.Approval == "Pending" && a.Designation == "Chef")
               .Select(a => new TeamSubmittedApprovalsViewModel
               {
                   TrekName = a.TrekName.Trim(),
                   StartDate = a.StartDate,
                   ToDate = a.ToDate,
                   TrekId = a.TrekId,
                   DepartureId = a.DepartureId,
                   Designation = a.Designation,
                   TeamMemberName = a.TeamMemberName,
                   Id = a.Id,
                   Approval = a.Approval,
                   FormSubmissionDate = a.FormSubmissionDate,
                   Batch=a.Batch


               })
               .ToList();
            ViewBag.SubmittedTrekDetails =
      (submittedTrekDetails?.Count ?? 0) + (chefTrekDetails?.Count ?? 0);


            var viewModel = new TeamFeedbackApprovalViewModel
            {
                PendingApprovals = pendingTrekDetails,
                SubmittedApprovals = submittedTrekDetails,
                ChefTrekDetails = chefTrekDetails
            };

            return View(viewModel);
        }
        [Route("EditBCMForm")]
        [HttpGet]
        public async Task<IActionResult> EditBCMForm(int departureId, int trekId, string teamMemberName, string designation, int id)
        {
            if (string.IsNullOrEmpty(designation))
            {
                return BadRequest("Designation is required.");
            }

            // Split teamMemberName into firstName and lastName if applicable
            string[] nameParts = teamMemberName?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? new string[0];

            string firstName = nameParts.Length > 2
                ? string.Join(" ", nameParts.Take(nameParts.Length - 1))
                : (nameParts.Length > 0 ? nameParts[0] : "");

            string lastName = nameParts.Length > 1 ? nameParts[^1] : "";

            var bcm = await _context.TrekBaseCampManager
      .Where(t => t.TeamId == id
          )
      .Select(t => new { t.Id, t.FromDate, t.ToDate })
      .FirstOrDefaultAsync();


            if (bcm == null)
            {
                return NotFound("Base Camp Manager not found.");
            }

            int bcmId = bcm.Id; // Get the Id when the condition is true
            DateTime bcmstartdate = bcm.FromDate;
            DateTime bcmenddate = bcm.ToDate;





            if (bcmstartdate <= bcmenddate)
            {
                DateTime StartDate = bcmstartdate.AddDays(-5);
                DateTime ToDate = bcmenddate.AddDays(5);

                // Generating a list of DateTime objects
                ViewBag.AvailableDates = Enumerable.Range(0, (bcmenddate - bcmstartdate).Days + 1)
                    .Select(offset => bcmstartdate.AddDays(offset)) // Ensure this returns DateTime
                    .ToList();
            }

            var trekItinerary = await _context.TrekBaseCampManager
                .Include(t => t.HelperFreelancers)
                .Include(t => t.GuideFreelancers)
                .Include(t => t.Mules)
                .Include(t => t.Porters)
                .Include(t => t.MainTransportArrivalDeparture)
                .Include(t => t.MainTransportArrival)
                .Include(t => t.LocalTransportArrival)
                .Include(t => t.LocalTransportArrivalDeparture)
                .Include(t => t.RoomArrivals)
                .Include(t => t.RoomArrivalsDepartures)
                .FirstOrDefaultAsync(t => t.Id == bcmId);

            return View(trekItinerary);
        }


        [Route("SaveEditBaseCampManagerForm")]
        [HttpPost]
        public IActionResult SaveEditBaseCampManagerForm(TrekBaseCampManager model)
        {

            var TeamMemberName = model.FirstName + " " + model.LastName;
            var addedTeamMemberId = 0;
            if (model.DepartureId != 0 && model.TrekId != 0)
            {
                addedTeamMemberId = _context.AddTeamToTrek
               .Where(a => a.TrekId == model.TrekId && a.DepartureId == model.DepartureId && a.TeamMemberName == TeamMemberName && a.Designation == "BCM")
               .Select(a => a.Id)
               .FirstOrDefault();
            }
            else
            {
                addedTeamMemberId = _context.AddTeamToTrek
               .Where(a => a.TrekName == model.TrekName && a.StartDate == model.FromDate && a.ToDate == model.ToDate && a.TeamMemberName == TeamMemberName && a.Designation == "BCM")
               .Select(a => a.Id)
               .FirstOrDefault();
            }
            var trekBaseCampManager = _context.TrekBaseCampManager
                .FirstOrDefault(t => t.DepartureId == model.DepartureId && t.TrekId == model.TrekId);

            if (trekBaseCampManager != null)
            {
                // Update existing record
                trekBaseCampManager.TrekName = model.TrekName;
                trekBaseCampManager.FromDate = model.FromDate;
                trekBaseCampManager.ToDate = model.ToDate;
                trekBaseCampManager.Batch = model.Batch;
                trekBaseCampManager.MaleTrekker = model.MaleTrekker;
                trekBaseCampManager.FemaleTrekker = model.FemaleTrekker;
                trekBaseCampManager.OnlineOffloadMode = model.OnlineOffloadMode;
                trekBaseCampManager.OflineOffloadMode = model.OflineOffloadMode;
                trekBaseCampManager.TeaxtArea = model.TeaxtArea;
                trekBaseCampManager.Email = model.Email;
                trekBaseCampManager.FirstName = model.FirstName;
                trekBaseCampManager.LastName = model.LastName;


                var existingHelper = _context.HelperFreelancer.FirstOrDefault(x => x.BaseCampManagerId == trekBaseCampManager.Id);
                _context.HelperFreelancer.Remove(existingHelper);

                var existingGuideFreelancers = _context.GuideFreelancer.FirstOrDefault(x => x.BaseCampManagerId == trekBaseCampManager.Id);
                _context.GuideFreelancer.Remove(existingGuideFreelancers);

                var existingMules = _context.Mules.FirstOrDefault(x => x.BaseCampManagerId == trekBaseCampManager.Id);
                _context.Mules.Remove(existingMules);

                var existingPorters = _context.Porter.FirstOrDefault(x => x.BaseCampManagerId == trekBaseCampManager.Id);
                _context.Porter.Remove(existingPorters);

                var existingMainTransportArrival = _context.MainTransportArrival.FirstOrDefault(x => x.BaseCampManagerId == trekBaseCampManager.Id);
                _context.MainTransportArrival.Remove(existingMainTransportArrival);

                var existingMainTransportArrivalDeparture = _context.MainTransportArrivalDeparture.FirstOrDefault(x => x.BaseCampManagerId == trekBaseCampManager.Id);
                _context.MainTransportArrivalDeparture.Remove(existingMainTransportArrivalDeparture);

                var existingLocalTransportArrival = _context.LocalTransportArrival.FirstOrDefault(x => x.BaseCampManagerId == trekBaseCampManager.Id);
                _context.LocalTransportArrival.Remove(existingLocalTransportArrival);

                var existingLocalTransportArrivalDeparture = _context.LocalTransportArrivalDeparture.FirstOrDefault(x => x.BaseCampManagerId == trekBaseCampManager.Id);
                _context.LocalTransportArrivalDeparture.Remove(existingLocalTransportArrivalDeparture);

                var existingRoomArrivals = _context.RoomArrival.FirstOrDefault(x => x.BaseCampManagerId == trekBaseCampManager.Id);
                _context.RoomArrival.Remove(existingRoomArrivals);

                var existingRoomArrivalsDepartures = _context.RoomArrivalsDeparture.FirstOrDefault(x => x.BaseCampManagerId == trekBaseCampManager.Id);
                _context.RoomArrivalsDeparture.Remove(existingRoomArrivalsDepartures);



            }

            else
            {
                // Create new record
                trekBaseCampManager = new TrekBaseCampManager
                {
                    TrekName = model.TrekName,
                    FromDate = model.FromDate,
                    ToDate = model.ToDate,
                    DepartureId = model.DepartureId,
                    Batch = model.Batch,
                    MaleTrekker = model.MaleTrekker,
                    FemaleTrekker = model.FemaleTrekker,
                    OnlineOffloadMode = model.OnlineOffloadMode,
                    OflineOffloadMode = model.OflineOffloadMode,
                    TeaxtArea = model.TeaxtArea,
                    Email = model.Email,
                    TrekId = model.TrekId,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    TeamId = addedTeamMemberId

                };
                _context.TrekBaseCampManager.Add(trekBaseCampManager);
                _context.SaveChanges(); // Save to generate Id
            }

            // Assign related entities
            trekBaseCampManager.HelperFreelancers = model.HelperFreelancers?.Select(h => new HelperFreelancer
            {
                Name = h.Name,
                Vendor = h.Vendor,
                Days = h.Days,
                BaseCampManagerId = trekBaseCampManager.Id
            }).ToList() ?? new List<HelperFreelancer>();

            trekBaseCampManager.GuideFreelancers = model.GuideFreelancers?.Select(g => new GuideFreelancer
            {
                Number = g.Number,
                Vendor = g.Vendor,
                Days = g.Days,
                BaseCampManagerId = trekBaseCampManager.Id
            }).ToList() ?? new List<GuideFreelancer>();


            trekBaseCampManager.Mules = model.Mules?.Select(g => new Mule
            {
                Number = g.Number,
                Days = g.Days,
                BaseCampManagerId = trekBaseCampManager.Id  // Assigning Foreign Key
            }).ToList() ?? new List<Mule>();

            trekBaseCampManager.Porters = model.Porters?.Select(g => new Porter
            {
                Number = g.Number,
                Vendor = g.Vendor,
                Days = g.Days,
                BaseCampManagerId = trekBaseCampManager.Id  // Assigning Foreign Key
            }).ToList() ?? new List<Porter>();


            trekBaseCampManager.MainTransportArrival = model.MainTransportArrival?.Select(g => new MainTransportArrival
            {
                Number = g.Number,
                Vendor = g.Vendor,
                BaseCampManagerId = trekBaseCampManager.Id  // Assigning Foreign Key
            }).ToList() ?? new List<MainTransportArrival>();


            trekBaseCampManager.MainTransportArrivalDeparture = model.MainTransportArrivalDeparture?.Select(g => new MainTransportArrivalDeparture
            {
                Number = g.Number,
                Vendor = g.Vendor,
                BaseCampManagerId = trekBaseCampManager.Id  // Assigning Foreign Key
            }).ToList() ?? new List<MainTransportArrivalDeparture>();

            trekBaseCampManager.LocalTransportArrival = model.LocalTransportArrival?.Select(g => new LocalTransportArrival
            {
                Number = g.Number,
                Vendor = g.Vendor,
                BaseCampManagerId = trekBaseCampManager.Id  // Assigning Foreign Key
            }).ToList() ?? new List<LocalTransportArrival>();


            trekBaseCampManager.LocalTransportArrivalDeparture = model.LocalTransportArrivalDeparture?.Select(g => new LocalTransportArrivalDeparture
            {
                Number = g.Number,
                Vendor = g.Vendor,
                BaseCampManagerId = trekBaseCampManager.Id  // Assigning Foreign Key
            }).ToList() ?? new List<LocalTransportArrivalDeparture>();

            trekBaseCampManager.RoomArrivals = model.RoomArrivals?.Select(g => new RoomArrival
            {
                Date = g.Date,
                Number = g.Number,
                Vendor = g.Vendor,
                BaseCampManagerId = trekBaseCampManager.Id  // Assigning Foreign Key
            }).ToList() ?? new List<RoomArrival>();

            trekBaseCampManager.RoomArrivalsDepartures = model.RoomArrivalsDepartures?.Select(g => new RoomArrivalsDeparture
            {
                Date = g.Date,
                Number = g.Number,
                Vendor = g.Vendor,
                BaseCampManagerId = trekBaseCampManager.Id  // Assigning Foreign Key
            }).ToList() ?? new List<RoomArrivalsDeparture>();


            _context.SaveChanges();



            var teamMemberName = model.FirstName + " " + model.LastName;

            //var addTeamToTrek = _context.AddTeamToTrek
            //    .FirstOrDefault(t => t.DepartureId == model.DepartureId &&
            //                         t.TrekId == model.TrekId &&
            //                         t.TeamMemberName == teamMemberName &&

            //                         t.FormSubmission == "Pending"
            //              );
            var addTeamToTrekApproval = _context.AddTeamToTrek
               .FirstOrDefault(t =>t.Id== addedTeamMemberId &&
                                    t.Approval == "Pending"
                         );

            if (addTeamToTrekApproval != null)
            {
                addTeamToTrekApproval.Approval = "Submitted";
                _context.SaveChanges();
            }
            //if (addTeamToTrek != null)
            //{
            //    addTeamToTrek.FormSubmission = "Submitted";
            //    addTeamToTrek.Approval = "Submitted";
            //    _context.SaveChanges();
            //}


            return RedirectToAction("TeamApprovalIndex");

        }

        [Route("EditTLForm")]
        [HttpGet]
        public async Task<IActionResult> EditTLForm(int departureId, int trekId, string teamMemberName, string designation, int id)
        {


            if (string.IsNullOrEmpty(designation))
            {
                return BadRequest("Designation is required.");
            }

            // Split teamMemberName into firstName and lastName if applicable
            string[] nameParts = teamMemberName?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? new string[0];

            string firstName = nameParts.Length > 2
                ? string.Join(" ", nameParts.Take(nameParts.Length - 1))
                : (nameParts.Length > 0 ? nameParts[0] : "");

            string lastName = nameParts.Length > 1 ? nameParts[^1] : "";

            var bcm = await _context.TLForm
      .Where(t => t.DepartureId == departureId && t.TrekId == trekId && t.TeamId == id
)
      .Select(t => new { t.Id, t.StartDate, t.EndDate })
      .FirstOrDefaultAsync();


            if (bcm == null)
            {
                return NotFound("Base Camp Manager not found.");
            }

            int bcmId = bcm.Id; // Get the Id when the condition is true
            //DateTime bcmstartdate = bcm.FromDate;
            //DateTime bcmenddate = bcm.ToDate;





            ViewBag.Participants = _context.TempParticipants
              .Where(p => p.DepartureId == departureId
                  && p.TrekId == trekId
                  && (p.BookingStatus == "Pending" || p.BookingStatus == "Transfer")
                  && p.PaymentStatus == "Paid")
              .Select(p => new ParticipantNamesModel
              {
                  FirstName = p.FirstName,
                  LastName = p.LastName,
                  ParticipantId = p.TempParticipantId

              })
              .ToList();

            var tlForm = await _context.TLForm
                .Include(t => t.CampManagmentRatingTL)
                .Include(t => t.TrekkersStatus)

                .FirstOrDefaultAsync(t => t.Id == bcmId);

            return View(tlForm);
        }


        [Route("SaveEditTLForm")]
        [HttpPost]
        public IActionResult SaveEditTLForm(TLForm model)
        {
            TLForm tlForm = null;

            if (model.DepartureId != 0 && model.TrekId != 0)
            {
                tlForm = _context.TLForm
                    .FirstOrDefault(t => t.DepartureId == model.DepartureId && t.TrekId == model.TrekId);
            }
            else
            {
                tlForm = _context.TLForm
                    .FirstOrDefault(t => t.StartDate == model.StartDate && t.EndDate == model.EndDate && t.TrekName == model.TrekName);
            }


            var TeamMemberName = model.FirstName + " " + model.LastName;
            var addedTeamMemberId = 0;
            if (model.DepartureId != 0 && model.TrekId != 0)
            {
                addedTeamMemberId = _context.AddTeamToTrek
               .Where(a => a.TrekId == model.TrekId && a.DepartureId == model.DepartureId && a.TeamMemberName == TeamMemberName && a.Designation == "TL")
               .Select(a => a.Id)
               .FirstOrDefault();
            }
            else
            {
                addedTeamMemberId = _context.AddTeamToTrek
               .Where(a => a.TrekName == model.TrekName && a.StartDate == model.StartDate && a.ToDate == model.EndDate && a.TeamMemberName == TeamMemberName && a.Designation == "TL")
               .Select(a => a.Id)
               .FirstOrDefault();
            }

            if (tlForm != null)
            {
                // Update existing record
                tlForm.TrekName = model.TrekName;

                tlForm.TrekId = model.TrekId;
                tlForm.DepartureId = model.DepartureId;
                tlForm.StartDate = model.StartDate;
                tlForm.EndDate = model.EndDate;
                tlForm.FirstName = model.FirstName;
                tlForm.LastName = model.LastName;
                tlForm.TLBriefing = model.TLBriefing;
                tlForm.AmsCPRLecture = model.AmsCPRLecture;
                tlForm.FloraFaunaLecture = model.FloraFaunaLecture;
                tlForm.SOSSignalLecture = model.SOSSignalLecture;
                tlForm.TerminologyLecture = model.TerminologyLecture;
                tlForm.RandomTopicLecture = model.RandomTopicLecture;
                tlForm.BPCheck = model.BPCheck;
                tlForm.OxygenCheck = model.OxygenCheck;
                tlForm.HealthFeedbackMorning = model.HealthFeedbackMorning;
                tlForm.HealthFeedbackAfterDinner = model.HealthFeedbackAfterDinner;
                tlForm.Debriefing = model.Debriefing;
                tlForm.OxygenCylinderAndRopeOnsummit = model.OxygenCylinderAndRopeOnsummit;
                tlForm.MealsAndEveningTea = model.MealsAndEveningTea;
                tlForm.CampCleanDrive = model.CampCleanDrive;
                tlForm.HotLunchServed = model.HotLunchServed;
                tlForm.HotTeaServed = model.HotTeaServed;
                tlForm.GReviewCount = model.GReviewCount;

                tlForm.HealthCheckupVandP = model.HealthCheckupVandP;
                tlForm.ExercisePandV = model.ExercisePandV;
                tlForm.LecturePandV = model.LecturePandV;
                tlForm.WhileTrekkingPandV = model.WhileTrekkingPandV;
                tlForm.SceneryPandV = model.SceneryPandV;
                tlForm.FoodPandV = model.FoodPandV;
                tlForm.SubmitPandV = model.SubmitPandV;
                tlForm.CampSitePandV = model.CampSitePandV;
                tlForm.CampCleaningPandV = model.CampCleaningPandV;
                tlForm.TrekkerActivityPandV = model.TrekkerActivityPandV;
                tlForm.InterviewVideo = model.InterviewVideo;
                tlForm.Email = model.Email;
                tlForm.TextArea = model.TextArea;


                var existingCampingRating = _context.CampManagmentRatingTL.FirstOrDefault(x => x.TLFormId == tlForm.Id);
                if (existingCampingRating != null)
                {
                    _context.CampManagmentRatingTL.Remove(existingCampingRating);
                }


                var existingTrekkerStatus = _context.TrekkersStatus.FirstOrDefault(x => x.TLFormId == tlForm.Id);
                if (existingTrekkerStatus != null)
                {
                    _context.TrekkersStatus.Remove(existingTrekkerStatus);
                }


            }
            else
            {


                var edittlForm = new TLForm
                {
                    TrekName = model.TrekName,
                    TrekId = model.TrekId,
                    DepartureId = model.DepartureId,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    TLBriefing = model.TLBriefing,
                    AmsCPRLecture = model.AmsCPRLecture,
                    FloraFaunaLecture = model.FloraFaunaLecture,
                    SOSSignalLecture = model.SOSSignalLecture,
                    TerminologyLecture = model.TerminologyLecture,
                    RandomTopicLecture = model.RandomTopicLecture,
                    BPCheck = model.BPCheck,
                    OxygenCheck = model.OxygenCheck,
                    HealthFeedbackMorning = model.HealthFeedbackMorning,
                    HealthFeedbackAfterDinner = model.HealthFeedbackAfterDinner,
                    Debriefing = model.Debriefing,
                    OxygenCylinderAndRopeOnsummit = model.OxygenCylinderAndRopeOnsummit,
                    MealsAndEveningTea = model.MealsAndEveningTea,
                    CampCleanDrive = model.CampCleanDrive,
                    HotLunchServed = model.HotLunchServed,
                    HotTeaServed = model.HotTeaServed,
                    GReviewCount = model.GReviewCount,

                    HealthCheckupVandP = model.HealthCheckupVandP,
                    ExercisePandV = model.ExercisePandV,
                    LecturePandV = model.LecturePandV,
                    WhileTrekkingPandV = model.WhileTrekkingPandV,
                    SceneryPandV = model.SceneryPandV,
                    FoodPandV = model.FoodPandV,
                    SubmitPandV = model.SubmitPandV,
                    CampSitePandV = model.CampSitePandV,
                    CampCleaningPandV = model.CampCleaningPandV,
                    TrekkerActivityPandV = model.TrekkerActivityPandV,
                    InterviewVideo = model.InterviewVideo,
                    Email = model.Email,
                    TextArea = model.TextArea,
                    TeamId = addedTeamMemberId


                };


                _context.TLForm.Add(edittlForm);
                _context.SaveChanges();
            }
            tlForm.TrekkersStatus = model.TrekkersStatus?.Select(g => new TrekkersStatus
            {
                FirstName = g.FirstName,
                LastName = g.LastName,
                TrekkerStatus = g.TrekkerStatus,
                ParticipantId = g.ParticipantId,

                TLFormId = tlForm.Id // Assigning Foreign Key
            }).ToList() ?? new List<TrekkersStatus>();
            // Save renting details if available
            if (model.CampManagmentRatingTL != null && model.CampManagmentRatingTL.Any())
            {
                foreach (var rentingDetail in model.CampManagmentRatingTL)



                {
                    var renting = new CampManagmentRatingTL
                    {
                        TLFormId = tlForm.Id, // Link it to TLForm
                        toiletRating = rentingDetail.toiletRating,
                        DiningTentRating = rentingDetail.DiningTentRating,
                        KitchenTentRating = rentingDetail.KitchenTentRating,
                        hygieneRating = rentingDetail.hygieneRating,
                        TableServingRating = rentingDetail.TableServingRating
                    };

                    _context.CampManagmentRatingTL.Add(renting);
                }

            }
            var teamMemberName = model.FirstName + " " + model.LastName;

            var addTeamToTrek = _context.AddTeamToTrek
        .FirstOrDefault(t => t.Id == addedTeamMemberId &&
                             t.Approval == "Pending");

            if (addTeamToTrek != null)
            {
           
                addTeamToTrek.Approval = "Submitted";
                _context.SaveChanges();
                return RedirectToAction("TeamApprovalIndex");
            }


            //var addTeamToTrekSub = _context.AddTeamToTrek
            //    .FirstOrDefault(t => t.StartDate == model.StartDate &&
            //                         t.ToDate == model.EndDate &&
            //                         t.TeamMemberName == teamMemberName &&
            //                         t.TrekName == model.TrekName &&
            //                         t.Approval == "Pending");

            //if (addTeamToTrekSub!=null)
            //{
            //    addTeamToTrek.FormSubmission = "Submitted";
            //    addTeamToTrek.Approval = "Submitted";
            //    _context.SaveChanges();
            //    return RedirectToAction("TeamApprovalIndex");
            //}
            return RedirectToAction("TeamApprovalIndex");


            //var addTeamToTrekApproval = _context.AddTeamToTrek
            //   .FirstOrDefault(t => t.DepartureId == model.DepartureId &&
            //                        t.TrekId == model.TrekId &&
            //                        t.TeamMemberName == teamMemberName &&

            //                        t.Approval == "Pending"
            //             );



            //        var addteamtotrekapproval = _context.addteamtotrek
            //.firstordefault(t =>
            //    (model.departureid != null && model.trekid != null
            //        ? t.departureid == model.departureid && t.trekid == model.trekid
            //        : t.startdate == model.startdate && t.todate == model.enddate) &&
            //    t.teammembername == teammembername &&
            //    t.formsubmission == "pending"
            //);





            //if (addTeamToTrekApproval != null)
            //{
            //    addTeamToTrekApproval.Approval = "Submitted";
            //    _context.SaveChanges();
            //}


        }
        [Route("EditChefForm")]
        [HttpGet]
        public async Task<IActionResult> EditChefForm(int departureId, int trekId, string teamMemberName, string designation, int id)
        {
            if (string.IsNullOrEmpty(designation))
            {
                return BadRequest("Designation is required.");
            }
            ViewBag.Id = id;
            int itineraryDaysCount = 0;
            // Split teamMemberName into firstName and lastName if applicable
            string[] nameParts = teamMemberName?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? new string[0];

            string firstName = nameParts.Length > 2
                ? string.Join(" ", nameParts.Take(nameParts.Length - 1))
                : (nameParts.Length > 0 ? nameParts[0] : "");

            string lastName = nameParts.Length > 1 ? nameParts[^1] : "";



            var bcm = await _context.ChefFormRatings
      .Where(t => t.TeamId == id)

      .Select(t => new { t.TrekId, t.DepartureId, t.FromDate, t.ToDate, t.ChefName })
      .FirstOrDefaultAsync();



            if (bcm == null)
            {
                return NotFound("Base Camp Manager not found.");
            }

            int? bcmTrekId = bcm.TrekId; // Get the Id when the condition is true
            int? bcmdepartureId = bcm.DepartureId;
            DateTime bcmstartdate = bcm.FromDate;
            DateTime bcmenddate = bcm.ToDate;





            if (bcmstartdate != null && bcmenddate != null)
            {
                itineraryDaysCount = (bcmenddate - bcmstartdate).Days + 1;
            }




            ViewBag.ItineraryDaysCount = itineraryDaysCount; // Pass count using ViewBag

            var chefForm = await _context.ChefFormRatings

                .Where(t => t.TeamId == id).ToListAsync();

            return View(chefForm);
        }



        [Route("SaveEditChefForm")]
        [HttpPost]
        public IActionResult SaveEditChefForm(IFormCollection form)
        {
            if (!form.ContainsKey("TrekId") || !form.ContainsKey("DepartureId"))
            {
                return BadRequest("Missing trek information.");
            }

            // Safely parse trek details with error checking
            if (!int.TryParse(form["TrekId"].ToString(), out int trekId) ||
                !int.TryParse(form["DepartureId"].ToString(), out int departureId))
            {
                return BadRequest("Invalid trek ID or departure ID.");
            }

            var tlForm = _context.ChefFormRatings
               .Where(t => t.DepartureId == departureId && t.TrekId == trekId).ToList();

            if (tlForm != null)
            {
                _context.ChefFormRatings.RemoveRange(tlForm);
                _context.SaveChanges();


            }
            if (!form.ContainsKey("Id") || !int.TryParse(form["Id"], out int id))
            {
                return BadRequest("Missing or invalid Id.");
            }

            // Basic validation for required trek information

            var trekName = form["TrekName"].ToString();

            // Parse dates with validation
            if (!DateTime.TryParse(form["StartDate"].ToString(), out DateTime startDate) ||
                !DateTime.TryParse(form["EndDate"].ToString(), out DateTime endDate))
            {
                return BadRequest("Invalid date format.");
            }
            if (!int.TryParse(form["ItineraryDaysCount"].ToString(), out int itineraryDaysCount)) { }


            var firstName = form.ContainsKey("FirstName") ? form["FirstName"].ToString() : string.Empty;
            var lastName = form.ContainsKey("LastName") ? form["LastName"].ToString() : string.Empty;
            var email = form.ContainsKey("Email") ? form["Email"].ToString() : string.Empty;


            //var TeamMemberName = firstName + " " + lastName;
            //var addedTeamMemberId = _context.AddTeamToTrek
            //    .Where(a => a.TrekId == trekId && a.DepartureId == departureId && a.TeamMemberName == TeamMemberName && a.Designation == "Chef")
            //    .Select(a => a.Id)
            //    .FirstOrDefault();

            var ratings = new List<ChefFormRatings>();

            int timingRating = 0, hygieneRating = 0, tasteRating = 0,
                asPerMenuRating = 0, involvementRating = 0;
            // Process each day's ratings
            for (int i = 0; i < itineraryDaysCount; i++)
            {

                var chefNameKey = $"ChefName[{i}].Name";
                var timingRatingKey = $"Timing-Rating[{i}]";
                var hygineRatingKey = $"Hygiene-Rating[{i}]";
                var tasteRatingKey = $"Taste-Rating[{i}]";
                var asperMenuRatingKey = $"AsPerMenu-Rating[{i}]";
                var involvementRatingKey = $"Involvement-Rating[{i}]";



                var chefName = form[chefNameKey].ToString();
                if (form.ContainsKey(timingRatingKey) && int.TryParse(form[timingRatingKey], out int tempTimingRating))
                {
                    timingRating = tempTimingRating;
                }


                if (form.ContainsKey(hygineRatingKey) && int.TryParse(form[hygineRatingKey], out int tempHygineRating))
                {
                    hygieneRating = tempHygineRating;
                }

                if (form.ContainsKey(tasteRatingKey) && int.TryParse(form[tasteRatingKey], out int tempTasteRating))
                {
                    tasteRating = tempTasteRating;
                }

                if (form.ContainsKey(asperMenuRatingKey) && int.TryParse(form[asperMenuRatingKey], out int tempasperMenuRating))
                {
                    asPerMenuRating = tempasperMenuRating;
                }

                if (form.ContainsKey(involvementRatingKey) && int.TryParse(form[involvementRatingKey], out int tempinvolvementRating))
                {
                    involvementRating = tempinvolvementRating;
                }






                // Create new rating entry
                var rating = new ChefFormRatings
                {
                    TrekId = trekId,
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,

                    DepartureId = departureId,
                    TrekName = trekName,
                    FromDate = startDate,
                    ToDate = endDate,
                    ChefName = chefName,
                    CampDays = i.ToString(),
                    TimingRating = timingRating,
                    HygieneRating = hygieneRating,
                    TasteRating = tasteRating,
                    AsPerMenuRating = asPerMenuRating,
                    InvolvementRating = involvementRating,
                    TeamId = id

                };

                ratings.Add(rating);
            }

            //if (!ratings.Any())
            //{
            //    return BadRequest("No valid ratings to save.");
            //}
            _context.ChefFormRatings.AddRange(ratings);
            _context.SaveChanges();
            var teamMemberName = firstName + " " + lastName;

            var addTeamToTrek = _context.AddTeamToTrek
                .FirstOrDefault(t =>
                                     t.Id == id &&

                                    t.Approval == "Pending"
                          );



            if (addTeamToTrek != null)
            {

                addTeamToTrek.Approval = "Submitted";
                _context.SaveChanges();
            }
            return RedirectToAction("TeamApprovalIndex");
        }


        [HttpGet]
        [Route("AllApprovalForm")]
        public IActionResult AllApprovalForm()
        {


            var submittedTrekDetails = _context.AddTeamToTrek
                .Where(a => a.FormSubmission == "Submitted" && a.Approval == "Submitted")
                .Select(a => new TeamSubmittedApprovalsViewModel
                {
                    TrekName = a.TrekName,
                    StartDate = a.StartDate,
                    ToDate = a.ToDate,
                    TrekId = a.TrekId,
                    DepartureId = a.DepartureId,
                    Designation = a.Designation,
                    TeamMemberName = a.TeamMemberName,
                    Id = a.Id

                })
                .ToList();





            ViewBag.submittedTrekDetails = submittedTrekDetails.Count;
            var viewModel = new TeamFeedbackApprovalViewModel
            {

                SubmittedApprovals = submittedTrekDetails
            };

            return View(viewModel);
        }


        [HttpGet]
        [Route("AllEmployeeData")]
        public IActionResult AllEmployeeData()
        {
            var positionOrder = new[] { "BCM", "TL", "ATL", "Chef", "A-Chef", "Helper", "Guide", "CM" };

            var employees = _context.Users
                .Where(u => u.Located == "Operation" && positionOrder.Contains(u.Position))
                .Select(u => new
                {
                    FullName = u.FirstName + " " + u.LastName,
                    Designation = u.Position,
                    u.Email
                })
                .ToList()
                .OrderBy(e => positionOrder.ToList().IndexOf(e.Designation))
                .ToList();

            var employeeNames = employees.Select(e => e.FullName).Distinct().ToList();
            var emplyeeDesignation = employees.Select(e => e.Designation).ToList();

            var trekDatesSet = new HashSet<string>();
            var assignments = new Dictionary<string, Dictionary<string, string>>();

            //var twoMonthsAgo = DateTime.Today.AddMonths(-2);

            //DateTime tenDaysAgo = DateTime.Today.AddDays(-10);





            var today = DateTime.Today;
            var oneMonthLater = today.AddMonths(1);

            var startDates = _context.TrekDeparture
    .Where(td =>
                    (td.HideFrom == "none" || td.HideFrom == "website")) // Filter by TrekId in trekIds list
                                                                         // Sort by StartDate in ascending order
     .Select(d => new
     {
         d.DepartureId,
         d.Batch,
         d.StartDate,
         d.TrekId
     })
     .ToList();

            DateTime Today = DateTime.Today;
            var AllsevenDaysAgo = Today.AddDays(-16);






            var AllFixedFromDates = startDates
           .Where(fixedDate => fixedDate.StartDate >= AllsevenDaysAgo && fixedDate.StartDate < today)
             .OrderBy(fixedDate => fixedDate.StartDate)
           .ToList();

            var afterAllFixedFromDates = startDates
                    .Where(trekDetailsItinerary => trekDetailsItinerary.StartDate >= today && trekDetailsItinerary.StartDate <= oneMonthLater)
                    .OrderBy(trekDetailsItinerary => trekDetailsItinerary.StartDate) // Sort by date (earliest first)
                    .ToList();

            // Combine both lists
            //var combinedAllFixedFromDates = AllFixedFromDates.Concat(afterAllFixedFromDates).ToList();

            var combinedAllFixedFromDates = AllFixedFromDates
    .Concat(afterAllFixedFromDates) // Merge both lists
    .OrderBy(item => item.StartDate)  // Order the combined list by TrekId
    .ToList();





            var trekDetails = _context.TrekItineraries
    .Include(t => t.ItineraryDetails)
    .Select(t => new
    {
        t.TrekId,
        t.StartDate,
        t.EndDate,
        t.Id,

        t.ItineraryType,
        ItineraryDetails = t.ItineraryDetails
            .Select(d => new
            {
                d.ItineraryDay,
                d.ItineraryHeading,
                d.TrekId,

            })
            .ToList(),
        DaysCount = t.ItineraryDetails.Count()
    })
    .ToList();

            var customizeTrekDetails = trekDetails.Where(t => t.ItineraryType == "Customize").ToList();


            var AllCoustomizeFromDates = customizeTrekDetails
                  .Where(fixedDate => fixedDate.StartDate >= AllsevenDaysAgo && fixedDate.StartDate < today)
                  .ToList();



            var afterAllCustomizeFromDates = customizeTrekDetails
                .Where(trekDetailsItinerary => trekDetailsItinerary.StartDate >= today && trekDetailsItinerary.StartDate <= oneMonthLater)
                .OrderBy(trekDetailsItinerary => trekDetailsItinerary.StartDate) // Sort by date (earliest first)
                .ToList();

            // Combine both lists
            var combinedAllCustomizeFromDates = AllCoustomizeFromDates.Concat(afterAllCustomizeFromDates).OrderBy(item => item.TrekId).ToList();



            var FixedTrekIdsdetails = trekDetails
.Where(t => t.ItineraryType == "Fixed")
.Select(t => t.TrekId)
.ToList();



            var FixedTrekIdsdetailsId = trekDetails
 .Where(t => t.ItineraryType == "Fixed")
 .Select(t => t.Id)
 .ToList();
            var AllTrekItineraries = _context.TrekDepartureItineraryDetail
                    .Where(td => FixedTrekIdsdetails.Contains(td.TrekId))
                    .Select(d => new
                    {
                        d.TrekId,
                        d.FromDate,
                        d.ToDate,
                        d.Id,
                        d.Batch,

                        ItineraryDetails = _context.TrekDepartureItinerary
                             .Where(td => EF.Property<int>(td, "TrekDepartureItineraryDetailId") == d.Id)
                            .Select(td => new
                            {
                                td.ItineraryDay,
                                td.ItineraryHeading,
                                td.TrekId

                            })
                            .ToList(),
                        DaysCount = _context.TrekDepartureItinerary
                             .Where(td => EF.Property<int>(td, "TrekDepartureItineraryDetailId") == d.Id)
                            .Count()
                    })
                    .ToList();
            var itineraryDetails = _context.ItineraryDetails
                 .Where(d => FixedTrekIdsdetailsId.Contains(d.TrekItineraryId))
                 .Select(d => new
                 {
                     d.ItineraryHeading,
                     d.ItineraryDay,
                     d.TrekId
                 })
                 .ToList();


            foreach (var departure in combinedAllFixedFromDates)
            {
                int fromDate = 0;
                int toDate = 0;

                // Get FromDate
                string fromDateStr = _context.TrekItineraries
                    .Where(t => t.TrekId == departure.TrekId)
                    .Select(t => t.FromDate)
                    .FirstOrDefault();

                if (!string.IsNullOrEmpty(fromDateStr))
                {
                    int.TryParse(fromDateStr, out fromDate);
                }

                // Get ToDate
                string toDateStr = _context.TrekItineraries
                    .Where(t => t.TrekId == departure.TrekId)
                    .Select(t => t.ToDate)
                    .FirstOrDefault();

                if (!string.IsNullOrEmpty(toDateStr))
                {
                    int.TryParse(toDateStr, out toDate);
                }


                var team = _context.AddTeamToTrek
                     .Where(t => t.TrekId == departure.TrekId && t.StartDate == departure.StartDate)
                     .ToList();

                foreach (var member in team)
                {
                    string heading = "";

                    if (member.Designation == "BCM")
                    {
                        var currentDate = departure.StartDate;
                        var dateKey = currentDate.ToString("dd-MM-yyyy");

                        trekDatesSet.Add(dateKey);

                        heading = _context.TrekDetails
                            .Where(t => t.TrekId == departure.TrekId)
                            .Select(t => t.TrekName)
                            .FirstOrDefault();

                        if (!assignments.ContainsKey(dateKey))
                            assignments[dateKey] = new Dictionary<string, string>();

                        assignments[dateKey][member.TeamMemberName] = heading;
                    }
                    else
                    {
                        if (fromDate > 0 && toDate > 0)
                        {
                            for (int i = fromDate; i <= toDate; i++)
                            {
                                var currentDate = departure.StartDate.AddDays(i - 1);
                                var dateKey = currentDate.ToString("yyyy-MM-dd");

                                trekDatesSet.Add(dateKey);

                                if (!assignments.ContainsKey(dateKey))
                                    assignments[dateKey] = new Dictionary<string, string>();

                                var itineraryDetail = itineraryDetails
                                    .Where(it => it.TrekId == departure.TrekId)
                                    .ElementAtOrDefault(i - 1);

                                if (itineraryDetail != null)
                                {
                                    heading = itineraryDetail.ItineraryHeading;
                                }

                                assignments[dateKey][member.TeamMemberName] = heading;
                            }
                        }
                    }
                }
            }
            foreach (var departure in combinedAllCustomizeFromDates)
            {
                int fromDate = 0;
                int toDate = 0;

                // Get FromDate
                string fromDateStr = _context.TrekItineraries
                    .Where(t => t.TrekId == departure.TrekId && t.ItineraryType == "Customize")
                    .Select(t => t.FromDate)
                    .FirstOrDefault();

                if (!string.IsNullOrEmpty(fromDateStr))
                {
                    int.TryParse(fromDateStr, out fromDate);
                }

                // Get ToDate
                string toDateStr = _context.TrekItineraries
                    .Where(t => t.TrekId == departure.TrekId && t.ItineraryType == "Customize")
                    .Select(t => t.ToDate)
                    .FirstOrDefault();

                if (!string.IsNullOrEmpty(toDateStr))
                {
                    int.TryParse(toDateStr, out toDate);
                }


                var team = _context.AddTeamToTrek
                     .Where(t => t.TrekId == departure.TrekId && t.StartDate == departure.StartDate)
                     .ToList();

                foreach (var member in team)
                {
                    string heading = "";

                    if (member.Designation == "BCM")
                    {
                        var currentDate = departure.StartDate;
                        var dateKey = currentDate.Value.ToString("dd-MM-yyyy");

                        trekDatesSet.Add(dateKey);

                        heading = _context.TrekDetails
                            .Where(t => t.TrekId == departure.TrekId)
                            .Select(t => t.TrekName)
                            .FirstOrDefault();

                        if (!assignments.ContainsKey(dateKey))
                            assignments[dateKey] = new Dictionary<string, string>();

                        assignments[dateKey][member.TeamMemberName] = heading;
                    }
                    else
                    {
                        if (fromDate > 0 && toDate > 0)
                        {
                            for (int i = fromDate; i <= toDate; i++)
                            {
                                var currentDate = departure.StartDate.Value.AddDays(i - 1);
                                var dateKey = currentDate.ToString("dd-MM-yyyy");

                                trekDatesSet.Add(dateKey);

                                if (!assignments.ContainsKey(dateKey))
                                    assignments[dateKey] = new Dictionary<string, string>();

                                var itineraryDetail = itineraryDetails
                                    .Where(it => it.TrekId == departure.TrekId)
                                    .ElementAtOrDefault(i - 1);

                                if (itineraryDetail != null)
                                {
                                    heading = itineraryDetail.ItineraryHeading;
                                }

                                assignments[dateKey][member.TeamMemberName] = heading;
                            }
                        }
                    }
                }
            }

            var viewModel = new TrekAssignmentToEmplyeeViewModel
            {
                EmployeesDesignation = emplyeeDesignation,
                EmployeeNames = employeeNames,
                TrekDates = trekDatesSet.OrderBy(d => d).ToList(),
                Assignments = assignments
            };


            return View(viewModel);
        }

        [HttpPost]
        [Route("DeleteEmployeeLeave/{id}")]
        public async Task<IActionResult> DeleteEmployeeLeave(int id)
        {
            var leave = await _context.AddTeamToTrek.FindAsync(id);

            if (leave == null)
            {
                return NotFound(new { message = $"Leave with ID {id} not found." });
            }

            _context.AddTeamToTrek.Remove(leave);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}



