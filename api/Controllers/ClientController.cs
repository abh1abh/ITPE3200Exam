using api.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using api.Services;

namespace api.Controllers;

[Authorize(Roles = "Admin, Client")]
[ApiController]
[Route("api/[controller]")]
public class ClientController : ControllerBase
{
    private readonly IClientService _service;
    private readonly ILogger<ClientController> _logger;
    private readonly IAuthService _authService;

    public ClientController(IClientService service, ILogger<ClientController> logger, IAuthService authService)
    {
        _service = service;
        _logger = logger;
        _authService = authService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var clients = await _service.GetAll();
        if (!clients.Any())
        {
            _logger.LogWarning("[ClientController] No clients found.");
            return NotFound("No clients found.");
        }       
        return Ok(clients);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var client = await _service.GetById(id);
        if (client == null)
        {
            _logger.LogError("[ClientController] Client not found for ClientId {ClientId:0000}", id);
            return NotFound("Client not found");
        }
        return Ok(client);
    }

    [HttpGet("clientauth/{authUserId}")]
    public async Task<IActionResult> GetByAuthUserId(string authUserId)
    {
        var client = await _service.GetByAuthUserId(authUserId);
        if (client == null)
        {
            _logger.LogError("[ClientController] Client not found for AuthUserId {AuthUserId}", authUserId);
            return NotFound("Client not found");
        }
        return Ok(client);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ClientDto clientDto)
    {
        try
        {
            var created = await _service.Create(clientDto);
            return CreatedAtAction(nameof(GetById), new { id = created.ClientId }, created);
        }
        catch (InvalidOperationException)
        {
            return StatusCode(500, "API had a problem while handling your request.");
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        } 
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromBody] UpdateClientDto clientDto)
    {
        int id = clientDto.ClientId;
        ClientDto? client = await _service.GetById(id);
        if (client == null)
        {
            return NotFound("Healthcare worker not found");
        }
        if (id != clientDto.ClientId)
        {
            return BadRequest("ID mismatch");
        }
        try
        {
            var existingClient = await _service.Update(id, clientDto);
            var authClientUpdate = await _authService.UpdateUserAsync(client.AuthUserId, clientDto.Email!, clientDto.Password!);
            if (!existingClient)
            {
                return NotFound("Client not found");
            }
            if (!authClientUpdate)
            {
                _logger.LogError("[ClientController] Worker update failed for AuthUserId {userId}, {@client}", client.AuthUserId, clientDto);
                return StatusCode(500, "Failed to update associated user.");
            }
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "[ClientController] Worker update failed for ClientId {id:0000}, {@client}", id, clientDto);
            return StatusCode(500, "Failed to update client.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ClientController] Client update failed for ClientId {id:0000}, {@client}", id, clientDto);
            return StatusCode(500, "A problem happened while updating the Client.");
        }
    }   

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            bool deleted = await _service.Delete(id);
            if (!deleted)
            {
                return NotFound("Client not found");
            }
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "[ClientController] Delete failed for client {Id:0000}", id);
            return StatusCode(500, "Failed to delete client.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ClientController] Unexpected error deleting client {Id:0000}", id);
            return StatusCode(500, "Unexpected error.");
        }

    }
}

