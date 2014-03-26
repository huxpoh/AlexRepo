using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.WebPages;
using Google.Apis.Auth.OAuth2.Mvc;
using Google.Apis.Auth.OAuth2.Web;
using Google.Apis.Sample.MVC.Models;
using Google.Apis.Sample.MVC.Utils;
using HtmlAgilityPack;
using LinqToExcel;

namespace Google.Apis.Sample.MVC.Controllers
{
    public static class Str
    {
        public static string ExceptChars(this string str, IEnumerable<char> toExclude)
        {
            var sb = new StringBuilder(str.Length);
            foreach (char c in str)
            {
                if (!toExclude.Contains(c))
                    sb.Append(c);
            }
            return sb.ToString();
        }
    }

    public class HomeController : Controller
    {
        private readonly GoogleService _googleService;
        private readonly AcrowireService _acrowireService;
        public Dictionary<string,int> Month { set; get; }

        public HomeController()
        {
            _googleService = new GoogleService();
            _acrowireService = new AcrowireService();
            Month = new Dictionary<string, int>()
            {
                {"Jan" , 1},
                {"Feb" , 2},
                {"Mar" , 3},
                {"Apr" , 4},
                {"May" , 5},
                {"Jun" , 6},
                {"Jul" , 7},
                {"Aug" , 8},
                {"Sep" , 9},
                {"Oct" , 10},
                {"Nov" , 11},
                {"Dec" , 12}
            };
        }

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

            var items = await _googleService.GetFileModels(result);
            return View(items);
        }

        [Authorize]
        public ActionResult TimeReport()
        {
            return View();
        }

        [Authorize]
        public ActionResult GetAcrowireReport(string login, string password)
        {
            _acrowireService.SetSessionCookie();
            _acrowireService.SetAuthCookie(login, password);
             var resultHtml =  _acrowireService.GetHtml(_acrowireService.CompareCookieToHeader());

            var html = new HtmlDocument();
            html.LoadHtml(resultHtml);
            var trNodes = html.GetElementbyId("global_time_report_records").ChildNodes.Where(x => x.Name == "tr");

            var acr = new AcrowireReportList();
            foreach (var item in trNodes)
            {
                var tdNodes = item.ChildNodes.Where(x => x.Name == "td").ToArray();
                if (tdNodes.Count() != 0)
                {
                    var location = tdNodes;
                    if (location.Count() == 7)
                    {
                        var acrRep = new AcrowireReport()
                        {
                            //Date = location[0].InnerText.ExceptChars(new[] { '\t', '\n', '\r' }),
                            Person = location[1].InnerText.ExceptChars(new[] {'\t', '\n', '\r'}),
                            Project = location[2].InnerText.ExceptChars(new[] {'\t', '\n', '\r'}),
                            Summary = location[3].InnerText.ExceptChars(new[] {'\t', '\n', '\r'}),
                            BillingStatus = location[4].InnerText.ExceptChars(new[] {'\t', '\n', '\r'}),
                            Hours = location[5].InnerText.ExceptChars(new[] {'\t', '\n', '\r'})
                        };
                        
                        var split = location[0].InnerText.ExceptChars(new[] {'\t', '\n', '\r'}).Split(' ');
                        acrRep.Date =
                            new DateTime(int.Parse(split[2]), Month[split[0]],
                                int.Parse(split[1].Substring(0, split[1].Length - 1))).ToString("yyyy-M-d");
                      acr.GridList.Add(acrRep);
                    }
                }
            }
            acr.GridList.Reverse();

            return Json(acr, JsonRequestBehavior.AllowGet);
        }


        [Authorize]
        public async Task<JsonResult> GetTimeReport(CancellationToken cancellationToken)
        {
            var result = await AuthResult(cancellationToken);

            var downloaduri = await _googleService.GetFileUri(result);


            var path = Server.MapPath(string.Format("~/Temp/{0}.xlsx", result.Credential.Token.AccessToken));
            using (var client = new WebClient())
            {
                client.Headers.Add("Authorization", string.Format("Bearer {0}", result.Credential.Token.AccessToken));
                client.DownloadFile(downloaduri, path);
            }

            var excelFile = new ExcelQueryFactory(path);
            var sheetNames = excelFile.GetWorksheetNames();
            var excel = excelFile.Worksheet(sheetNames.FirstOrDefault()).Skip(1).ToList();

            var viewModel = GetViewModel(excel);

            return Json(viewModel, JsonRequestBehavior.AllowGet);
        }

        private static TimeReportList GetViewModel(IEnumerable<Row> excel)
        {
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
                DateTime.TryParse(DateTime.UtcNow.Year + "-" + item[0], out dt);
                model.Date = !item[0].ToString().IsEmpty() ? dt.ToString("yyyy-M-d") : "";

                decimal duration;
                decimal.TryParse(item[5], out duration);
                model.Duration = duration;

                decimal overtime;
                decimal.TryParse(item[6], out overtime);
                model.Overtime = overtime;
                viewModel.GridList.Add(model);
            }
            return viewModel;
        }


        private async Task<AuthorizationCodeWebApp.AuthResult> AuthResult(CancellationToken cancellationToken)
        {
            var result = await new AuthorizationCodeMvcApp(this, new AppAuthFlowMetadata()).AuthorizeAsync(cancellationToken);

            if (result.Credential == null)
                throw new Exception("You dont have credentials.");
            return result;
        }
    }
}