using MoznyZaklad.Server;
using System.IO;
using System.Text.Json;

public static class JsonLoader
{
    public static T NactiData<T>(string cesta)
    {
        try
        {
            string text = File.ReadAllText(cesta);
            return JsonSerializer.Deserialize<T>(text, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (Exception ex)
        {
            Logger.Log($"Chyba nacteni {cesta}: {ex.Message}");
            throw;
        }
    }
}