// Elias Åsgård, 2026-10-29
using System.Globalization;

public record HistoryEntryRequest(int CharacterId, string Field, string? OldValue, string? NewValue, string? Description = null);
public record HistoryEntryDto(int Id, int CharacterId, string Field, string? OldValue, string? NewValue, string Description, DateTime TimeChanged);

public class HistoryClient(HttpClient http, ILogger<HistoryClient> log)
{
    public async Task RecordAsync(int characterId, string field, string? oldValue, string? newValue, string? description = null)
    {
        try
        {
            var resp = await http.PostAsJsonAsync("history",
                new HistoryEntryRequest(characterId, field, oldValue, newValue, description));
            resp.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            log.LogWarning(ex, "Could not record history for character {Id}", characterId);
        }
    }

    public async Task<List<HistoryEntryDto>> GetManyAsync(int[] ids, string? field, DateTime? from=null, int limit=200)
    {
        if (from is null)
            from ??= DateTime.UtcNow;
        else
            from = from.Value.ToUniversalTime() != from.Value ? from = from.Value.ToUniversalTime() : from;
        try
        {
            var query = string.Join("&", ids.Select(i => $"ids={i}"))
                      + (field is null ? "" : $"&field={Uri.EscapeDataString(field)}")
                      + $"&from={Uri.EscapeDataString(from.Value.ToString("O", CultureInfo.InvariantCulture))}"
                      + $"&limit={limit}";
            return await http.GetFromJsonAsync<List<HistoryEntryDto>>($"history?{query}") ?? [];
        }
        catch (Exception ex)
        {
            log.LogWarning(ex, "Could not read history");
            return [];
        }
    }
}