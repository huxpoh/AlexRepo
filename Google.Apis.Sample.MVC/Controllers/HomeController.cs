using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

using Google.Apis.Auth.OAuth2.Mvc;
using Google.Apis.Download;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using Google.Apis.Sample.MVC.Models;
using Google.Apis.Services;

namespace Google.Apis.Sample.MVC.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        // [Authorize]
        //public async Task<ActionResult> ReportAsync(CancellationToken cancellationToken)
        //{
        //    var result = await new AuthorizationCodeMvcApp(this, new AppAuthFlowMetadata()).
        //      AuthorizeAsync(cancellationToken);

        //    if (result.Credential == null)
        //        return new RedirectResult(result.RedirectUri);

        //    var driveService = new DriveService(new BaseClientService.Initializer
        //    {
        //        HttpClientInitializer = result.Credential,
        //        ApplicationName = "ASP.NET Google APIs MVC Sample"
        //    });


        //    var listReq = driveService.Files.List();
        //    listReq.Fields = "items/title,items/id,items/createdDate,items/downloadUrl,items/exportLinks";

        //    var list = await listReq.ExecuteAsync();
        //    var item =  list.Items.FirstOrDefault(x => x.ExportLinks != null && x.Title.Contains("Time Report")).ExportLinks[
        //         "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"];

        //    return View(item);
        //}

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [Authorize]
        public async Task<ActionResult> DriveAsync(CancellationToken cancellationToken)
        {
            ViewBag.Message = "Your drive page.";

            var result = await new AuthorizationCodeMvcApp(this, new AppAuthFlowMetadata()).
                AuthorizeAsync(cancellationToken);

            if (result.Credential == null)
                return new RedirectResult(result.RedirectUri);

            var driveService = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = result.Credential,
                ApplicationName = "ASP.NET Google APIs MVC Sample"
            });


            var listReq = driveService.Files.List();
            listReq.Fields = "items/title,items/id,items/createdDate,items/downloadUrl,items/exportLinks";

            var list = await listReq.ExecuteAsync();

            var items = (list.Items.Where(x =>x.ExportLinks!=null).Select(file => new FileModel
            {
                Title = file.Title,
                Id = file.Id,
                CreatedDate = file.CreatedDate,
                DownloadUrl = file.ExportLinks.Select(x=>x.Value).ToList(),
            })).OrderBy(f => f.Title).ToList();
            return View(items);
        }

        [Authorize]
        public ActionResult DownloadAsync(string title, string downloadurl)
        {
            Stream stream;
            using (var client = new WebClient())
            {
                var byteArray = client.DownloadData(downloadurl);

                stream = new MemoryStream(byteArray);
                stream.Flush();
                stream.Position = 0;
            }

            return File(stream, "application/xls", "Labels.xlsx");
        }
    }
}