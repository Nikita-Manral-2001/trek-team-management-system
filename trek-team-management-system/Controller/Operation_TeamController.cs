using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TTH.Areas.Super.Data;
using TTH.Areas.Super.Repository.RentRepository;
using TTH.Areas.Super.Repository;
using TTH.Models.user;
using TTH.Areas.Super.Data.Operation_Team;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.Identity.Client;
using TTH.Areas.Super.Models.Trek;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;
using Microsoft.Build.Framework;
using Microsoft.DotNet.Scaffolding.Shared;
using TTH.Areas.Super.Models;
using Microsoft.AspNetCore.Authorization;

namespace TTH.Areas.Super.Controllers
{
    [Area("Super")]
    [Route("super/[controller]")]
    [Authorize(Roles = "Super,TLM,OperationTeam")]
    public class Operation_TeamController : Controller
    {
        private readonly AppDataContext _context;
        
        public Operation_TeamController(AppDataContext context)
        {
            
            _context = context;

        }
        [HttpGet]
        [Route("Add_Team")]
        public IActionResult Add_Team(bool isSuccess = false)
        {
            ViewBag.IsSuccess = isSuccess;
            return View();
        }

        [HttpPost]
        [Route("Add_Team")]
        public async Task<IActionResult> Add_Team(Add_Team addTeam)
        {
            // Handle single file upload for Photo
            if (addTeam.Photo != null && addTeam.Photo.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Team_Operation");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + addTeam.Photo.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await addTeam.Photo.CopyToAsync(stream);
                }

                addTeam.PhotoPath = $"/Team_Operation/{uniqueFileName}";
            }

            // Handle multiple file uploads for Upload_Id
            if (addTeam.Upload_Id != null && addTeam.Upload_Id.Any())
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Team_Operation/Upload_Ids");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var filePaths = new List<string>();
                foreach (var file in addTeam.Upload_Id)
                {
                    if (file.Length > 0)
                    {
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        filePaths.Add($"/Team_Operation/Upload_Ids/{uniqueFileName}");
                    }
                }

                // Save file paths as JSON
                addTeam.UploadIdPaths = Newtonsoft.Json.JsonConvert.SerializeObject(filePaths);
            }

            
            // Process Education and Cource fields to include commas and remove HTML tags
            addTeam.Education = CleanHtmlAndConvertToCsv(addTeam.Education);
            addTeam.Cource = CleanHtmlAndConvertToCsv(addTeam.Cource);

          
            addTeam.PhoneNo = addTeam.PhoneNo;
            // Save data to database
            _context.Add_Team.Add(addTeam);
            await _context.SaveChangesAsync();

