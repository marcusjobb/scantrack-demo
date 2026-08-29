using System.Text;
using System.Text.Json;
using ScanTrackNode.Models;

namespace ScanTrackNode.Services;

public class PackageForwarder
{
    private readonly NodeRegistry _registry;
    private readonly IHttpClientFactory _factory;
    private readonly ILogger<PackageForwarder> _logger;

    public PackageForwarder(NodeRegistry registry, IHttpClientFactory factory, ILogger<PackageForwarder> logger)
    {
        _registry = registry;
        _factory = factory;
        _logger = logger;
    }

    public async Task<bool> ForwardAsync(Package package, string nextCity)
    {
        var nodes = await _registry.GetNodesAsync();

        if (!nodes.TryGetValue(nextCity, out var url))
        {
            _logger.LogError("Okänd nod: {City} — finns inte i registret", nextCity);
            return false;
        }

        var json = JsonSerializer.Serialize(package);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var http = _factory.CreateClient();

        try
        {
            var response = await http.PostAsync($"{url}/paket", content);
            _logger.LogInformation(
                "Paket {Id} vidarebefordrat till {City} ({Url}) — {Status}",
                package.PackageId, nextCity, url, response.StatusCode);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fel vid vidarebefordran till {City} ({Url})", nextCity, url);
            return false;
        }
    }
}
