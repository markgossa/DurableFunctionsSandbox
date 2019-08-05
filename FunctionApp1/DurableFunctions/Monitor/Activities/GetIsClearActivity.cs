using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Queue;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System;

namespace FunctionApp1.DurableFunctions.Monitor.Activities
{
    public class GetIsClearActivity
    {
        [FunctionName("GetIsClear")]
        public async Task<bool> GetIsClear([ActivityTrigger] string location,
            ILogger log)
        {
            log.LogInformation($"Checking weather for {location}.");

            var random = new Random();
            var result = random.Next(100);

            return (result > 75);
        }
    }
}