            return RedirectToAction("Add_Team", new { isSuccess = true });
        }

        private string CleanHtmlAndConvertToCsv(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Replace closing </p> tags with commas
            string textWithCommas = input.Replace("</p>", ", ");

            // Remove all other HTML tags
            string cleanText = System.Text.RegularExpressions.Regex.Replace(textWithCommas, "<.*?>", string.Empty);

            // Split by commas, trim each item, and join back into a single string
            var items = cleanText.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                 .Select(item => item.Trim())
                                 .Where(item => !string.IsNullOrEmpty(item));

            return string.Join(", ", items);
        }




        [HttpGet]
        [Route("DownloadFiles")]
        public async Task<IActionResult> DownloadFiles(int id)
        {
            var teamMember = await _context.Add_Team.FindAsync(id);
            if (teamMember == null || string.IsNullOrEmpty(teamMember.UploadIdPaths))
            {
                return NotFound("Team member or files not found.");
            }

            List<string> filePaths;
            try
            {
                
                filePaths = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(teamMember.UploadIdPaths) ?? new List<string>();
            }
            catch (Exception ex)
            {
               
                
                filePaths = new List<string>(); 
            }

           
            if (!filePaths.Any())
            {
                return NotFound("No valid file paths found.");
            }

            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    using (var archive = new System.IO.Compression.ZipArchive(memoryStream, System.IO.Compression.ZipArchiveMode.Create, true))
                    {
                        foreach (var filePath in filePaths)
                        {
                            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath.TrimStart('/'));
                            if (System.IO.File.Exists(fullPath))
                            {
                                var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);
                                var fileName = Path.GetFileName(fullPath);
                                var zipEntry = archive.CreateEntry(fileName);
                                using (var entryStream = zipEntry.Open())
                                {
                                    await entryStream.WriteAsync(fileBytes, 0, fileBytes.Length);
                                }
                            }
                         
                        }
                    }

                    return File(memoryStream.ToArray(), "application/zip", $"TeamMember_{id}_Files.zip");
                }
            }
            catch (Exception ex)
            {
               
               
                return StatusCode(500, "Internal server error while processing the files.");
            }
        }



        [HttpGet]
        [Route("All_Team")]
        public async Task<IActionResult> All_Team(int id)
        {
            var teamMembers = await _context.Add_Team.ToListAsync();
            return View(teamMembers);

        }


        [HttpPost]
        [Route("Delete_Team")]
        public async Task<IActionResult> Delete_Team(int id)
        {
            var teamMember = await _context.Add_Team.FindAsync(id);
            if (teamMember != null)
            {
                _context.Add_Team.Remove(teamMember);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("All_Team");
        }

        [HttpGet]
        public IActionResult Edit_Team(int id) 
        {
            var teamMember = _context.Add_Team.Find(id); 
            if (teamMember == null)
            {
                return NotFound();
            }
            return View(teamMember);
        }



        [HttpPost]
        public IActionResult Edit_Team(Add_Team updatedTeamMember)
        {
            if (ModelState.IsValid)
            {
                var existingTeamMember = _context.Add_Team.Find(updatedTeamMember.Id);
                if (existingTeamMember == null)
                {
                    return NotFound();
                }
      

                existingTeamMember.Name = updatedTeamMember.Name;
                existingTeamMember.PhoneNo = updatedTeamMember.PhoneNo;
                existingTeamMember.Email = updatedTeamMember.Email;
                existingTeamMember.DateOfJoining = updatedTeamMember.DateOfJoining;
                existingTeamMember.Cource = updatedTeamMember.Cource;
                existingTeamMember.Education = updatedTeamMember.Education;
                existingTeamMember.DOB = updatedTeamMember.DOB;
                existingTeamMember.Position = updatedTeamMember.Position;
                existingTeamMember.Gender = updatedTeamMember.Gender;
                existingTeamMember.Location = updatedTeamMember.Location;

                if (updatedTeamMember.Photo != null)
                {
                    string uniqueFileName = SavePhoto(updatedTeamMember.Photo);
                    existingTeamMember.PhotoPath = uniqueFileName;
                }

                // Handle multiple file uploads for Upload_Id during the edit process
                if (updatedTeamMember.Upload_Id != null && updatedTeamMember.Upload_Id.Any())
                {
                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Team_Operation/Upload_Ids");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var uploadPaths = new List<string>();
                    foreach (var file in updatedTeamMember.Upload_Id)
                    {
                        if (file.Length > 0)
                        {
                            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                file.CopyTo(stream);
                            }

                            uploadPaths.Add($"/Team_Operation/Upload_Ids/{uniqueFileName}");
                        }
                    }

                    // Save file paths as JSON
                    existingTeamMember.UploadIdPaths = Newtonsoft.Json.JsonConvert.SerializeObject(uploadPaths);
                }

                _context.SaveChanges();
                return RedirectToAction("All_Team");
            }
            return View(updatedTeamMember);
        }


        private string SavePhoto(IFormFile photo)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            var uniqueFileName = Guid.NewGuid().ToString() + "_" + photo.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                photo.CopyTo(fileStream);
            }

            return "/uploads/" + uniqueFileName;
        }
        private string SaveFile(IFormFile file)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }
            var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(fileStream);
            }

            return "/uploads/" + uniqueFileName;
        }

        [HttpGet]
        [Route("View_Team")]
        public IActionResult View_Team(int id)
        {
            var teamMember = _context.Add_Team.FirstOrDefault(t => t.Id == id);

            if (teamMember == null)
            {
                return NotFound();
            }
      
            return View(teamMember);

        }
        [HttpGet]
        [Route("Add_Trek_Itinerary")]
        public IActionResult Add_Trek_Itinerary(bool isSuccess = false)
        
        {
            ViewBag.IsSuccess = isSuccess;

         

            // Get the Treks from TrekDetails table, excluding those that are already in TrekItineraries
            var treks = _context.TrekDetails
              
                .Select(t => new TrekModelForDbTable
                {
                    TrekId = t.TrekId,
                    TrekName = t.TrekName
                })
                .ToList();




            // Get TrekIds that are already in the TrekItineraries table
            var trekIdsInItineraries = _context.TrekItineraries
      .Where(ti => ti.ItineraryType == "Fixed")
      .Select(ti => ti.TrekId)
      .ToHashSet();

            var fixedTreks = _context.TrekDetails
                .Where(t => !trekIdsInItineraries.Contains(t.TrekId))
                .Select(t => new TrekModelForDbTable
                {
                    TrekId = t.TrekId,
                    TrekName = t.TrekName
                })
                .ToList();

            // Prepare the model
            var model = new TrekViewModel
            {
                FixedTreks= fixedTreks,

                Treks = treks
            };


            

            return View(model);
        }




        [HttpPost]
        [Route("Add_Trek_Itinerary")]
        public async Task<IActionResult> Add_Trek_Itinerary(IFormCollection form)
        {
            string trekName;
            int trekId;
            string trekShortName = form["TrekShortName"];

            // Check if "Other" is selected and a custom trek name is entered
            if (form["trekId"] == "Other")
            {
                trekName = form["customTrekName"];
                if (string.IsNullOrWhiteSpace(trekName))
                {
                    ModelState.AddModelError("", "Custom Trek Name is required.");
                    return View();
                }

                var existingTrek = _context.TrekDetails
                    .FirstOrDefault(t => t.TrekName.ToLower() == trekName.ToLower());

                if (existingTrek == null)
                {
                    trekId = GenerateUniqueTrekId();
                    await _context.SaveChangesAsync();
                }
                else
                {
                    trekId = existingTrek.TrekId;
                }
            }
            else
            {
                if (!int.TryParse(form["trekId"], out trekId))
                {
                    ModelState.AddModelError("", "Invalid Trek selection.");
                    return View();
                }

                trekName = _context.TrekDetails
                    .Where(t => t.TrekId == trekId)
                    .Select(t => t.TrekName)
                    .FirstOrDefault();

                if (string.IsNullOrWhiteSpace(trekName))
                {
                    ModelState.AddModelError("", "Trek not found.");
                    return View();
                }
            }

            string itineraryType = form["ItineraryType"];
            if (string.IsNullOrWhiteSpace(itineraryType))
            {
                ModelState.AddModelError("", "Itinerary type is required.");
                return View();
            }

            DateTime? startDate = null;
            DateTime? endDate = null;

            // Try parsing StartDate
            if (DateTime.TryParse(form["startDate"], out DateTime parsedStartDate))
            {
                startDate = parsedStartDate;
            }

            // Try parsing EndDate
            if (DateTime.TryParse(form["endDate"], out DateTime parsedEndDate))
            {
                endDate = parsedEndDate;
            }

            // Create a new TrekItinerary
            var trekItinerary = new TrekItinerary
            {
                TrekId = trekId,
                TrekName = trekName,
                StartDate = startDate, // Allow null
                EndDate = endDate,     // Allow null
                FromDate = form["FromDate"],
                ToDate = form["ToDate"],
                ItineraryType = itineraryType,
                TrekShortName= trekShortName,
                ItineraryDetails = new List<ItineraryDetail>()
            };

            int dayCount = 0;

            if (DateTime.TryParse(form["startDate"], out DateTime StartDate) &&
                DateTime.TryParse(form["endDate"], out DateTime EndDate))
            {
                // Populate ItineraryDetails with dates between StartDate and EndDate
                for (DateTime date = StartDate; date <= EndDate; date = date.AddDays(1))
                {
                    string dayKey = $"day{dayCount + 1}";
                    string itineraryHeading = form[dayKey];

                    if (string.IsNullOrWhiteSpace(itineraryHeading))
                    {
                        break;
                    }

                    trekItinerary.ItineraryDetails.Add(new ItineraryDetail
                    {
                        ItineraryDay = $"Day {dayCount + 1}",
                        ItineraryHeading = itineraryHeading,
                        ItineraryDate = date, // Save the valid date
                        TrekId = trekId
                    });

                    dayCount++;
                }
            }
            else
            {
                // Handle null StartDate and EndDate by saving null ItineraryDate
                while (true)
                {
                    string dayKey = $"day{dayCount + 1}";
                    string itineraryHeading = form[dayKey];

                    if (string.IsNullOrWhiteSpace(itineraryHeading))
                    {
                        break;
                    }

                    trekItinerary.ItineraryDetails.Add(new ItineraryDetail
                    {
                        ItineraryDay = $"Day {dayCount + 1}",
                        ItineraryHeading = itineraryHeading,
                        ItineraryDate = null, // Save null ItineraryDate
                        TrekId = trekId
                    });

                    dayCount++;
                }
            }

            // Set the ItineraryDayCount in TrekItinerary
            trekItinerary.ItineraryDayCount = dayCount;

            // Save the Itinerary
            _context.TrekItineraries.Add(trekItinerary);
            await _context.SaveChangesAsync();

            return RedirectToAction("Add_Trek_Itinerary", new { isSuccess = true });
        }




        private int GenerateUniqueTrekId()
        {
            var allTrekIds = _context.TrekDetails.Select(t => t.TrekId)
                               .Union(_context.TrekItineraries.Select(ti => ti.TrekId))
                               .ToList();

            return allTrekIds.Any() ? allTrekIds.Max() + 1 : 1; // Start from 1 if no IDs exist
        }



        [HttpGet]
        [Route("SetItinerary")]
        public async Task<IActionResult> SetItinerary()
        {
            var trekItineraries = await _context.TrekItineraries
      .Include(t => t.ItineraryDetails) 
      .ToListAsync();

            return View(trekItineraries);
        }

        


        [HttpGet]
        [Route("Edit_Itinerary/{id}")]
        public async Task<IActionResult> Edit_Itinerary(int id, int? departureId)
        {
            var trekItinerary = await _context.TrekItineraries
                .Include(t => t.ItineraryDetails)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (trekItinerary == null)
            {
                return NotFound();
            }

            // Get the available departures for the trek
            var trekDepartures = await _context.TrekDeparture
                .Where(td => td.TrekId == trekItinerary.TrekId)
                .ToListAsync();
            if (departureId.HasValue)
            {
                var selectedDeparture = trekDepartures.FirstOrDefault(td => td.DepartureId == departureId.Value);
                if (selectedDeparture != null)
                {
                    ViewBag.SelectedDepartureId = selectedDeparture.DepartureId;
                    ViewBag.SelectedDepartureDate = selectedDeparture.StartDate.ToString("yyyy-MM-dd");
                    ViewBag.SelectedDate = departureId;
                }
            }

            ViewBag.TrekDepartures = trekDepartures;
            // Check if any TrekDepartureItineraryDetails exist for the TrekId and DepartureId
            var trekDepartureItineraryDetails = await _context.TrekDepartureItineraryDetail
                .Where(tdd => tdd.TrekId == trekItinerary.TrekId)
                .ToListAsync();

            ViewBag.TrekDepartures = trekDepartures;
            ViewBag.TrekDepartureItineraryDetails = trekDepartureItineraryDetails;

            // Initialize ItineraryDetails if it's null
            if (trekItinerary.ItineraryDetails == null)
            {
                trekItinerary.ItineraryDetails = new List<ItineraryDetail>();
            }

            return View(trekItinerary);
        }


        [HttpPost]
        [Route("Delete_Itinerary")]
        public async Task<IActionResult> Delete_Itinerary(int id, int TrekId)
        {
            // Find the trek itinerary including its details
            var trekItinerary = await _context.TrekItineraries
                .Include(t => t.ItineraryDetails)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (trekItinerary != null)
            {
                // Check and remove entries from TrekDepartureItineraryDetail table where TrekId matches
                var trekDepartureItineraryDetails = await _context.TrekDepartureItineraryDetail
                    .Where(td => td.TrekId == TrekId)
                    .ToListAsync();

                if (trekDepartureItineraryDetails.Any())
                {
                    _context.TrekDepartureItineraryDetail.RemoveRange(trekDepartureItineraryDetails);
                }

                // Remove the main trek itinerary
                _context.TrekItineraries.Remove(trekItinerary);

                // Save changes
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("SetItinerary");
        }

        [HttpPost]
        [Route("Delete_Itinerary_Days")]
        public async Task<IActionResult> Delete_Itinerary_Days(int id)
        {
            var itineraryDetail = await _context.ItineraryDetails.FindAsync(id);

            if (itineraryDetail != null)
            {
                var trekItineraryId = itineraryDetail.TrekItineraryId;

                _context.ItineraryDetails.Remove(itineraryDetail);
                await _context.SaveChangesAsync();

                var remainingDays = await _context.ItineraryDetails
                    .Where(d => d.TrekItineraryId == trekItineraryId)
                    .ToListAsync();

                if (!remainingDays.Any())
                {
                    var trekItinerary = await _context.TrekItineraries.FindAsync(trekItineraryId);
                    if (trekItinerary != null)
                    {
                        _context.TrekItineraries.Remove(trekItinerary);
                        await _context.SaveChangesAsync();
                    }
                }

        
                return RedirectToAction("SeeItinerary", "Operation_Team", new { id = trekItineraryId });
            }

          
            return RedirectToAction("SeeItinerary", "Operation_Team", new { id = id });
        }

       

        [HttpPost]
        [Route("Edit_Itinerary/{id}")]
        public async Task<IActionResult> Edit_Itinerary(int id, TrekItinerary model, [FromForm] int? TrekDeparture, int SelectedDate)
        {
            
            var trekItinerary = await _context.TrekItineraries
                .Include(t => t.ItineraryDetails)
                .FirstOrDefaultAsync(t => t.Id == id);

            var TrekItineraries = _context.TrekItineraries.FirstOrDefault(t => t.Id == model.Id);

            if (trekItinerary == null)
            {
                return NotFound();
            }

           
            trekItinerary.TrekName = model.TrekName;
            trekItinerary.TrekShortName = model.TrekShortName;
        
            trekItinerary.FromDate = model.FromDate;  // Still as string
            trekItinerary.ToDate = model.ToDate;
            trekItinerary.TrekShortName = model.TrekShortName;

            if (SelectedDate == 0)
            {
                if (model.ItineraryDetails != null && model.ItineraryDetails.Any())
                {
                    // Fetch the existing itinerary details to remove
                    var itineraryDetailsToUpdate = trekItinerary.ItineraryDetails
                        .Where(detail => detail.TrekId == trekItinerary.TrekId && detail.TrekItineraryId == trekItinerary.Id)
                        .ToList();

                    // Remove the existing itinerary details from the context
                    _context.ItineraryDetails.RemoveRange(itineraryDetailsToUpdate);

                    // Save changes to ensure deletions are persisted
                    _context.SaveChanges();

                    // Add the updated itinerary details
                    int dayCounter = 1;
                    DateTime? itineraryDate = model.StartDate; // Use nullable DateTime

                    foreach (var detail in model.ItineraryDetails)
                    {
                        var updatedDetail = new ItineraryDetail
                        {
                            ItineraryDay = $"Day {dayCounter}",
                            ItineraryHeading = detail.ItineraryHeading,
                            TrekItineraryId = trekItinerary.Id,
                            TrekId = trekItinerary.TrekId,
                            ItineraryDate = itineraryDate // If null, it will be saved as null
                        };

                        // If StartDate is not null, increment the date by 1 day
                        if (itineraryDate.HasValue)
                        {
                            itineraryDate = itineraryDate.Value.AddDays(1);
                        }

                        _context.ItineraryDetails.Add(updatedDetail);
                        dayCounter++;
                    }

                    // Save changes again to persist the added itinerary details
                    _context.SaveChanges();

                    // Update TrekItineraries with FromDate, ToDate, and ItineraryDayCount
                    var trekItineraries = _context.TrekItineraries.FirstOrDefault(t => t.Id == model.Id);
                    if (trekItineraries != null)
                    {
                        trekItineraries.FromDate = model.FromDate;
                        trekItineraries.ToDate = model.ToDate;
                        trekItineraries.ItineraryDayCount = model.ItineraryDetails.Count(); // Store total day count

                        _context.TrekItineraries.Update(trekItineraries);
                        await _context.SaveChangesAsync();
                    }
                }
            }




            else
            {
                var departure = await _context.TrekDeparture
                    .FirstOrDefaultAsync(d => d.DepartureId == SelectedDate);

                if (departure == null)
                {
                    return BadRequest("No departure found for the selected date.");
                }

                // Check if a TrekDepartureItineraryDetail record already exists
                var existingDetailRecord = await _context.TrekDepartureItineraryDetail
                    .FirstOrDefaultAsync(d => d.TrekId == trekItinerary.TrekId && d.DepartureId == departure.DepartureId);

                TrekDepartureItineraryDetail detailRecordToUse;

                if (existingDetailRecord == null)
                {
                    // Create a new record for TrekDepartureItineraryDetail if it doesn't exist
                    detailRecordToUse = new TrekDepartureItineraryDetail
                    {
                        TrekId = trekItinerary.TrekId,
                        TrekName = trekItinerary.TrekName,
                        TrekShortName=trekItinerary.TrekShortName,
                        DepartureId = departure.DepartureId,
                        Batch=departure.Batch,
                        FromDate = departure?.StartDate ?? DateTime.MinValue,
                        ToDate = departure?.EndDate ?? DateTime.MinValue,
                        TrekDepartureItinerary = new List<TrekDepartureItinerary>()
                    };

                    // Add the new detail record to the database
                    _context.TrekDepartureItineraryDetail.Add(detailRecordToUse);
                }
                else
                {
                    // Use the existing record
                    detailRecordToUse = existingDetailRecord;
                }

                int dayCounter = 1;

                // Iterate through the ItineraryDetails and add only TrekDepartureItinerary records
                if (model.ItineraryDetails != null && model.ItineraryDetails.Any())
                {
                    foreach (var detail in model.ItineraryDetails)
                    {
                        // Check if the Itinerary for this day and heading already exists
                        if (!detailRecordToUse.TrekDepartureItinerary.Any(i =>
                                i.ItineraryDay == $"Day {dayCounter}" &&
                                i.ItineraryHeading == detail.ItineraryHeading))
                        {
                            // Create a new record for TrekDepartureItinerary
                            var newItineraryRecord = new TrekDepartureItinerary
                            {
                                TrekId = trekItinerary.TrekId,
                                ItineraryDay = $"Day {dayCounter}",
                                ItineraryHeading = detail.ItineraryHeading,
                                TrekDepartureItineraryDetail = detailRecordToUse
                            };

                            // Add the itinerary record to the Detail's collection
                            detailRecordToUse.TrekDepartureItinerary.Add(newItineraryRecord);
                        }

                        dayCounter++;
                    }
                }

                // Save changes to the database
                await _context.SaveChangesAsync();
            }






            return RedirectToAction("SetItinerary", "Operation_Team");
        }






        [HttpGet]
        [Route("Edit_Itinerary_Departure")]
        public IActionResult Edit_Itinerary_Departure()
        {
            return View();
        }
        [HttpPost]
        [Route("Edit_Itinerary_Departure")]
        public IActionResult Edit_Itinerary_Departure(int? DepartureId, List<ItineraryDetailViewModel> ItineraryDetails)
        {
            if (DepartureId == null)
            {
                // Case when DepartureId is null: Fetch data from TrekItineraries table
                var trekDeparture = _context.TrekDeparture
              .Where(td => td.DepartureId == DepartureId)
              .Select(td => td.TrekId)
              .FirstOrDefault();




                var trekItineraryIds = _context.TrekItineraries
                    .Where(td => td.TrekId == trekDeparture && td.ItineraryType == "Fixed")
                    .Select(td => td.Id)
                    .ToList();
                var itineraryDetails = _context.ItineraryDetails
     .Where(i => trekItineraryIds.Contains(i.TrekItineraryId))
     .Select(i => new
     {
         i.ItineraryDay,
         i.ItineraryHeading
     })
     .ToList();

                var idsString = string.Join(",", trekItineraryIds);
                return RedirectToAction("Edit_Itinerary", new { id = idsString, departureId = DepartureId });
            }

            // Case when DepartureId is not null: Check TrekDepartureItineraryDetail table
            var trekDetails = _context.TrekDepartureItineraryDetail
                .Where(td => td.DepartureId == DepartureId)
                .Select(td => new
                {
                    td.TrekName,
                    td.TrekId
                })
                .FirstOrDefault();

            if (trekDetails == null)
            {
                // If DepartureId is not found in TrekDepartureItineraryDetail, fetch trek details
                var trekDeparture = _context.TrekDeparture
                    .Where(td => td.DepartureId == DepartureId)
                    .Select(td => td.TrekId)
                    .FirstOrDefault();

                var trekItineraryIds = _context.TrekItineraries
                    .Where(ti => ti.TrekId == trekDeparture && ti.ItineraryType == "Fixed")
                    .Select(ti => ti.Id)
                    .ToList();

                var itineraryDetails = _context.ItineraryDetails
                    .Where(i => trekItineraryIds.Contains(i.TrekItineraryId))
                    .Select(i => new
                    {
                        i.ItineraryDay,
                        i.ItineraryHeading
                    })
                    .ToList();

                var idsString = string.Join(",", trekItineraryIds);
                return RedirectToAction("Edit_Itinerary", new { id = idsString, departureId = DepartureId });
            }

            // Process itinerary details
            foreach (var detail in ItineraryDetails)
            {
                var existingDetail = _context.TrekDepartureItineraryDetail
                    .FirstOrDefault(d =>
                        d.DepartureId == DepartureId &&
                        d.TrekDepartureItinerary.Any(ti => ti.ItineraryDay == detail.ItineraryDay));

                if (existingDetail != null)
                {
                    // Update existing itinerary detail
                    existingDetail.FromDate = detail.StartDate;
                    existingDetail.ToDate = detail.EndDate;

                    var itinerary = existingDetail.TrekDepartureItinerary
                        .FirstOrDefault(ti => ti.ItineraryDay == detail.ItineraryDay);
                    if (itinerary != null)
                    {
                        itinerary.ItineraryHeading = detail.ItineraryHeading;
                    }
                }
                else
                {
                    // Add new itinerary detail
                    var newItinerary = new TrekDepartureItinerary
                    {
                        TrekId = trekDetails.TrekId,
                        ItineraryDay = detail.ItineraryDay,
                        ItineraryHeading = detail.ItineraryHeading
                    };

                    var newDetail = new TrekDepartureItineraryDetail
                    {
                        TrekId = trekDetails.TrekId,
                        TrekName = trekDetails.TrekName,
                        DepartureId = DepartureId.Value,
                        FromDate = detail.StartDate,
                        ToDate = detail.EndDate,
                        TrekDepartureItinerary = new List<TrekDepartureItinerary> { newItinerary }
                    };

                    _context.TrekDepartureItineraryDetail.Add(newDetail);
                }
            }

            // Save changes to the database
            _context.SaveChanges();

            // Fetch updated itinerary details
            var updatedItineraryDetails = _context.TrekDepartureItineraryDetail
                .Where(d => d.DepartureId == DepartureId)
                .SelectMany(d => d.TrekDepartureItinerary, (d, ti) => new ItineraryDetailViewModel
                {
                    StartDate = d.FromDate,
                    EndDate = d.ToDate,
                    ItineraryDay = ti.ItineraryDay,
                    ItineraryHeading = ti.ItineraryHeading
                })
                .ToList();

            ViewBag.TrekName = trekDetails.TrekName;
            ViewBag.TrekId = trekDetails.TrekId;
            ViewBag.TrekDepartures = _context.TrekDeparture
                .Where(td => td.TrekId == trekDetails.TrekId)
                .Select(td => new TrekDepartureViewModel
                {
                    DepartureId = td.DepartureId,
                    StartDate = td.StartDate,
                    EndDate = td.EndDate
                })
                .ToList();
            ViewBag.SelectedDepartureId = DepartureId;
            ViewBag.ItineraryDetails = updatedItineraryDetails;

            return View();
        }




        [HttpPost]
        [Route("Save_Itinerary_Departure")]
        public async Task<IActionResult> Save_Itinerary_Departure(TrekDepartureItineraryDetailViewModel model)
        {
            try
            {
                // Check if the trek departure exists
                var trekDeparture = await _context.TrekDeparture
                    .FirstOrDefaultAsync(t => t.TrekId == model.TrekId && t.DepartureId == model.DepartureId);

                if (trekDeparture == null)
                {
                    ModelState.AddModelError("", "Trek departure not found.");
                    return View("Edit_Itinerary_Departure", model);
                }

                // Prepare updated details
                var updatedDetails = model.ItineraryDetails.Select((detail, index) => new TrekDepartureItinerary
                {
                    ItineraryDay = $"Day {index + 1}",
                    ItineraryHeading = detail.ItineraryHeading,
                    TrekId = model.TrekId,
                  
                    TrekDepartureItineraryDetail = new TrekDepartureItineraryDetail
                    {
                        TrekId = model.TrekId,
                        TrekName = model.TrekName,
                        DepartureId = model.DepartureId,
                        FromDate = trekDeparture.StartDate,
                        ToDate = trekDeparture.EndDate
                    }
                }).ToList();

                // Get existing details from the database
                var existingDetails = _context.TrekDepartureItineraryDetail
                    .Where(d => d.DepartureId == model.DepartureId && d.TrekId == model.TrekId && d.TrekName == model.TrekName)
                    .Include(d => d.TrekDepartureItinerary) // Include related TrekDepartureItinerary
                    .ToList();

                // Process updated details
                foreach (var updatedDetail in updatedDetails)
                {
                    var existingDetail = existingDetails
                        .FirstOrDefault(d => d.TrekDepartureItinerary.Any(ti => ti.ItineraryDay == updatedDetail.ItineraryDay));

                    if (existingDetail != null)
                    {
                        // Update existing detail
                        existingDetail.FromDate = updatedDetail.TrekDepartureItineraryDetail.FromDate;
                        existingDetail.ToDate = updatedDetail.TrekDepartureItineraryDetail.ToDate;
                        existingDetail.TrekName = updatedDetail.TrekDepartureItineraryDetail.TrekName;

                        // Update related itinerary
                        var itinerary = existingDetail.TrekDepartureItinerary
                            .FirstOrDefault(ti => ti.ItineraryDay == updatedDetail.ItineraryDay);
                        if (itinerary != null)
                        {
                            itinerary.ItineraryHeading = updatedDetail.ItineraryHeading;
                        }

                        _context.TrekDepartureItineraryDetail.Update(existingDetail);
                    }
                    else
                    {
                        // Check if TrekId and DepartureId already exist in TrekDepartureItineraryDetail
                        var existingTrekDepartureDetail = _context.TrekDepartureItineraryDetail
                            .FirstOrDefault(d => d.TrekId == updatedDetail.TrekDepartureItineraryDetail.TrekId &&
                                                 d.DepartureId == updatedDetail.TrekDepartureItineraryDetail.DepartureId);

                        if (existingTrekDepartureDetail != null)
                        {
                            // Add new data to TrekDepartureItinerary table and associate it with the existing detail
                            var newItinerary = new TrekDepartureItinerary
                            {
                                ItineraryDay = updatedDetail.ItineraryDay,
                                TrekId=updatedDetail.TrekId,
                                ItineraryHeading = updatedDetail.ItineraryHeading,
                                TrekDepartureItineraryDetail = existingTrekDepartureDetail // Link the object, not the Id
                            };

                            _context.TrekDepartureItinerary.Add(newItinerary);
                        }

                        else
                        {
                            // Add new TrekDepartureItineraryDetail with related itineraries
                            var newDetail = updatedDetail.TrekDepartureItineraryDetail;
                            newDetail.TrekDepartureItinerary = new List<TrekDepartureItinerary>
            {
                new TrekDepartureItinerary
                {
                    ItineraryDay = updatedDetail.ItineraryDay,
                    ItineraryHeading = updatedDetail.ItineraryHeading
                }
            };

                            _context.TrekDepartureItineraryDetail.Add(newDetail);
                        }
                    }
                }

                // Remove unused details
                var detailsToRemove = existingDetails.Where(d => !updatedDetails.Any(ud =>
                    d.TrekDepartureItinerary.Any(ti => ti.ItineraryDay == ud.ItineraryDay))).ToList();
                _context.TrekDepartureItineraryDetail.RemoveRange(detailsToRemove);

                // Save changes to the database
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Itinerary details saved successfully.";

                // Fetch trek departures for the view
                var trekDepartureViewModels = _context.TrekDeparture
                    .Where(t => t.TrekId == model.TrekId)
                    .Select(t => new TrekDepartureViewModel
                    {
                        DepartureId = t.DepartureId,
                        StartDate = t.StartDate,
                        EndDate = t.EndDate
                    }).ToList();

                ViewBag.TrekDepartures = trekDepartureViewModels;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while updating the itinerary: {ex.Message}";
            }

            return RedirectToAction("SetItinerary", "Operation_Team");
        }








    }
}


