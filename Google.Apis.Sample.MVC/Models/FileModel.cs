using System;
using System.Collections.Generic;
using System.IO;

namespace Google.Apis.Sample.MVC.Models
{
    public class FileModel
    {
        public FileModel()
        {
            DownloadUrl = new List<string>();
        }

        public string Id { get; set; }
        public string Title { get; set; }
        public DateTime? CreatedDate { get; set; }
        public List<string> DownloadUrl { get; set; }
    }
}