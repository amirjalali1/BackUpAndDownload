using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using System;
using System.Collections.Generic;
using System.Data;
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
                _logger.LogInformation($"start downloading {DateTime.Now.ToString()}");

                var web = new HtmlWeb();
                var doc = web.Load(_config.OriginDirectory);
                var nodes = doc.DocumentNode.SelectNodes("//table/tbody/tr");

                var list = new List<TableDto>();

                var rows = nodes.Select(tr => tr
                    .Elements("td")
                    .Select(td => td.InnerText.Trim())                    
                    .ToArray());
                foreach (var row in rows)
                {
                    list.Add(new TableDto
                    { 
                        Name = row[0],
                        Size = row[1],
                        Modified = DateTime.Parse(row[2].Replace(" &#x2B;00:00", ""))
                    });                    
                }

                if (!Directory.Exists(_config.DestDirectory))
                {
                    Directory.CreateDirectory(_config.DestDirectory);
                }

                foreach (var item in list)
                {
                    if(item.Modified > DateTime.Now.AddDays(-15))
                    {
                        WebClient webClient = new WebClient();
                        webClient.DownloadFile($"{_config.OriginDirectory}/{item.Name}", $"{_config.DestDirectory}\\{item.Name}");
                    }
                }

                _logger.LogInformation($"finish downloading {DateTime.Now.ToString()}");
            }
            catch (Exception e)
            {
                _logger.LogInformation($"exception {e.Message}");
            }

            return Task.CompletedTask;
        }
    }

    public class TableDto 
    {
        public string Name { get; set; }
        public string Size { get; set; }
        public DateTime Modified { get; set; }
    }
}
