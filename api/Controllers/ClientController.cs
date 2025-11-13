using api.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using api.Services;
using System.Security.Claims;


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

    private (string? role, string? authUserId) UserContext() // Get role and AuthUserId from JWT token
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); 
        var role = User.FindFirstValue(ClaimTypes.Role); // Specified Role when creating the JWT token
        return (role, userId);
    }


    [HttpGet]
    public async Task<IActionResult> GetAll() // Get all clients
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
    public async Task<IActionResult> GetById(int id) // Get client by Id
    {
        var client = await _service.GetById(id);
        if (client == null)
        {
            _logger.LogError("[ClientController] Client not found for Id {ClientId:0000}", id);
            return NotFound("Client not found");
        }
        return Ok(client);
    }

    [HttpGet("clientauth")]
    public async Task<IActionResult> GetBySelf() // Get client by AuthUserId from JWT token
    {
        var (_, authUserId) = UserContext(); // Get role and AuthUserId
        try
        {
            var client = await _service.GetByAuthUserId(authUserId!);
            if (client == null)
            {
                _logger.LogError("[HealthcareWorkerController] Healthcare worker not found for AuthUserId {AuthUserId}", authUserId);
                return NotFound("Healthcare worker not found");
            }
            return Ok(client);
        }
        catch (UnauthorizedAccessException) // Handles different exceptions like unauthorized users. 
        {
            return Forbid();
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ClientDto clientDto) // Create new client
    {
        try
        {
            var created = await _service.Create(clientDto);                 // Create client in App Database
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created); // Return 201 Created with location header
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

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id) // Delete client by Id
    {
        try
        {
            ClientDto user = await _service.GetById(id); //Find client in App Database
            string username = user.Email;               //Get username to delete from Auth Database
            bool deleted = await _service.Delete(id);   //Delete client from App Database
            bool authDeleted = await _authService.DeleteUserAsync(username); //Delete client from Auth Database
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

