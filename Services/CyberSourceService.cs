using CyberSourceIntegration.Configurations;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CyberSourceIntegration.Services
{
    public class CyberSourceService : ICyberSourceService
    {
        private readonly CyberSourceSettings _settings;
        private readonly IHttpClientFactory _clientFactory;

        public CyberSourceService(IOptions<CyberSourceSettings> settings, IHttpClientFactory clientFactory)
        {
            _settings = settings.Value;
            _clientFactory = clientFactory;
        }

        public async Task<string> CreatePaymentSessionAsync()
        {
            var client = new RestClient(_settings.ApiUrl);
            var request = new RestRequest("up/v1/sessions", Method.Post);

            var payload = new
            {
                targetOrigins = new[] { "https://www.example.com" },
                allowedCardNetworks = new[] { "VISA", "MASTERCARD", "AMEX" },
                clientVersion = "v2.0"
            };

            request.AddJsonBody(payload);
            AddAuthenticationHeaders(request, payload);

            var response = await client.ExecuteAsync<RestResponse>(request);

            if (!response.IsSuccessful)
            {
                throw new Exception("Error creating payment session: " + response.Content);
            }

            if (response.Content == null)
            {
                throw new Exception("Response content is null.");
            }

            var jsonResponse = JsonConvert.DeserializeObject<dynamic>(response.Content!);
            if (jsonResponse.captureContext == null)
            {
                throw new Exception("Capture context is null.");
            }

            return jsonResponse.captureContext;
        }

        public async Task<string> ProcessPaymentAsync(string transientToken)
        {
            var client = new RestClient(_settings.ApiUrl);
            var request = new RestRequest("pts/v2/payments", Method.Post);

            var payload = new
            {
                tokenInformation = new {
                    transientTokenJwt = transientToken
                },
            };

            request.AddJsonBody(payload);

            var response = await client.ExecuteAsync<RestResponse>(request);

            if (!response.IsSuccessful)
            {
                throw new Exception("Error processing payment: " + response.Content);
            }

            if (response.Content == null)
            {
                throw new Exception("Response content is null.");
            }

            var jsonResponse = JsonConvert.DeserializeObject<dynamic>(response.Content!);
            if (jsonResponse.id == null)
            {
                throw new Exception("Transaction ID is null.");
            }

            return jsonResponse.id;
        }

        private void AddAuthenticationHeaders(RestRequest request, object payload)
        {
            var jsonPayload = JsonConvert.SerializeObject(payload);
            var date = DateTime.UtcNow.ToString("r");
            var digest = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(jsonPayload)));

            request.AddHeader("v-c-merchant-id", _settings.MerchantID);
            request.AddHeader("date", date);
            request.AddHeader("digest", $"SHA-256={digest}");

            var signature = GenerateSignature(request, jsonPayload, date, digest);
            request.AddHeader("Authorization", signature);
        }

        private string GenerateSignature(RestRequest request, string payload, string date, string digest)
        {
            var keyId = _settings.MerchantKeyID;
            var secretKey = _settings.MerchantSecretKey;
            var method = request.Method.ToString().ToLower();
            var target = request.Resource;

            var signatureComponents = new[]
            {
                $"host: {_settings.ApiUrl}",
                $"date: {date}",
                $"(request-target): {method} {target}",
                $"digest: SHA-256={digest}",
                $"v-c-merchant-id: {_settings.MerchantID}"
            };

            var signatureString = string.Join("\n", signatureComponents);
            var hmac = new HMACSHA256(Convert.FromBase64String(secretKey));
            var signatureHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(signatureString)));

            return $"keyid=\"{keyId}\", algorithm=\"hmac-sha256\", headers=\"host date (request-target) digest v-c-merchant-id\", signature=\"{signatureHash}\"";
        }
    }
}
