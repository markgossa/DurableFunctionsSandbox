using System;
using System.Threading;
using System.Threading.Tasks;
using FunctionApp1.DurableFunctions.ExternalEvent.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace FunctionApp1.DurableFunctions.ExternalEvent.Orchestrators
{
    public static class ExternalEvent
    {
        [FunctionName("ExternalEvent")]
        public static async Task<bool> Run(
            [OrchestrationTrigger] DurableOrchestrationContext context, ILogger log)
        {
            // Function input comes from the request content.
            using (var timeoutCts = new CancellationTokenSource())
            {
                DateTime expiration = context.CurrentUtcDateTime.AddSeconds(300);
                Task timeoutTask = context.CreateTimer(expiration, timeoutCts.Token);

                bool authorized = false;
                for (int retryCount = 0; retryCount <= 10; retryCount++)
                {
                    Task<ExternalEventResponse> challengeResponseTask =
                        context.WaitForExternalEvent<ExternalEventResponse>("MyExternalEvent");

                    Task winner = await Task.WhenAny(challengeResponseTask, timeoutTask);
                    if (winner == challengeResponseTask)
                    {
                        log.LogInformation($"");
                        if (challengeResponseTask.Result.Input.Equals(true))
                        {
                            authorized = true;
                            log.LogInformation($"Attempt {retryCount}, Authorized: {challengeResponseTask.Result.Input}");
                            break;
                        }
                        else
                        {
                            log.LogInformation($"Attempt {retryCount}, Authorized: {challengeResponseTask.Result.Input}");
                        }
                    }
                }

                if (!timeoutTask.IsCompleted)
                {
                    // All pending timers must be complete or canceled before the function exits.
                    timeoutCts.Cancel();
                }

                return authorized;
            }
        }
    }
}