
using api.DTO;

namespace api.Services;
public interface IClientService
{
    Task<IEnumerable<ClientDto>> GetAll();
    Task<ClientDto?> GetById(int id);
    Task<ClientDto> Create(ClientDto dto);
    Task<bool> Update(int id, ClientDto dto);
    Task<bool> Delete(int id);

}