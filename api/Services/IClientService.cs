
using api.DTO;

namespace api.Services;

public interface IClientService
{
    Task<IEnumerable<ClientDto>> GetAll(bool isAdmin);
    Task<ClientDto?> GetById(int id, string authUserId, string role);
    Task<ClientDto> Create(RegisterDto dto, string authId);
    Task<string?> Update(UpdateUserDto dto, string authUserId, string role);
    Task<string?> Delete(int id, string authUserId, string role);
    Task<ClientDto?> GetByAuthUserId(string authUserId, string authId, string role);
}