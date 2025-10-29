using api.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using api.Services;

namespace api.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class ClientController : ControllerBase
{
    private readonly IClientService _service;
    private readonly ILogger<ClientController> _logger;

    public ClientController(IClientService service, ILogger<ClientController> logger)
    {
        _service = service;
        _logger = logger;
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
    public async Task<IActionResult> Update(int id, [FromBody] ClientDto clientDto)
    {
        if (id != clientDto.ClientId)
        {
            return BadRequest("ID mismatch");
        }

        try
        {
            bool updated = await _service.Update(id, clientDto);
            if (!updated)
            {
                return NotFound("Client not found");
            }
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "[ClientController] Client update failed for ClientId {ClientId:0000}, {@client}", id, clientDto);
            return StatusCode(500, "Failed to update client.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ClientController] Client update failed for ClientId {ClientId:0000}, {@client}", id, clientDto);
            return StatusCode(500, "A problem happened while updating the client.");
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

