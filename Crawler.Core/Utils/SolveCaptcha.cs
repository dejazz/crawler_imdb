using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Crawler_Data_Lawer.Crawler.Core.Utils
{
    public class AntiCaptchaSolver
    {
        private const string ApiUrl = "https://api.anti-captcha.com/createTask";
        private const string GetTaskResultUrl = "https://api.anti-captcha.com/getTaskResult";
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;

        public AntiCaptchaSolver(string apiKey)
        {
            _apiKey = apiKey;
            _httpClient = new HttpClient();
        }

        public async Task<string?> SolveCaptchaFromUrlAsync(string imageUrl)
        {
            try
            {
                string base64Image = await DownloadImageAsBase64(imageUrl);
                return await SolveImageCaptchaAsync(base64Image);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao resolver captcha: {ex.Message}");
                return null;
            }
        }

        private async Task<string> DownloadImageAsBase64(string imageUrl)
        {
            var imageBytes = await _httpClient.GetByteArrayAsync(imageUrl);
            return Convert.ToBase64String(imageBytes);
        }

        private async Task<string?> SolveImageCaptchaAsync(string base64Image)
        {
            var taskData = new
            {
                clientKey = _apiKey,
                task = new
                {
                    type = "ImageToTextTask",
                    body = base64Image
                }
            };

            var taskResponse = await SendPostRequest(ApiUrl, taskData);
            if (taskResponse == null || !taskResponse.ContainsKey("taskId"))
                throw new Exception("Erro ao criar a tarefa no AntiCaptcha.");

            int taskId = taskResponse["taskId"]!.Value<int>();
            return await WaitForCaptchaResult(taskId);
        }

        private async Task<string?> WaitForCaptchaResult(int taskId)
        {
            while (true)
            {
                var resultData = new { clientKey = _apiKey, taskId = taskId };
                var resultResponse = await SendPostRequest(GetTaskResultUrl, resultData);

                if (resultResponse == null)
                    throw new Exception("Erro ao obter resultado do AntiCaptcha.");

                string status = resultResponse["status"]!.ToString();
                if (status == "ready")
                    return resultResponse["solution"]?["text"]?.ToString();

                await Task.Delay(3000);
            }
        }

        private async Task<JObject?> SendPostRequest(string url, object data)
        {
            var jsonContent = new StringContent(
                Newtonsoft.Json.JsonConvert.SerializeObject(data),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync(url, jsonContent);
            var responseString = await response.Content.ReadAsStringAsync();
            return JObject.Parse(responseString);
        }
    }
}
