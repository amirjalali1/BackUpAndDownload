using System.Collections.Generic;

namespace WorkerService.Internals
{
    internal class JobSettings
    {
        public List<JobCronExpression> Jobs { get; set; }
    }
}