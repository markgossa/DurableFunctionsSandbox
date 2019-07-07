using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FunctionApp1.DurableFunctions.Monitor.Orchestrators
{
    public class Monitor
    {
        string Location => "London";

        bool IsClear { get; set; }

        [FunctionName("Monitor")]
        public async Task<bool> Run([OrchestrationTrigger] DurableOrchestrationContext monitorContext, ILogger log)
        {
            log.LogInformation(!monitorContext.IsReplaying
                ? $"Received monitor request" 
                : $"Retrying monitor request");


            DateTime endTime = monitorContext.CurrentUtcDateTime.AddSeconds(300);
            if (!monitorContext.IsReplaying) { log.LogInformation($"Instantiating monitor. Expires: {endTime}."); }

            while (monitorContext.CurrentUtcDateTime < endTime)
            {
                this.IsClear = await monitorContext.CallActivityAsync<bool>("GetIsClear", this.Location);

                if (this.IsClear)
                {
                    if (!monitorContext.IsReplaying) { log.LogInformation($"Detected clear weather for {this.Location}."); }
                    break;
                }
                else
                {
                    var nextCheckpoint = monitorContext.CurrentUtcDateTime.AddSeconds(10);
                    if (!monitorContext.IsReplaying) { log.LogInformation($"Next check for {this.Location} at {nextCheckpoint}."); }

                    await monitorContext.CreateTimer(nextCheckpoint, CancellationToken.None);
                }
            }

            log.LogInformation($"Monitor expiring.");

            return IsClear;
        }
    }
}
