using System.Collections.Concurrent;
using System.Text.Json;
using Backend_Mboagreen.Models;

namespace Backend_Mboagreen.Services
{
    public sealed class PaymentSimulator
    {
        private readonly ConcurrentDictionary<string, string> _statuses = new();

        public JsonElement CreatePayment(PaiementRequest request, string simulatorBaseUrl)
        {
            var uuid = Guid.NewGuid().ToString("N");
            _statuses[uuid] = "PENDING";

            return JsonSerializer.SerializeToElement(new
            {
                uuid,
                status = "PENDING",
                payment_url = $"{simulatorBaseUrl}/{uuid}",
                amount = request.Amount,
                currency = request.Currency,
                description = request.Description
            });
        }

        public JsonElement GetStatus(string uuid)
        {
            var status = _statuses.TryGetValue(uuid, out var currentStatus)
                ? currentStatus
                : "NOT_FOUND";

            return JsonSerializer.SerializeToElement(new { uuid, status });
        }

        public bool SetStatus(string uuid, string status)
        {
            if (status is not ("SUCCESS" or "FAILED") || !_statuses.ContainsKey(uuid))
            {
                return false;
            }

            _statuses[uuid] = status;
            return true;
        }

        public bool Exists(string uuid) => _statuses.ContainsKey(uuid);
    }
}