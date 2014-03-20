using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.WebPages;
using Google.Apis.Auth.OAuth2.Mvc;
using Google.Apis.Auth.OAuth2.Web;
using Google.Apis.Drive.v2;
using Google.Apis.Sample.MVC.Models;
using Google.Apis.Services;
using LinqToExcel;

namespace Google.Apis.Sample.MVC.Controllers
{
    public class HomeController : Controller
    {

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [Authorize]
        public async Task<ActionResult> DriveAsync(CancellationToken cancellationToken)
        {
            ViewBag.Message = "Your drive page.";

            var result = await AuthResult(cancellationToken);

            var driveService = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = result.Credential,
                ApplicationName = "ASP.NET Google APIs MVC Sample"
            });

            var listReq = driveService.Files.List();
            listReq.Fields = "items/title,items/id,items/createdDate,items/downloadUrl,items/exportLinks";

            var list = await listReq.ExecuteAsync();

            var items = (list.Items.Where(x => x.ExportLinks != null).Select(file => new FileModel
            {
                Title = file.Title,
                Id = file.Id,
                CreatedDate = file.CreatedDate,
                ExprotList = file.ExportLinks.Select(x => x.Value).ToList(),
                DownloadUrl = file.DownloadUrl
            })).OrderBy(f => f.Title).ToList();
            return View(items);
        }


        [Authorize]
        public  ActionResult TimeReport()
        {
            return View();
        }


        [Authorize]
        public async Task<JsonResult> GetTimeReport(CancellationToken cancellationToken)
        {
            var result = await AuthResult(cancellationToken);

            var driveService = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = result.Credential,
                ApplicationName = "ASP.NET Google APIs MVC Sample"
            });

            var listReq = driveService.Files.List();
            listReq.Fields = "items/title,items/id,items/createdDate,items/downloadUrl,items/exportLinks";

            var list = await listReq.ExecuteAsync();

            var timeReport = list.Items.FirstOrDefault(x => x.ExportLinks != null && x.Title.Contains("Time Report"));
            if (timeReport == null)
            {
                throw new Exception("You dont have Time Report.");
            }
            var downloaduri = timeReport.ExportLinks.FirstOrDefault(x => x.Value.Contains("xlsx")).Value;


            var path = Server.MapPath(string.Format("~/Temp/{0}.xlsx", result.Credential.Token.AccessToken));
            using (var client = new WebClient())
            {
                client.Headers.Add("Authorization", string.Format("Bearer {0}", result.Credential.Token.AccessToken));
                client.DownloadFile(downloaduri, path);
            }

            var excelFile = new ExcelQueryFactory(path);
            var sheetNames = excelFile.GetWorksheetNames();
            var excel = excelFile.Worksheet(sheetNames.FirstOrDefault()).Skip(1).ToList();

            var viewModel = new TimeReportList();
            foreach (var item in excel)
            {
                var model = new TimeReport
                {
                    Project = item[1] ?? "",
                    Task = item[2] ?? "",
                    Type = item[3] ?? ""
                };

                DateTime dt;
                DateTime.TryParse(DateTime.UtcNow.Year +"-" +item[0],out dt);
                model.Date = !item[0].ToString().IsEmpty() ? dt.ToString("yyyy MMMM dd") :"";

                decimal duration;
                decimal.TryParse(item[5], out duration);
                model.Duration = duration;

                decimal overtime;
                decimal.TryParse(item[6], out overtime);
                model.Overtime = overtime;
                viewModel.GridList.Add(model);
            }

            return Json(viewModel,JsonRequestBehavior.AllowGet);
        }


        private async Task<AuthorizationCodeWebApp.AuthResult> AuthResult(CancellationToken cancellationToken)
        {
            var result =
                await new AuthorizationCodeMvcApp(this, new AppAuthFlowMetadata()).AuthorizeAsync(cancellationToken);

            if (result.Credential == null)
                throw new Exception("You dont have credentials.");
            return result;
        }

        //[Authorize]
        //public async Task<ActionResult> Query(CancellationToken cancellationToken, string downloaduri)
        //{
        //    var result = await new AuthorizationCodeMvcApp(this, new AppAuthFlowMetadata()).AuthorizeAsync(cancellationToken);

        //    var path =Server.MapPath( string.Format("~/Temp/{0}.xlsx", result.Credential.Token.AccessToken));
        //    using (var client = new WebClient())
        //    {
        //        client.Headers.Add("Authorization", string.Format("Bearer {0}", result.Credential.Token.AccessToken));
        //        client.DownloadFile(downloaduri + "&exportFormat=xlsx", path);
        //    }

        //    var excelFile = new ExcelQueryFactory(path);
        //    var sheetNames = excelFile.GetWorksheetNames();
        //    var excel = excelFile.Worksheet(sheetNames.FirstOrDefault()).ToList();
        //    var viewModel = new Grid {GridList = excel};


        //    return View(viewModel);
        //}
    }

    public class TimeReportList
    {
        public TimeReportList()
        {
            GridList = new List<TimeReport>();
        }
        public List<TimeReport> GridList { set; get; }
    }
}