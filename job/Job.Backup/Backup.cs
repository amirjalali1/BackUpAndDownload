//using Ionic.Zip;
using Ionic.Zip;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Job.Backup
{
    [DisallowConcurrentExecution]
    internal class Backup : IJob
    {
        private readonly ILogger<Backup> _logger;
        private readonly AppSettings _config;

        public Backup(ILogger<Backup> logger, IOptions<AppSettings> config)
        {
            _logger = logger;
            _config = config.Value;
        }

        public Task Execute(IJobExecutionContext context)
        {
            try
            {
                _logger.LogInformation($"start zip and moving to other folder {DateTime.Now.ToString()}");

                DirectoryInfo originDirectory = new DirectoryInfo(_config.OriginDirectory);

                FileInfo[] originDirectoryFiles = originDirectory.GetFiles("*.bak");

                foreach (FileInfo file in originDirectoryFiles)
                {
                    if (file.LastAccessTime > DateTime.Now.AddDays(-15))
                    {
                        if (!Directory.Exists(_config.DestDirectory))
                        {
                            Directory.CreateDirectory(_config.DestDirectory);
                        }

                        ///zip and save bak file in destination directory
                        using (ZipFile zip = new ZipFile())
                        {
                            zip.Password = "123456!";
                            zip.AddFile($"{file}");                           
                            zip.Save($"{_config.DestDirectory}{file.Name.Replace("bak", "zip")}");
                        }

                        ///move zip file 
                        //string fileToMove = _config.OriginDirectory + fileName;
                        //string moveTo = _config.DestDirectory + fileName;
                        //if (!Directory.Exists(_config.DestDirectory))
                        //{
                        //    Directory.CreateDirectory(_config.DestDirectory);
                        //}
                        //File.Move(fileToMove, moveTo);

                        ///delete bak file
                        file.Delete();
                    }

                }
                _logger.LogInformation($"finish zip and  moving to other folder {DateTime.Now.ToString()}");

            }
            catch (Exception e)
            {
                _logger.LogInformation($"exception {e.Message}");
            }

            return Task.CompletedTask;
        }
    }
}
