
using System.Diagnostics.Eventing.Reader;
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
    private bool IsAuthorized(Client client, string? authUserId, string? role)
    {
        if (string.IsNullOrEmpty(authUserId))
        {
            _logger.LogWarning("[ClientService] Authorization failed: AuthUserId is null or empty.");
            return false;
        } 
        var ok = false;

        if (role == "Admin") return true;

        else if (role == "Client" && client.AuthUserId == authUserId)
        {
            ok = true;
        }
        return ok;
    }
    public async Task<IEnumerable<ClientDto>> GetAll(bool isAdmin) // Get all clients
    {
        if (!isAdmin) // If not admin, return empty
        {
            _logger.LogWarning("[ClientService] Unauthorized access attempt to get all clients by non-admin user.");
            return Enumerable.Empty<ClientDto>();
        }
        var clients = await _repository.GetAll();
        if (clients == null || !clients.Any())
        {
            _logger.LogWarning("[ClientService] No clients found.");
            return Enumerable.Empty<ClientDto>();
        } 

        var clientDtos = clients.Select(c => new ClientDto
        {
            Id = c.Id,
            Name = c.Name,
            Address = c.Address,
            Phone = c.Phone,
            Email = c.Email,
            AuthUserId = c.AuthUserId
        });
        return clientDtos; // return empty enumerable if no clients found

    }

    public async Task<ClientDto?> GetByAuthUserId(string authUserId, string authId, string role) // Get client by AuthUserId
    {   
        var client = await _repository.GetByAuthUserId(authUserId); // Get client from repository
        if (client == null)
        {
            _logger.LogWarning("[ClientService] Client not found for AuthUserId {AuthUserId}", authUserId);
            return null;
        }
        if(!IsAuthorized(client, authId, role))
        {
            _logger.LogWarning("[ClientService] Unauthorized access attempt by AuthUserId {AuthUserId} to delete ClientId {ClientId:0000}", authId, client.Id);
            throw new UnauthorizedAccessException("You are not authorized to delete this client.");
        }

        var clientDto = new ClientDto // Map Client to ClientDto
        {
            Id = client.Id,
            Name = client.Name,
            Address = client.Address,
            Phone = client.Phone,
            Email = client.Email,
            AuthUserId = client.AuthUserId
        };
        return clientDto; // return empty if not found
    }

    //add authentication
    public async Task<ClientDto?> GetById(int id, string authUserId, string role) // Get client by Id
    {
        var client = await _repository.GetClientById(id); // Get client from repository
        if (client == null)
        {
            _logger.LogWarning("[ClientService] Client not found for Id {Id:0000}", id);
            return null;
        }
        if(!IsAuthorized(client, authUserId, role))
        {
            _logger.LogWarning("[ClientService] Unauthorized access attempt by AuthUserId {AuthUserId} to get ClientId {ClientId:0000}", authUserId, id);
            throw new UnauthorizedAccessException("You are not authorized to get this client.");
        }

        var clientDto = new ClientDto // Map Client to ClientDto
        {
            Id = client.Id,
            Name = client.Name,
            Address = client.Address,
            Phone = client.Phone,
            Email = client.Email,
            AuthUserId = client.AuthUserId
        };
        return clientDto; // return empty if not found
    }

    public async Task<ClientDto> Create(RegisterDto dto, string authId) // Create new client
    {
        var client = new Client // Map ClientDto to Client
        {
            Name = dto.Name,
            Address = dto.Address,
            Phone = dto.Phone,
            Email = dto.Email,
            AuthUserId = authId
        };

        bool created = await _repository.Create(client); // Create client in repository
        if (!created)
        {
            _logger.LogWarning("[ClientService] Client creation failed {@client}", client);
            throw new InvalidOperationException("Create failed");
        }

        // It's good practice to return the created object, with its new ID
        var createdDto = new ClientDto // Map Client to ClientDto
        {
            Id = client.Id,
            Name = client.Name,
            Address = client.Address,
            Phone = client.Phone,
            Email = client.Email,
            AuthUserId = client.AuthUserId
        };

        return createdDto; // return created client dto
    }

    //Add authorization
    public async Task<bool> Update(UpdateUserDto userDto, string authId, string role) // Update existing client
    {
        var id = userDto.Id; // Get client Id from dto
        var existingClient = await _repository.GetClientById(id); // Get existing client from repository
        if (existingClient == null)
        {
            return false;
        }
        if(!IsAuthorized(existingClient, authId, role))
        {
            _logger.LogWarning("[ClientService] Unauthorized access attempt by AuthUserId {AuthUserId} to delete ClientId {ClientId:0000}", authId, id);
            throw new UnauthorizedAccessException("You are not authorized to delete this client.");
        }

        existingClient.Name = userDto.Name; // Update client properties
        existingClient.Address = userDto.Address;
        existingClient.Phone = userDto.Phone;
        existingClient.Email = userDto.Email;

        bool updated = await _repository.Update(existingClient); // Update client in repository
        if (!updated)
        {
            _logger.LogWarning("[ClientService] Update failed for HealthcareWorkerId {HealthcareWorkerId:0000}, {@worker}", id, existingClient);
            throw new InvalidOperationException($"Update operation failed for HealthcareWorkerId {id}");
        }
        return updated; // return true if updated
    }

    //add authorization
    public async Task<bool> Delete(int id, string authId, string role) // Delete client by Id
    {
        var client = await _repository.GetClientById(id); // Get client from repository
        if (client is null) return false; // normal "not found"
        if(!IsAuthorized(client, authId, role))
        {
            _logger.LogWarning("[ClientService] Unauthorized access attempt by AuthUserId {AuthUserId} to delete ClientId {ClientId:0000}", authId, id);
            throw new UnauthorizedAccessException("You are not authorized to delete this client.");
        }
        var ok = await _repository.Delete(id); // Delete client in repository
        if (!ok)
        {
            _logger.LogWarning("[ClientService] Client deletion failed for Id {Id:0000}", id);
            throw new InvalidOperationException($"Delete operation failed for client ID {id}");

        }
        return true;
    }
}