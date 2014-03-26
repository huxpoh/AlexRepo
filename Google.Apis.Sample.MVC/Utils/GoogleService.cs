using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2.Web;
using Google.Apis.Drive.v2;
using Google.Apis.Sample.MVC.Models;
using Google.Apis.Services;

namespace Google.Apis.Sample.MVC.Utils
{
    public class GoogleService
    {
        public async Task<string> GetFileUri(AuthorizationCodeWebApp.AuthResult result)
        {
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
                throw new Exception(
                    "You dont have Time Report./n Reason 1)You dont have xslx doc that contains Time report in name");
            }
            var downloaduri = timeReport.ExportLinks.FirstOrDefault(x => x.Value.Contains("xlsx")).Value;
            return downloaduri;
        }

        public async Task<List<FileModel>> GetFileModels(AuthorizationCodeWebApp.AuthResult result)
        {
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
            return items;
        }
    }
}