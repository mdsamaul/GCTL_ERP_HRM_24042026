using GCTL.Service.ManageNoticeEntries;
using GCTL.UI.Core.ViewModels.ManageNotices;
using GCTL.Core.ViewModels.ManageNoticeEntries;
using Microsoft.AspNetCore.Mvc;
using GCTL.Core.Helpers;
using GCTL.Core.Data;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace GCTL.UI.Core.Controllers
{
    public class ManageNoticeEntryController : BaseController
    {
        private readonly IManageNoticeService entryService;
        private readonly IRepository<NoticeDocumentFile> noticeRepo;

        public ManageNoticeEntryController(IManageNoticeService entryService, IRepository<NoticeDocumentFile> noticeRepo)
        {
            this.entryService = entryService;
            this.noticeRepo = noticeRepo;
        }


        public IActionResult Index()
        {
            ManageNoticePageViewModel model = new ManageNoticePageViewModel()
            {
                PageUrl = Url.Action(nameof(Index)),
            };

            return View(model);
        }

        public async Task<IActionResult> getFilterEmp([FromBody] EmployeeFilterViewModel model)
        {
            var data = await entryService.GetFilterEmpAsync(model);
            return Json(data);
        }

        public async Task<IActionResult> GenerateNewId()
        {
            var newId = await entryService.GenerateIdAsync();
            return Json(newId);
        }

        public async Task<IActionResult> GetById(int id)
        {
            var result = await entryService.GetByIdAsync(id);
            return Json(new { data = result });
        }

        public async Task<ActionResult> GetPaginatedData()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            var sortColumnIndex = Request.Form["order[0][column]"].FirstOrDefault();
            var sortColumn = Request.Form[$"columns[{sortColumnIndex}][data]"].FirstOrDefault();
            var sortDirection = Request.Form["order[0][dir]"].FirstOrDefault();

            var pageSize = string.IsNullOrEmpty(length) ? 10 : Convert.ToInt32(length);
            var page = string.IsNullOrEmpty(start) ? 1 : (Convert.ToInt32(start) / pageSize) + 1;

            var result = await entryService.GetPaginatedDataAsync(searchValue, page, pageSize, sortColumn, sortDirection);

            var response = new
            {
                draw = draw,
                recordsTotal = result.TotalRecords,
                recordsFiltered = result.FilterRecords,
                data = result.Data
            };

            return Ok(response);
        }
        //[HttpGet]
        //[Route("NoticeDocument/{fileName}")]
        //public async Task<IActionResult> GetNoticeDocument(string fileName)
        //{
        //    try
        //    {
        //        var noticeDoc = await noticeRepo.All()
        //            .FirstOrDefaultAsync(x => x.ImgType == fileName);

        //        if (noticeDoc?.Photo == null)
        //            return NotFound();

        //        // Get file extension to determine content type
        //        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        //        var contentType = extension switch
        //        {
        //            ".pdf" => "application/pdf",
        //            ".jpg" or ".jpeg" => "image/jpeg",
        //            ".png" => "image/png",
        //            ".gif" => "image/gif",
        //            ".doc" => "application/msword",
        //            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        //            ".txt" => "text/plain",
        //            _ => "application/octet-stream"
        //        };

        //        // Set Content-Disposition to inline for browser display
        //        Response.Headers.Add("Content-Disposition", $"inline; filename=\"{fileName}\"");

        //        // Optional: Add cache headers for better performance
        //        Response.Headers.Add("Cache-Control", "public, max-age=3600");

        //        return File(noticeDoc.Photo, contentType);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error serving file: {ex.Message}");
        //        return NotFound();
        //    }
        //}
        //[HttpGet]
        //public async Task<IActionResult> GetNoticeFile(decimal documentTc)
        //{
        //    try
        //    {
        //        var fileRecord = await noticeRepo.All()
        //            .Where(x => x.Tc == documentTc).FirstOrDefaultAsync();

        //        if (fileRecord == null)
        //        {
        //            return NotFound();
        //        }

        //        string fileExtension = GetFileExtension(fileRecord.ImgType);
        //        string filename = $"notice_document_{documentTc}{fileExtension}";

        //        return File(fileRecord.Photo, fileRecord.ImgType, filename);
        //    }
        //    catch (Exception ex)
        //    {

        //        throw;
        //    }
        //}

        //private string GetFileExtension(string contentType)
        //{
        //    return contentType?.ToLower() switch
        //    {
        //        "image/jpeg" => ".jpg",
        //        "image/png" => ".png",
        //        "image/bmp" => ".bmp",
        //        "image/webp" => ".webp",
        //        _ => ""
        //    };
        //}
        public async Task<IActionResult> SaveNotice([FromForm] ManageNoticeSetupViewModel model)
        {
            if (model.Tc == 0)
            {
                model.ToAudit(LoginInfo);
                var result = await entryService.SaveAsync(model);
                return Json(new
                {
                    success = result,
                    message = "Saved Successfully"
                });
            }
            else
            {
                model.ToAudit(LoginInfo, true);
                var result = await entryService.EditAsync(model);
                return Json(new
                {
                    success = result,
                    message = "Updated Successfully"
                });
            }
        }

        public async Task<IActionResult> BulkDelete([FromBody] ManageNoticeSetupViewModel model)
        {
            try
            {
                if (model.Tcs == null || !model.Tcs.Any() || model.Tcs.Count == 0)
                {
                    return Json(new { isSuccess = false, message = "No Notice is Selected" });
                }

                var result = await entryService.BulkDeleteAsync(model.Tcs);

                if (!result)
                {
                    return Json(new { isSuccess = false, message = "No notice is found to delete" });
                }

                return Json(new { isSuccess = true, message = $"Deleted {model.Tcs.Count} Notice Successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> SendNoticeToEmployee([FromBody] EmailSentParamDTO request)
        {
            if (request.EmployeeIds.Count == 0 || request.Tcs.Count == 0)
                return Json(new { isSuccess = false, message = "Employee IDs and Notice IDs are required" });

            var result = await entryService.SentNoticeToEmployeeAsync(request.EmployeeIds, request.Tcs);

            if (result)
                return Json(new { isSuccess = true, message = "Notice sent successfully" });
            else
                return Json(new { isSuccess = false, message = "Failed to send notices" });

        }
    }
}
