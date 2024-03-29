﻿using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace BcGov.Jag.AccountManagement.Server.Services;

public static class HttpClientExtensions
{
    public static Task<HttpResponseMessage> PostODataJsonAsync<TBody>(this HttpClient client, string requestUri, TBody body)
    {
        string json = JsonSerializer.Serialize(body);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json;odata=verbose");

        return client.PostAsync(requestUri, content);
    }
}
