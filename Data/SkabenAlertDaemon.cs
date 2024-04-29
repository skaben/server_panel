using Panel.Helpers;
using System.Diagnostics;
using System.Text.Json;
using System.Text;
using System.Net.Http;

namespace Panel.Data
{
    public class SkabenAlertDaemon
    {
        private readonly IHttpClientFactory _clientFactory;
        public SkabenAlertDaemon(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
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
            using var client = _clientFactory.CreateClient("skaben_api");
            var counter = await client.GetStringAsync("alert_counter/get_latest");
            Trace.WriteLine($"alerts: {counter} success");
            return PanelHelper.JsonSerializerHelper.Deserialize<AlertCounter>(counter) ?? throw new Exception("Что-от пошло не так, вызывайте котиков. SkabenAlertDaemon");
        }

        public async Task<AlertState[]> GetAlertsAsync()
        {
            using var client = _clientFactory.CreateClient("skaben_api");
            var alerts = await client.GetStringAsync("alert_state");
            return PanelHelper.JsonSerializerHelper.Deserialize<AlertState[]>(alerts) ?? throw new Exception("Что-от пошло не так, вызывайте котиков. SkabenAlertDaemon");
        }

        public async Task<bool> PostAlertAsync(int id)
        {
            using var client = _clientFactory.CreateClient("skaben_api");
            using StringContent jsonContent = new(PanelHelper.JsonSerializerHelper.Serialize(new
            {
                current = true,
            }),
                                                Encoding.UTF8,
                                                "application/json");
            using HttpResponseMessage response = await client.PostAsync(
                $"alert_state/{id}/set_current/",
                jsonContent);
            if (response.IsSuccessStatusCode)
                await RefreshSummariesAsync();
            return response.IsSuccessStatusCode;
        }
    }
}
