
using api.DAL;
using api.DTO;
using api.Models;

namespace api.Services;
public class ClientService : IClientService
{
    private readonly IClientRepository _repository;
    private readonly ILogger<ClientService> _logger;

    public ClientService(IClientRepository repository, ILogger<ClientService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<ClientDto>> GetAll()
    {
        var clients = await _repository.GetAll();
        if (clients == null || !clients.Any()) return Enumerable.Empty<ClientDto>();

        var clientDtos = clients.Select(c => new ClientDto
        {
            Id = c.Id,
            Name = c.Name,
            Address = c.Address,
            Phone = c.Phone,
            Email = c.Email,
            AuthUserId = c.AuthUserId
        });
        return clientDtos;

    }

    public async Task<ClientDto?> GetByAuthUserId(string authUserId)
    {
        var client = await _repository.GetByAuthUserId(authUserId);
        if (client == null)
        {
            _logger.LogWarning("[ClientService] Client not found for AuthUserId {AuthUserId}", authUserId);
            return null;
        }

        var clientDto = new ClientDto
        {
            Id = client.Id,
            Name = client.Name,
            Address = client.Address,
            Phone = client.Phone,
            Email = client.Email,
            AuthUserId = client.AuthUserId
        };
        return clientDto;
    }

    public async Task<ClientDto?> GetById(int id)
    {
        var client = await _repository.GetClientById(id);
        if (client == null)
        {
            _logger.LogWarning("[ClientService] Client not found for Id {Id:0000}", id);
            return null;
        }

        var clientDto = new ClientDto
        {
            Id = client.Id,
            Name = client.Name,
            Address = client.Address,
            Phone = client.Phone,
            Email = client.Email,
            AuthUserId = client.AuthUserId
        };
        return clientDto;
    }
    
    public async Task<ClientDto> Create(ClientDto dto)
    {
        var client = new Client
        {
            Name = dto.Name,
            Address = dto.Address,
            Phone = dto.Phone,
            Email = dto.Email,
            AuthUserId = dto.AuthUserId
        };

        bool created = await _repository.Create(client);
        if (!created)
        {
            _logger.LogError("[ClientService] Client creation failed {@client}", client);
            throw new InvalidOperationException("Create failed");
        }

        // It's good practice to return the created object, with its new ID
        var createdDto = new ClientDto
        {
            Id = client.Id,
            Name = client.Name,
            Address = client.Address,
            Phone = client.Phone,
            Email = client.Email,
            AuthUserId = client.AuthUserId
        };

        return createdDto;
    }
    public async Task<bool> Update(UpdateUserDto userDto)
    {
        var id = userDto.Id;
        var existingClient = await _repository.GetClientById(id);
        if (existingClient == null)
        {
            return false;
        }

        existingClient.Name = userDto.Name;
        existingClient.Address = userDto.Address;
        existingClient.Phone = userDto.Phone;
        existingClient.Email = userDto.Email;

        bool updated = await _repository.Update(existingClient);
        if (!updated)
        {
            _logger.LogError("[ClientService] Update failed for HealthcareWorkerId {HealthcareWorkerId:0000}, {@worker}", id, existingClient);
            throw new InvalidOperationException($"Update operation failed for HealthcareWorkerId {id}");
        }
        return updated;
    }
    
    public async Task<bool> Delete(int id)
    {
        var client = await _repository.GetClientById(id);
        if (client is null) return false; // normal "not found"

        var ok = await _repository.Delete(id);
        if (!ok)
        {
            _logger.LogError("[ClientService] Client deletion failed for Id {Id:0000}", id);
            throw new InvalidOperationException($"Delete operation failed for client ID {id}");

        }

        return true;
    }


}