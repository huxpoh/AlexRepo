using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using Google.Apis.Auth.OAuth2.Mvc;
using Google.Apis.Drive.v2;
using Google.Apis.Sample.MVC.Models;
using Google.Apis.Services;

namespace Google.Apis.Sample.MVC.Controllers
{

    public class HomeController : Controller
    {
        private const int KB = 0x400;
        private const int DownloadChunkSize = 256 * KB;

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


        //[Authorize]
        //public async Task<ActionResult> Query(CancellationToken cancellationToken, string fileId)
        //{
        //    var result = await new AuthorizationCodeMvcApp(this, new AppAuthFlowMetadata()).
        //       AuthorizeAsync(cancellationToken);

        //    if (result.Credential == null)
        //        return new RedirectResult(result.RedirectUri);

        //    var driveService = new DriveService(new BaseClientService.Initializer
        //    {
        //        HttpClientInitializer = result.Credential,
        //        ApplicationName = "ASP.NET Google APIs MVC Sample"
        //    });

        //    var file = driveService.Files.Get(fileId).Execute();

        //    var downloader = new MediaDownloader(driveService);

        //    //using (FileStream fs = System.IO.File.Create("D:\\file2.xslx"))
        //    //{
        //    //    downloader.Download(file.SelfLink, fs);
        //    //}

        //    using (FileStream fs = System.IO.File.Create("D:\\file3.xslx"))
        //    {
        //        downloader.Download(file.ExportLinks.FirstOrDefault(x=>x.Value.Contains("sheet")).Value, fs);
        //    }

        //    return new EmptyResult();
        //}





        //[Authorize]
        //public ActionResult Filtring(string downloadurl)
        //{

        //    return new EmptyResult();
        //}
    }
}