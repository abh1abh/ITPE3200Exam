using HomecareAppointmentManagement.DAL;
using HomecareAppointmentManagment.Models;
using HomecareAppointmentManagment.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomecareAppointmentManagment.Controllers;

[Authorize(Roles = "Admin")]
public class ClientController : Controller
{

    private readonly IClientRepository _clientRepository;
    private readonly ILogger<ClientController> _logger;

    public ClientController(IClientRepository clientRepository, ILogger<ClientController> logger)
    {
        _clientRepository = clientRepository;
        _logger = logger;
    }

    public async Task<IActionResult> Table() 
    {
        var clients = await _clientRepository.GetAll(); 
        if (!clients.Any())
        {
            _logger.LogError("[ClientController] client list not found while executing _clientRepository.GetAll()");
            return NotFound("Client list not found");
        }
        var clientsViewModel = new ClientViewModel(clients, "Table"); // Using "Table" view
        return View(clientsViewModel);
    }

    public async Task<IActionResult> Details(int id) 
    {
        var client = await _clientRepository.GetClientById(id); // Get client by ID
        if (client == null)
        {
            _logger.LogError("[ClientController] client not found while executing _clientRepository.GetClientById() for ClientId {ClientId:0000}", id);
            return NotFound("Client not found");
        }
        return View(client);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Client client) 
    {
        if (ModelState.IsValid) // Checks if the model is valid
        {
            bool returnOk = await _clientRepository.Create(client); // Create the client
            if (returnOk)
                return RedirectToAction(nameof(Table)); // Redirect to Table on success
        }
        _logger.LogError("[ClientController] client creation failed {@client}", client); // Log error
        return View(client);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var client = await _clientRepository.GetClientById(id); // Get client by ID
        if (client == null)
        {
            _logger.LogError("[ClientController] client not found when editing for ClientId {ClientId:0000}", id);
            return NotFound("Client not found");
        }
        return View(client);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, Client client)
    {
        if (ModelState.IsValid) // Checks if the model is valid
        {
            bool returnOk = await _clientRepository.Update(client); // Update the client
            if (returnOk)
                return RedirectToAction(nameof(Table)); // Redirect to Table on success
        }
        _logger.LogError("[ClientController] client update failed for ClientId {ClientId:0000}, {@client}", id, client);
        return View(client);
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var client = await _clientRepository.GetClientById(id); // Get client by ID
        if (client == null)
        {
            _logger.LogError("[ClientController] client not found when deleting for ClientId {ClientId:0000}", id);
            return NotFound("Client not found");
        }
        return View(client);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        bool returnOk = await _clientRepository.Delete(id); // Delete the client
        if (!returnOk) // Check if deletion was unsuccessful, log error, and return BadRequest
        {
            _logger.LogError("[ClientController] client deletion failed for ClientId {ClientId:0000}", id);
            return BadRequest("Client deletion failed");
        }
        return RedirectToAction(nameof(Table)); // Redirect to Table on success
    }
}

