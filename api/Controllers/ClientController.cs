using HomecareAppointmentManagement.DAL;
using HomecareAppointmentManagement.Models;
using HomecareAppointmentManagement.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomecareAppointmentManagement.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class ClientController : ControllerBase
{
    private readonly IClientRepository _clientRepository;
    private readonly ILogger<ClientController> _logger;

    public ClientController(IClientRepository clientRepository, ILogger<ClientController> logger)
    {
        _clientRepository = clientRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var clients = await _clientRepository.GetAll();
        if (clients == null || !clients.Any())
        {
            _logger.LogWarning("[ClientController] No clients found.");
            return NotFound("No clients found.");
        }

        var clientDtos = clients.Select(c => new ClientDto
        {
            ClientId = c.ClientId,
            Name = c.Name,
            Address = c.Address,
            Phone = c.Phone,
            Email = c.Email,
            AuthUserId = c.AuthUserId
        });

        return Ok(clientDtos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var client = await _clientRepository.GetClientById(id);
        if (client == null)
        {
            _logger.LogError("[ClientController] Client not found for ClientId {ClientId:0000}", id);
            return NotFound("Client not found");
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

        return Ok(clientDto);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ClientDto clientDto)
    {
        var client = new Client
        {
            Name = clientDto.Name,
            Address = clientDto.Address,
            Phone = clientDto.Phone,
            Email = clientDto.Email,
            AuthUserId = clientDto.AuthUserId
        };

        bool created = await _clientRepository.Create(client);
        if (!created)
        {
            _logger.LogError("[ClientController] Client creation failed {@client}", client);
            return StatusCode(500, "A problem happened while handling your request.");
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

        return CreatedAtAction(nameof(GetById), new { id = client.ClientId }, createdDto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ClientDto clientDto)
    {
        if (id != clientDto.ClientId)
        {
            return BadRequest("ID mismatch");
        }

        var existingClient = await _clientRepository.GetClientById(id);
        if (existingClient == null)
        {
            return NotFound("Client not found");
        }

        existingClient.Name = clientDto.Name;
        existingClient.Address = clientDto.Address;
        existingClient.Phone = clientDto.Phone;
        existingClient.Email = clientDto.Email;
        existingClient.AuthUserId = clientDto.AuthUserId;

        bool updated = await _clientRepository.Update(existingClient);
        if (!updated)
        {
            _logger.LogError("[ClientController] Client update failed for ClientId {ClientId:0000}, {@client}", id, existingClient);
            return StatusCode(500, "A problem happened while handling your request.");
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var client = await _clientRepository.GetClientById(id);
        if (client == null)
        {
            return NotFound("Client not found");
        }

        bool deleted = await _clientRepository.Delete(id);
        if (!deleted)
        {
            _logger.LogError("[ClientController] Client deletion failed for ClientId {ClientId:0000}", id);
            return StatusCode(500, "A problem happened while handling your request.");
        }

        return NoContent();
    }
}

