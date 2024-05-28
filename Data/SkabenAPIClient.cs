using Panel.Helpers;
using System.Diagnostics;
using System.Text;

namespace Panel.Data
{
    public class SkabenAPIClient
    {
        private readonly HttpClient _client;
        public SkabenAPIClient(IHttpClientFactory factory) 
        {
            _client = factory.CreateClient("skaben_api"); 
            _client.Timeout = TimeSpan.FromSeconds(5);
        }

        public async Task<bool> SetCurrentAlertAsync(int id)
        {
            using StringContent jsonContent = new(PanelHelper.JsonSerializerHelper.Serialize(new
            {
                current = true,
            }), Encoding.UTF8, "application/json");
            using HttpResponseMessage response = await _client.PostAsync(
                $"alert/alert_state/{id}/set_current/",
                jsonContent);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateCurrentCounterValueAsync(int value)
        {
            var time = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
            using StringContent jsonContent = new(PanelHelper.JsonSerializerHelper.Serialize(new
            {
                value = value,
                comment = "Изменение из панели",
                timestamp = time
            }), Encoding.UTF8, "application/json");
            using HttpResponseMessage response = await _client.PostAsync(
                $"alert/alert_counter/",
                jsonContent);
            return response.IsSuccessStatusCode;
        }

        public async Task<AlertCounter> GetCounterLastAsync()
        {
            var response = await _client.GetStringAsync("alert/alert_counter/get_latest");
            return PanelHelper.JsonSerializerHelper.Deserialize<AlertCounter>(response) 
                ?? new AlertCounter { Comment = "ERROR" };
        }

        public async Task<AlertState[]> GetAlertsAsync()
        {
            var response = await _client.GetStringAsync("alert/alert_state");
            return PanelHelper.JsonSerializerHelper.Deserialize<AlertState[]>(response) ?? new AlertState[] { };
        }
    }
}
