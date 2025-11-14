using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using CLDV6212_POE.Models;

namespace CLDV6212_POE.Services
{
    public class FunctionsClient
    {
        private readonly HttpClient _http;
        private readonly string? _key;
        private static readonly JsonSerializerOptions _json =
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        public FunctionsClient(HttpClient http, IConfiguration cfg)
        {
            _http = http;
            _key = cfg["FunctionsKey"];
        }

        // ---------------- Customers ----------------

        // POST save (sync)
        public void SaveCustomer(object dto)
        {
            var path = "/api/customers";
            if (!string.IsNullOrWhiteSpace(_key)) path += $"?code={_key}";

            var res = _http.PostAsJsonAsync(path, dto).GetAwaiter().GetResult();
            if (!res.IsSuccessStatusCode)
            {
                var body = res.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                throw new HttpRequestException(
                    $"SaveCustomer failed {(int)res.StatusCode}: {res.ReasonPhrase}. Body: {body}");
            }
        }

        // GET list (sync) with fallback to function-name route
        public List<Customer> GetCustomers()
        {
            string BuildPath(string p) => !string.IsNullOrWhiteSpace(_key) ? $"{p}?code={_key}" : p;

            var res = _http.GetAsync(BuildPath("/api/customers")).GetAwaiter().GetResult();

            if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // Fallback if custom route not present on the deployed app
                res = _http.GetAsync(BuildPath("/api/GetCustomersFromTable")).GetAwaiter().GetResult();
            }

            if (!res.IsSuccessStatusCode)
            {
                var body = res.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                throw new HttpRequestException(
                    $"GetCustomers failed {(int)res.StatusCode}: {res.ReasonPhrase}. Body: {body}");
            }

            var dtos = res.Content.ReadFromJsonAsync<List<CustomerDto>>(_json)
                                  .GetAwaiter().GetResult()
                                  ?? new List<CustomerDto>();

            return dtos.Select(d => new Customer
            {
                FirstName = d.FirstName ?? "",
                LastName  = d.LastName  ?? "",
                Email     = d.Email     ?? ""
            }).ToList();
        }

        // ---------------- Products (Blob) ----------------

        public string UploadProductImage(IFormFile file, string productId)
        {
            using var content = new MultipartFormDataContent();
            content.Add(new StreamContent(file.OpenReadStream()), "file", file.FileName);

            var path = $"/api/products/{productId}/image";
            if (!string.IsNullOrWhiteSpace(_key)) path += $"?code={_key}";

            var res = _http.PostAsync(path, content).GetAwaiter().GetResult();
            if (!res.IsSuccessStatusCode)
            {
                var body = res.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                throw new HttpRequestException(
                    $"UploadProductImage failed {(int)res.StatusCode}: {res.ReasonPhrase}. Body: {body}");
            }

            return res.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        }

        // ---------------- Orders (Queue) ----------------

        public void EnqueueOrder(object orderDto)
        {
            var path = "/api/orders";
            if (!string.IsNullOrWhiteSpace(_key)) path += $"?code={_key}";

            var res = _http.PostAsJsonAsync(path, orderDto).GetAwaiter().GetResult();
            if (!res.IsSuccessStatusCode)
            {
                var body = res.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                throw new HttpRequestException(
                    $"EnqueueOrder failed {(int)res.StatusCode}: {res.ReasonPhrase}. Body: {body}");
            }
        }

        // ---------------- Azure Files ----------------

        public void UploadContract(IFormFile file)
        {
            using var content = new MultipartFormDataContent();
            content.Add(new StreamContent(file.OpenReadStream()), "file", file.FileName);

            var path = "/api/contracts";
            if (!string.IsNullOrWhiteSpace(_key)) path += $"?code={_key}";

            var res = _http.PostAsync(path, content).GetAwaiter().GetResult();
            if (!res.IsSuccessStatusCode)
            {
                var body = res.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                throw new HttpRequestException(
                    $"UploadContract failed {(int)res.StatusCode}: {res.ReasonPhrase}. Body: {body}");
            }
        }

        private class CustomerDto
        {
            public string? FirstName { get; set; }
            public string? LastName  { get; set; }
            public string? Email     { get; set; }
        }
    }
}
