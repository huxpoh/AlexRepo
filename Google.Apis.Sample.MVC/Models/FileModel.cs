using System;
using System.Collections.Generic;
using System.IO;

namespace Google.Apis.Sample.MVC.Models
{
    public class FileModel
    {
        public FileModel()
        {
            ExprotList = new List<string>();
        }

        public string Id { get; set; }
        public string Title { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string DownloadUrl { get; set; }
        public List<string> ExprotList { get; set; }
    }
}