using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HoboEX_ModMaker
{
    public class DeepSeekService
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public static async Task<string> TranslateAsync(string text)
        {
            var settings = LocalizationManager.Settings;
            if (string.IsNullOrEmpty(settings.AiApiKey))
                throw new Exception("API Key is missing. Please set it in Tools -> Settings.");

            var requestBody = new
            {
                model = settings.AiModel,
                messages = new[]
                {
                    new { role = "system", content = LocalizationManager.Get("AiPrompt") },
                    new { role = "user", content = text }
                },
                temperature = 0.3
            };

            var postJson = JsonSerializer.Serialize(requestBody);
            var httpContent = new StringContent(postJson, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, settings.AiApiBase.TrimEnd('/') + "/chat/completions");
            request.Headers.Add("Authorization", "Bearer " + settings.AiApiKey);
            request.Content = httpContent;

            var response = await _httpClient.SendAsync(request);
            var responseJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"API Error: {response.StatusCode}\n{responseJson}");

            using var doc = JsonDocument.Parse(responseJson);
            string aiText = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString()?.Trim();

            // Robust Sanitize: Find the first '{' and last '}'
            if (!string.IsNullOrEmpty(aiText))
            {
                int firstBrace = aiText.IndexOf('{');
                int lastBrace = aiText.LastIndexOf('}');
                if (firstBrace >= 0 && lastBrace > firstBrace)
                {
                    aiText = aiText.Substring(firstBrace, lastBrace - firstBrace + 1);
                }
            }

            return aiText;
        }
    }
}
