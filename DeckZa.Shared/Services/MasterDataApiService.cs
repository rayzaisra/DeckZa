using System.Net.Http.Json;

using DeckZa.Application.DTOs.General;
using DeckZa.Application.DTOs.MasterData;

namespace DeckZa.Shared.Services;

public class MasterDataApiService
{
    private readonly ApiService _api;

    public MasterDataApiService(ApiService api) => _api = api;

    public async Task<PaginatedResponse<MasterItemDto>> GetListAsync(string table, DataTableRequestDto request)
        => await _api.PostRawFromJsonAsync<DataTableRequestDto, PaginatedResponse<MasterItemDto>>(
               $"api/master-data/{table}/list", request)
           ?? new PaginatedResponse<MasterItemDto>();

    public async Task<List<MasterItemDto>> GetAsync(string table)
        => await _api.GetFromJsonAsync<List<MasterItemDto>>($"api/master-data/{table}", unwrap: true)
           ?? new List<MasterItemDto>();

    public async Task<BaseGeneralResponse> SaveAsync(string table, MasterItemDto item)
    {
        var response = await _api.PostAsync($"api/master-data/{table}", item);
        return await response.Content.ReadFromJsonAsync<BaseGeneralResponse>()
               ?? new BaseGeneralResponse { Success = false, Message = "Request failed." };
    }

    public async Task<BaseGeneralResponse> DeactivateAsync(string table, string id)
        => await DeleteAsync(table, id);

    public async Task<BaseGeneralResponse> DeleteAsync(string table, string id)
    {
        var response = await _api.DeleteAsync($"api/master-data/{table}/{Uri.EscapeDataString(id)}");
        return await response.Content.ReadFromJsonAsync<BaseGeneralResponse>()
               ?? new BaseGeneralResponse { Success = false, Message = "Request failed." };
    }
}
