using Panel.Helpers;
using System.Diagnostics;
using System.Text.Json;
using System.Text;
using System.Net.Http;

namespace Panel.Data
{
    public class SkabenAlertDaemon
    {
        private readonly HttpClient _httpClient;
        public SkabenAlertDaemon(HttpClient http)
        {
            _httpClient = http;
            _httpClient.Timeout = TimeSpan.FromSeconds(5);
            _httpClient.BaseAddress = new Uri("http://127.0.0.1/api/");
            Summaries = new[] { new AlertState { Id = 1, Name = "white", IsCurrent = true } };
        }

        public AlertState[] Summaries;
        public int CurrentCounter = 0;

        public AlertState CurrentAlertState
        {
            get { return Summaries.FirstOrDefault(x => x.IsCurrent) ?? throw new Exception("Что-от пошло не так, вызывайте котиков. getCurrentAlertState"); }
            set
            {
                (Summaries.FirstOrDefault(x => x.Name == value.Name) ?? throw new Exception("Что-от пошло не так, вызывайте котиков. setCurrentAlertState")).IsCurrent = true;
            }
        }
        public async Task<AlertState[]> RefreshSummariesAsync()
        {
            CurrentCounter = (await GetCounterLastAsync()).Value;
            Summaries = await GetAlertsAsync();
            return Summaries;
        }

        public async Task<AlertCounter> GetCounterLastAsync()
        {
            var counter = await _httpClient.GetStringAsync("alert_counter/get_latest");
            Trace.WriteLine($"alerts: {counter} success");
            return PanelHelper.JsonSerializerHelper.Deserialize<AlertCounter>(counter) ?? throw new Exception("Что-от пошло не так, вызывайте котиков. SkabenAlertDaemon");
        }

        public async Task<AlertState[]> GetAlertsAsync()
        {
            Trace.WriteLine($"GetAlertsAsync");
            var alerts = await _httpClient.GetStringAsync("alert_state");
            Trace.WriteLine($"alerts: {alerts} success");
            return PanelHelper.JsonSerializerHelper.Deserialize<AlertState[]>(alerts) ?? throw new Exception("Что-от пошло не так, вызывайте котиков. SkabenAlertDaemon");
        }

        public async Task<bool> PostAlertAsync(int id)
        {
            try
            {
                using StringContent jsonContent = new(PanelHelper.JsonSerializerHelper.Serialize(new
                {
                    current = true,
                }),
                                                Encoding.UTF8,
                                                "application/json");
                using HttpResponseMessage response = await _httpClient.PostAsync(
                    $"alert_state/{id}/set_current/",
                    jsonContent);
                if (response.IsSuccessStatusCode)
                    await RefreshSummariesAsync();
                return response.IsSuccessStatusCode;
            }
            catch (Exception e)
            {

                Trace.TraceError(e.Message);
            }
            return false;
        }
    }
}
