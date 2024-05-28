using Panel.Helpers;
using System.Diagnostics;
using System.Text.Json;
using System.Text;
using System.Net.Http;

namespace Panel.Data
{
    public class SkabenAlertDaemon
    {
        private readonly SkabenAPIClient _client;
        public SkabenAlertDaemon(IHttpClientFactory clientFactory)
        {
            _client = new SkabenAPIClient(clientFactory);
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
            Summaries = (await GetAlertsAsync()).OrderBy(s=>s.Id).ToArray();
            return Summaries;
        }

        public async Task DecreaseCurrentCounterValueAsync(int value)
        {
            var current = await GetCounterLastAsync();
            if (await _client.UpdateCurrentCounterValueAsync(current.Value - value))
                await RefreshSummariesAsync();
        }

        public async Task IncreaseCurrentCounterValueAsync(int value)
        {
            var current = await GetCounterLastAsync();
            if (await _client.UpdateCurrentCounterValueAsync(current.Value + value))
                await RefreshSummariesAsync();
        }

        public async Task PostAlertAsync(int id)
        {
            if (await _client.SetCurrentAlertAsync(id))
                await RefreshSummariesAsync();
        }

        private async Task<AlertCounter> GetCounterLastAsync()
        {
            var counter = await _client.GetCounterLastAsync();
            //обработка ошибок
            return counter;
        }

        private async Task<AlertState[]> GetAlertsAsync()
        {
            var alerts = await _client.GetAlertsAsync();
            // обработка ошибок
            return alerts;
        }
    }
}
