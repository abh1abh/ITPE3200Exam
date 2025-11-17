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
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get AuthUserId from JWT token
        var role = User.FindFirstValue(ClaimTypes.Role); // Specified Role when creating the JWT token
        return (role, userId);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll() // Get all clients
    {
        var (role, _) = UserContext(); // Get role from JWT token
        bool isAdmin = role == "Admin"; // Check if user is Admin
        try
        {
            var clients = await _service.GetAll(isAdmin); // Get all clients from database through service
            return Ok(clients); // Return 200 OK with clients
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ClientController] Error retrieving all clients");
            return StatusCode(500, "A problem happened while handling your request."); // Return 500 Internal Server Error
        }
    }

    [Authorize(Roles = "Admin,Client")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id) // Get client by Id
    {
        var (role, authUserId) = UserContext(); // Get role and AuthUserId from JWT token
        try
        {
             var client = await _service.GetById(id, authUserId!, role!); // Get client from database through service
            if (client == null) //Check if client exists
            {
                _logger.LogError("[ClientController] Client not found for Id {ClientId:0000}", id);
                return NotFound("Client not found"); // Return 404 if client Not Found
            }
            return Ok(client); // Return 200 OK with client
        } 
        catch (UnauthorizedAccessException)
        {
            _logger.LogWarning("[ClientController] Unauthorized access attempt for ClientId {ClientId:0000}", id);
            return Forbid(); // Return 403 Forbidden if unauthorized
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ClientController] Error retrieving client for Id {ClientId:0000}", id);
            return StatusCode(500, "A problem happened while handling your request."); // Return 500 Internal Server Error
        }
       
    } 

    [Authorize(Roles = "Client")]
    [HttpGet("me")]
    public async Task<IActionResult> GetBySelf() // Get client by AuthUserId from JWT token
    {
        var (role, authUserId) = UserContext(); // Get role and AuthUserId
        try
        {
            var client = await _service.GetByAuthUserId(authUserId!, authUserId!, role!); // Get client from database through service
            if (client == null)
            {
                _logger.LogError("[HealthcareWorkerController] Healthcare worker not found for AuthUserId {AuthUserId}", authUserId);
                return NotFound("Healthcare worker not found"); // Return 404 if client Not Found
            }
            return Ok(client);
        }
        catch (UnauthorizedAccessException) // Handles different exceptions like unauthorized users. 
        {
            _logger.LogWarning("[ClientController] Unauthorized access attempt for AuthUserId {AuthUserId}", authUserId);
            return Forbid();
        } catch (Exception ex)
        {
            _logger.LogError(ex, "[ClientController] Error retrieving client for AuthUserId {AuthUserId}", authUserId);
            return StatusCode(500, "A problem happened while handling your request."); // Return 500 Internal Server Error
        }
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> RegisterClient([FromBody] RegisterDto registerDto) // Register new client
    {
        var authUser = new AuthUser // Create AuthUser object for Auth Database
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
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "[ClientController] API failed while registering new client for {Username}", registerDto.Email);
            return StatusCode(500, "API had a problem while handling your request.");
        }
        catch (ArgumentException e)
        {
            _logger.LogWarning("[ClientController] Bad request while registering new client for {Username}: {Message}", registerDto.Email, e.Message);
            return BadRequest( new { message = e.Message });
        } catch (Exception ex)
        {
            _logger.LogError(ex, "[ClientController] Error registering new client for {Username}", registerDto.Email);
            return StatusCode(500, "A problem happened while handling your request."); // Return 500 Internal Server Error
        }

    }

    [Authorize(Roles = "Admin,Client")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id) // Delete client by Id
    {
        var (role, authUserId) = UserContext(); // Get role and AuthUserId from JWT token
        try
        {
            string? deleted = await _service.Delete(id, authUserId!, role!); // Delete client from App Database
            if(deleted == null)
            {
                _logger.LogWarning("[ClientController] Client not found for Id {Id:0000}", id);
                return NotFound("Client not found");
            }
            string authId = deleted;
            bool authDeleted = await _authService.DeleteUserAsync(authId, authUserId!, role!); //Delete client from Auth Database
            if (!authDeleted)
            {
                _logger.LogWarning("[ClientController] Client not found in Auth Database for AuthUserId {AuthUserId}", authId);
                return NotFound("Client not found in Auth Database");
            }         
            return NoContent();
        }
        // Only log unauthorized access here. Not in service.
        catch (UnauthorizedAccessException) // Handles different exceptions like unauthorized users.
        {
            _logger.LogWarning("[ClientController] Unauthorized delete attempt for ClientId {Id:0000}", id);
            return Forbid(); // Return 403 Forbidden if unauthorized
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
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto updateUserDto) // Update client information
    {
        var (role, authUserId) = UserContext();
        if (id != updateUserDto.Id)
        {
            return BadRequest( new { message = "ID mismatch" });
        }
        try
        {

            string? clientUpdate = await _service.Update(updateUserDto, authUserId!, role!); // sUpdate client in App Database
            if(clientUpdate == null)
            {
                _logger.LogWarning("[ClientController] Client not found for Id {id:0000}", id);
                return NotFound("Client not found");
            }
            string authId = clientUpdate;
            var authClientUpdate = await _authService.UpdateUserAsync(updateUserDto, authId, authUserId!, role!); // Update user in Auth Database
            if(!authClientUpdate)
            {
                _logger.LogWarning("[ClientController] Client not found in Auth Database for AuthUserId {authId}", authId);
                return NotFound("Client not found in Auth Database");
            }
            return NoContent();
        }
        catch (UnauthorizedAccessException) // Handles different exceptions like unauthorized users.
        {
            _logger.LogWarning("[ClientController] Unauthorized update attempt for ClientId {id:0000}", id);
            return Forbid(); // Return 403 Forbidden if unauthorized
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

