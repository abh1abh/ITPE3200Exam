using api.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using api.Services;
using System.Security.Claims;


namespace api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
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

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll() // Get all clients
    {
        var (role, _) = UserContext();
        bool isAdmin = role == "Admin";
        try
        {
            var clients = await _service.GetAll(isAdmin); // Get all clients from database through service
            return Ok(clients);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ClientController] Error retrieving all clients");
            return StatusCode(500, "A problem happened while handling your request.");
        }
    }

    [Authorize(Roles = "Admin,Client")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id) // Get client by Id
    {
        var (role, authUserId) = UserContext();
        var client = await _service.GetById(id, authUserId!, role!);
        if (client == null)
        {
            _logger.LogError("[ClientController] Client not found for Id {ClientId:0000}", id);
            return NotFound("Client not found");
        }
        return Ok(client);
    }

    [Authorize(Roles = "Client")]
    [HttpGet("me")]
    public async Task<IActionResult> GetBySelf() // Get client by AuthUserId from JWT token
    {
        var (role, authUserId) = UserContext(); // Get role and AuthUserId
        try
        {
            var client = await _service.GetByAuthUserId(authUserId!, authUserId!, role!);
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

    [HttpPost("register")]
    public async Task<IActionResult> RegisterClient([FromBody] RegisterDto registerDto) // Register new client
    {
        var authUser = new AuthUser
        {
            Email = registerDto.Email,
            UserName = registerDto.Email,
        };
        try
        {
            var authCreate = await _authService.RegisterClientAsync(authUser, registerDto.Password); // Register client in Auth Database
            try
            {
                var created = await _service.Create(registerDto, authUser.Id); // Create client in App Database
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created); // Return 201 Created with location header
            }
            catch (Exception)
            {
                // Rollback Auth user creation if App Database creation fails
                await _authService.DeleteUserAsync(authUser.UserName, "Admin", "system");
                throw;
            }
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

    [Authorize(Roles = "Admin,Client")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id) // Delete client by Id
    {
        var (role, authUserId) = UserContext();
        var user = await _service.GetById(id, authUserId!, role!); //Find client in App Database
        if (user == null)
        {
            return NotFound("Client not found");
        }
        try
        {
            string authId = user.AuthUserId!;               //Get username to delete from Auth Database
            bool deleted = await _service.Delete(id, authUserId!, role!);   //Delete client from App Database
            bool authDeleted = await _authService.DeleteUserAsync(authId, authUserId!, role!); //Delete client from Auth Database
            if(!deleted || !authDeleted)
            {
                throw new InvalidOperationException("Failed to delete client.");
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
    [Authorize(Roles = "Admin,Client")]
    [HttpPut("update/{id}")]
    public async Task<IActionResult> Update([FromBody] UpdateUserDto updateUserDto) // Update client information
    {
        var (role, authUserId) = UserContext();
        int id = updateUserDto.Id;
        ClientDto? client = await _service.GetById(id, authUserId!, role!); //Find client in App Database
        string authId = client!.AuthUserId!;               //Get authId to update user in Auth Database
        if (client == null)
        {
            return NotFound("Client not found");
        }
        if (id != client.Id)
        {
            return BadRequest("ID mismatch");
        }
        try
        {
            var clientUpdate = await _service.Update(updateUserDto, authUserId!, role!); //Update client in App Database
            var authClientUpdate = await _authService.UpdateUserAsync(updateUserDto, authId, authUserId!, role!); //Update user in Auth Database
            return Ok(authClientUpdate && clientUpdate);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "[ClientController] Client update failed for ClientId {id:0000}, {@client}", id, updateUserDto);
            return StatusCode(500, "Failed to update client.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ClientController] Client update failed for ClientId {id:0000}, {@client}", id, updateUserDto);
            return StatusCode(500, "A problem happened while updating the Client.");
        }
    }
}

