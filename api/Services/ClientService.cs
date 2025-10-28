
using HomecareAppointmentManagement.DAL;
using HomecareAppointmentManagement.DTO;
using HomecareAppointmentManagement.Models;

namespace HomecareAppointmentManagement.Services;
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
            ClientId = c.ClientId,
            Name = c.Name,
            Address = c.Address,
            Phone = c.Phone,
            Email = c.Email,
            AuthUserId = c.AuthUserId
        });
        return clientDtos;

    }

    public async Task<ClientDto?> GetById(int id)
    {
        var client = await _repository.GetClientById(id);
        if (client == null)
        {
            _logger.LogWarning("[ClientController] Client not found for ClientId {ClientId:0000}", id);
            return null;
        }

        var clientDto = new ClientDto
        {
            ClientId = client.ClientId,
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
            ClientId = client.ClientId,
            Name = client.Name,
            Address = client.Address,
            Phone = client.Phone,
            Email = client.Email,
            AuthUserId = client.AuthUserId
        };

        return createdDto;
    }
    public async Task<bool> Update(int id, ClientDto dto)
    {
        var existingClient = await _repository.GetClientById(id);
        if (existingClient == null)
        {
            return false;
        }

        try
        {
            existingClient.Name = dto.Name;
            existingClient.Address = dto.Address;
            existingClient.Phone = dto.Phone;
            existingClient.Email = dto.Email;
            existingClient.AuthUserId = dto.AuthUserId;

            bool updated = await _repository.Update(existingClient);
            if (!updated)
            {
                _logger.LogError("[ClientController] Client update failed for ClientId {ClientId:0000}, {@client}", id, existingClient);
                throw new Exception($"Update operation failed for client ID {id}");
            }

            return updated;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[CLientService] Error deleting client {Id:0000}", id);
            throw; // Push the real problem up to the controller 
        }

        
    }
    public async Task<bool> Delete(int id)
    {
        var client = await _repository.GetClientById(id);
        if (client == null)
        {
            return false;
        }

        try
        {
            bool deleted = await _repository.Delete(id);
            if (!deleted)
            {
                _logger.LogError("[ClientService] Deletion failed for ClientId {HealthcareWorkerId:0000}", id);
                throw new Exception($"Delete operation failed for client ID {id}");
                
            }
            return true;
        } catch (Exception ex)
        {
            _logger.LogError(ex, "[CLientService] Error deleting client {Id:0000}", id);
            throw; // Push the real problem up to the controller 
        }

 
    }

}