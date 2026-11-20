//Elias Åsgård, 2026-11-13
using CharacterWebsite.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CharacterWebsite.Clients
{
    public record ApiResult(bool Ok, IDictionary<string, string[]>? Errors = null);

    public class CharacterClient(HttpClient client)
    {
        /// <summary>
        /// Async GET: Gets all characters
        /// </summary>
        /// <returns></returns>
        public async Task<List<CharacterDto>> ListAsync()
        {
            return await client.GetFromJsonAsync<List<CharacterDto>>("characters") ?? [];
        }

        /// <summary>
        /// Async GET: gets a specified character
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<CharacterDto?> GetAsync(int id)
        {
            var resp = await client.GetAsync($"characters/{id}");
            if (resp.StatusCode == HttpStatusCode.NotFound) return null;
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<CharacterDto>();
        }

        /// <summary>
        /// Async GET: Gets all weapons
        /// </summary>
        /// <returns></returns>
        public async Task<List<WeaponDto>> WeaponsAsync()
        {
            return await client.GetFromJsonAsync<List<WeaponDto>>("weapons") ?? [];
        }

        /// <summary>
        /// Async GET: Gets all Species
        /// </summary>
        /// <returns></returns>
        public async Task<List<string>> SpeciesAsync()
        {
            return await client.GetFromJsonAsync<List<string>>("species") ?? [];
        }

        /// <summary>
        /// Async CREATE: Creates a new character
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        public async Task<ApiResult> CreateAsync(CharacterForm character)
        {
            return await ToResult(await client.PostAsJsonAsync("characters", character));
        }

        /// <summary>
        /// Async PUT: changes a specified character
        /// </summary>
        /// <param name="id"></param>
        /// <param name="character"></param>
        /// <returns></returns>
        public async Task<ApiResult> UpdateAsync(int id, CharacterForm character)
        { 
            return await ToResult(await client.PutAsJsonAsync($"characters/{id}", character));
        }

        /// <summary>
        /// Async DELETE: Deletes a specified character
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ApiResult> DeleteAsync(int id)
        {
            return await ToResult(await client.DeleteAsync($"characters/{id}"));
        }


        /// <summary>
        /// Async PUT: Changes the hp of a character
        /// </summary>
        /// <param name="id"></param>
        /// <param name="currentHp"></param>
        /// <returns></returns>
        public async Task<bool> SetHpAsync(int id, int currentHp)
        {
            try
            {
                var resp = await client.PutAsJsonAsync($"characters/{id}/hp", new { currentHp });
                return resp.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        /// <summary>
        /// Async PUT: Subtracts from the hp of a character
        /// </summary>
        /// <param name="id"></param>
        /// <param name="amount"></param>
        /// <param name="attacker"></param>
        /// <returns></returns>
        public async Task<CharacterDto?> DamageAsync(int id, int amount, string attacker)
        {
            var resp = await client.PostAsJsonAsync($"characters/{id}/damage", new { amount, attacker });
            return resp.IsSuccessStatusCode ? await resp.Content.ReadFromJsonAsync<CharacterDto>() : null;
        }

        /// <summary>
        /// Async GET: Gets a list of history entries given two character ids and date from and limit amount
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="bossId"></param>
        /// <param name="from"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public async Task<List<HistoryEntryDto>> FightLogAsync( int playerId, int bossId,
                                                                DateTime? from = null, int limit = 20)
        {
            if (from is null) from = DateTime.UtcNow.AddHours(-1);
            return await client.GetFromJsonAsync<List<HistoryEntryDto>>(    
                         $"characters/history?ids={playerId}&ids={bossId}&field=CurrentHp" +
                         $"&from={from}" +
                         $"&limit={limit}") ?? [];
        }

        /// <summary>
        /// Async GET: Gets a list of history entries given a character id and limit amount
        /// </summary>
        /// <param name="id"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public async Task<List<HistoryEntryDto>> HistoryAsync(int id, int limit = 200)
        {
            return await client.GetFromJsonAsync<List<HistoryEntryDto>>(
                        $"characters/history?ids={id}&limit={limit}") ?? [];
        }

        /// <summary>
        /// Async converter: converts a HTTP response to an ApiResult
        /// </summary>
        /// <param name="resp"></param>
        /// <returns></returns>
        private static async Task<ApiResult> ToResult(HttpResponseMessage resp)
        {
            //Return true if success
            if (resp.IsSuccessStatusCode) return new(true);
            //Return false and error if bad request
            if (resp.StatusCode == HttpStatusCode.BadRequest)
            {
                var problem = await resp.Content.ReadFromJsonAsync<ValidationProblemDetails>();
                return new(false, problem?.Errors);
            }
            resp.EnsureSuccessStatusCode();   //Throw error for anything else
            //Return false (just in case)
            return new(false);
        }
    }
}
