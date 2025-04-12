using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Exiled.API.Features;
using Newtonsoft.Json;

namespace SCI.Services
{
    public class WebhookService
    {
        // Static HttpClient properly configured with timeout
        private static readonly HttpClient Client = new()
        {
            Timeout = TimeSpan.FromSeconds(5)
        };

        private readonly string _webhookUrl;
        private readonly bool _debug;

        public WebhookService(string webhookUrl, bool debug)
        {
            _webhookUrl = webhookUrl;
            _debug = debug;
            Plugin.Instance?.DebugLog($"WebhookService initialized with URL: {(string.IsNullOrEmpty(_webhookUrl) ? "none" : "[URL hidden for security]")}");
        }

        /// <summary>
        /// Sends a command usage notification to Discord webhook WITHOUT blocking the main thread
        /// </summary>
        public void SendCommandUsage(string commandName, string userName, string arguments, bool success)
        {
            // Skip if webhook URL is not configured
            if (string.IsNullOrWhiteSpace(_webhookUrl))
            {
                if (_debug)
                    Log.Debug("[WebhookService] Webhook URL is not configured. Skipping notification.");
                return;
            }

            // Fire and forget - do not await or block the main thread
            Task.Run(() => SendWebhookInternalAsync(commandName, userName, arguments, success));
        }

        /// <summary>
        /// Internal method that actually sends the webhook request on a background thread
        /// </summary>
        private async Task SendWebhookInternalAsync(string commandName, string userName, string arguments, bool success)
        {
            try
            {
                Plugin.Instance?.DebugLog($"SendWebhookInternal: Processing webhook for command={commandName}, user={userName}");

                // Create webhook payload
                var payload = new
                {
                    embeds = new object[]
                    {
                        new
                        {
                            title = $"Command Used: {commandName}",
                            color = success ? 3066993 : 15158332,
                            fields = new object[]
                            {
                                new { name = "User", value = userName, inline = true },
                                new { name = "Status", value = success ? "Success" : "Failed", inline = true },
                                new { name = "Arguments", value = string.IsNullOrEmpty(arguments) ? "None" : arguments },
                                new { name = "Timestamp", value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
                            },
                            footer = new { text = "SwishhyysCustomItems" }
                        }
                    }
                };

                // Convert to JSON and send
                string json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Create a cancellation token that will cancel the request after 5 seconds
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                // Send the webhook with the cancellation token
                var response = await Client.PostAsync(_webhookUrl, content, cts.Token);

                if (!response.IsSuccessStatusCode)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    if (_debug)
                        Log.Debug($"[WebhookService] Failed to send webhook: {response.StatusCode} - {errorContent}");
                }
            }
            catch (OperationCanceledException)
            {
                // This is expected if the request times out or is cancelled
                if (_debug)
                    Log.Debug("[WebhookService] Webhook request was cancelled or timed out");
            }
            catch (Exception ex)
            {
                // Log other exceptions but don't let them crash the server
                if (_debug)
                    Log.Debug($"[WebhookService] Error sending webhook: {ex.Message}");
            }
        }

        /// <summary>
        /// Legacy method for backward compatibility
        /// </summary>
        public async Task SendCommandUsageAsync(string commandName, string userName, string arguments, bool success)
        {
            // This will run without blocking by delegating to the non-blocking implementation
            SendCommandUsage(commandName, userName, arguments, success);

            // Return a completed task to satisfy the async signature
            await Task.CompletedTask;
        }
    }
}