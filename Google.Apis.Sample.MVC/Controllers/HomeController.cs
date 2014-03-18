using System.Linq;
using System.Net;
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

            var result =
                await new AuthorizationCodeMvcApp(this, new AppAuthFlowMetadata()).AuthorizeAsync(cancellationToken);

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
        

        [Authorize]
        public async Task<ActionResult> Query(CancellationToken cancellationToken, string downloaduri)
        {
            var result = await new AuthorizationCodeMvcApp(this, new AppAuthFlowMetadata()).AuthorizeAsync(cancellationToken);

            using (var client = new WebClient())
            {
                client.Headers.Add("Authorization", string.Format("Bearer {0}", result.Credential.Token.AccessToken));
                client.DownloadFile(downloaduri + "&exportFormat=xlsx", Server.MapPath(string.Format("~/Temp/{0}.xlsx", result.Credential.Token.AccessToken)));
                
            }

            return new EmptyResult();
        }
    }

}