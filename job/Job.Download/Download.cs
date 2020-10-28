using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Job.Download
{
    [DisallowConcurrentExecution]
    internal class Download : IJob
    {
        private readonly ILogger<Download> _logger;
        private readonly AppSettings _config;

        public Download(ILogger<Download> logger, IOptions<AppSettings> config)
        {
            _logger = logger;
            _config = config.Value;
        }
        public Task Execute(IJobExecutionContext context)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_config.OriginDirectory);
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        string html = reader.ReadToEnd();
                    }
                }

                _logger.LogInformation($"start downloading {DateTime.Now.ToString()}");

                DirectoryInfo originDirectory = new DirectoryInfo(_config.OriginDirectory);

                FileInfo[] originDirectoryFiles = originDirectory.GetFiles("*.bak");

                foreach (FileInfo file in originDirectoryFiles)
                {
                    WebClient webClient = new WebClient();
                    webClient.DownloadFile($"{_config.OriginDirectory}/{file.Name}", $"{_config.DestDirectory}\\{file.Name}");
                }
            }
            catch (Exception e)
            {
                _logger.LogInformation($"exception {e.Message}");
            }

            return Task.CompletedTask;
        }
    }
}
