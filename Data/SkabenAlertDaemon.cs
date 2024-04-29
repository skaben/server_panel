using Panel.Helpers;
using System.Diagnostics;
using System.Text.Json;
using System.Text;
using System.Net.Http;

namespace Panel.Data
{
    public class SkabenAlertDaemon
    {
        private readonly HttpClient _client;
        public SkabenAlertDaemon(IHttpClientFactory clientFactory)
        {
            _client = clientFactory.CreateClient("skaben_api"); ;
            Summaries = new[] { new AlertState { Id = 1, Name = "white", IsCurrent = true } };
        }

        public AlertState[] Summaries;
        public int CurrentCounter = 0;

        public AlertState CurrentAlertState
        {
            get { return Summaries.FirstOrDefault(x => x.IsCurrent) ?? throw new Exception("Что-от пошло не так, вызывайте котиков. CurrentAlertState"); }
            set
            {
                (Summaries.FirstOrDefault(x => x.Name == value.Name) ?? throw new Exception("Что-от пошло не так, вызывайте котиков. SkabenAlertDaemon")).IsCurrent = true;
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
            var counter = await _client.GetStringAsync("alert_counter/get_latest");
            Trace.WriteLine($"alerts: {counter} success");
            return PanelHelper.JsonSerializerHelper.Deserialize<AlertCounter>(counter) ?? throw new Exception("Что-от пошло не так, вызывайте котиков. SkabenAlertDaemon");
        }

        public async Task<AlertState[]> GetAlertsAsync()
        {
            var alerts = await _client.GetStringAsync("alert_state");
            return PanelHelper.JsonSerializerHelper.Deserialize<AlertState[]>(alerts) ?? throw new Exception("Что-от пошло не так, вызывайте котиков. SkabenAlertDaemon");
        }

        public async Task<bool> DecreaseCurrentCounterValueAsync(int value)
        {
            var current = await GetCounterLastAsync();
            return await UpdateCurrentCounterValueAsync(current.Value - value);
        }

        public async Task<bool> IncreaseCurrentCounterValueAsync(int value)
        {
            var current = await GetCounterLastAsync();
            return await UpdateCurrentCounterValueAsync(current.Value + value);
        }

        private async Task<bool> UpdateCurrentCounterValueAsync(int value)
        {
            var time = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
            using StringContent jsonContent = new(PanelHelper.JsonSerializerHelper.Serialize(new
            {
                value = value,
                comment = "Изменение из админки",
                timestamp = time
            }), Encoding.UTF8, "application/json");
            using HttpResponseMessage response = await _client.PostAsync(
                $"alert_counter/",
                jsonContent);
            if (response.IsSuccessStatusCode)
                await RefreshSummariesAsync();
            return response.IsSuccessStatusCode;
        }


        public async Task<bool> PostAlertAsync(int id)
        {
            using StringContent jsonContent = new(PanelHelper.JsonSerializerHelper.Serialize(new
            {
                current = true,
            }), Encoding.UTF8, "application/json");
            using HttpResponseMessage response = await _client.PostAsync(
                $"alert_state/{id}/set_current/",
                jsonContent);
            if (response.IsSuccessStatusCode)
                await RefreshSummariesAsync();
            return response.IsSuccessStatusCode;
        }
    }
}
