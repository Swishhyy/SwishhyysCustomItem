using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features;
using Newtonsoft.Json;

namespace SCI.Services
{
    public class WebhookService
    {
        private static readonly HttpClient Client = new HttpClient();
        private readonly string _webhookUrl;
        private readonly bool _debug;

        public WebhookService(string webhookUrl, bool debug)
        {
            Plugin.Instance?.DebugLog("WebhookService constructor called");
            _webhookUrl = webhookUrl;
            _debug = debug;
            Plugin.Instance?.DebugLog($"WebhookService initialized with URL: {(string.IsNullOrEmpty(_webhookUrl) ? "none" : "[URL hidden for security]")}");
        }

        public async Task SendCommandUsageAsync(string commandName, string userName, string arguments, bool success)
        {
            Plugin.Instance?.DebugLog($"SendCommandUsageAsync called: command={commandName}, user={userName}, success={success}");

            // Skip if webhook URL is not configured
            if (string.IsNullOrWhiteSpace(_webhookUrl))
            {
                if (_debug)
                    Log.Debug("[WebhookService] Webhook URL is not configured. Skipping notification.");
                Plugin.Instance?.DebugLog("SendCommandUsageAsync: Webhook URL is not configured, skipping");
                return;
            }

            try
            {
                Plugin.Instance?.DebugLog("SendCommandUsageAsync: Creating webhook payload");

                // Create webhook payload with explicit types for arrays
                var payload = new
                {
                    embeds = new object[]
                    {
                        new
                        {
                            title = $"Command Used: {commandName}",
                            color = success ? 3066993 : 15158332, // Green for success, red for failure
                            fields = new object[]
                            {
                                new { name = "User", value = userName, inline = true },
                                new { name = "Status", value = success ? "Success" : "Failed", inline = true },
                                new { name = "Arguments", value = string.IsNullOrEmpty(arguments) ? "None" : arguments },
                                new { name = "Timestamp", value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
                            },
                            footer = new
                            {
                                text = "SwishhyysCustomItems"
                            }
                        }
                    }
                };

                // Convert to JSON and send
                string json = JsonConvert.SerializeObject(payload);
                Plugin.Instance?.DebugLog("SendCommandUsageAsync: Payload serialized to JSON");

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                Plugin.Instance?.DebugLog("SendCommandUsageAsync: Sending HTTP request to webhook URL");
                var response = await Client.PostAsync(_webhookUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    Plugin.Instance?.DebugLog($"SendCommandUsageAsync: Request failed with status {response.StatusCode}");
                    Plugin.Instance?.DebugLog($"SendCommandUsageAsync: Error content: {errorContent}");

                    if (_debug)
                        Log.Debug($"[WebhookService] Failed to send webhook: {response.StatusCode} - {errorContent}");
                }
                else
                {
                    Plugin.Instance?.DebugLog($"SendCommandUsageAsync: Webhook sent successfully with status {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Plugin.Instance?.DebugLog($"SendCommandUsageAsync: Exception occurred: {ex.Message}\n{ex.StackTrace}");

                if (_debug)
                    Log.Debug($"[WebhookService] Error sending webhook: {ex.Message}");
            }
        }
    }
}
