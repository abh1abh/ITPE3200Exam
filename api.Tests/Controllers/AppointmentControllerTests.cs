using System.Security.Claims;
using api.Controllers;
using api.DTO;
using api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace api.Tests.Controllers;

public class AppointmentControllerTests
{
    private readonly Mock<IAppointmentService> _appointmentService = new();
    private readonly Mock<ILogger<AppointmentController>> _logger = new();


    private AppointmentController CreateController(ClaimsPrincipal user)
    {
        var controller = new AppointmentController(_appointmentService.Object, _logger.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
        return controller;
    }

    private static ClaimsPrincipal BuildUser(string role, string authUserId)
    {
        var identity = new ClaimsIdentity(
            authenticationType: "TestAuthType",           // makes IsAuthenticated = true
            nameType: ClaimTypes.Name,                    // optional, but fine
            roleType: ClaimTypes.Role                     // matches your JWT config
        );
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, authUserId)); // matches your token
        identity.AddClaim(new Claim(ClaimTypes.Role, role));    
        
        return new ClaimsPrincipal(identity);
    }

    [Fact]
    public async Task PositiveTestGetAll()
    {
        // Arrange
        const string role = "Admin";
        const string authUserId = "auth-abc";

        var expectedDto = new List<AppointmentViewDto>
        {
            new() { Id = 1, ClientId = 10, HealthcareWorkerId = 20, AvailableSlotId = 30, Notes = "A" }
        };

        _appointmentService.Setup(s => s.GetAll()).ReturnsAsync(expectedDto);


        var controller = CreateController(BuildUser(role, authUserId));

        var result = await controller.GetAll();

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var body = Assert.IsAssignableFrom<IEnumerable<AppointmentViewDto>>(ok.Value);
        Assert.Single(body);
        _appointmentService.Verify(s => s.GetAll(), Times.Once);

    }

    [Fact]
    public async Task PositiveTestGetAppointmentsByClientId()
    {
        // Arrange
        const string role = "Client";
        const string authUserId = "client-abc";

        var expectedDto = new List<AppointmentViewDto>
        {
            new() { Id = 1, ClientId = 10, HealthcareWorkerId = 20, AvailableSlotId = 30, Notes = "A" }
        };

        _appointmentService.Setup(s => s.GetAppointmentsByClientId(authUserId)).ReturnsAsync(expectedDto);


        var controller = CreateController(BuildUser(role, authUserId));

        var result = await controller.GetAppointmentsByClient();

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var body = Assert.IsAssignableFrom<IEnumerable<AppointmentViewDto>>(ok.Value);
        Assert.Single(body);
        _appointmentService.Verify(s => s.GetAppointmentsByClientId(authUserId), Times.Once);
    }

    [Fact]
    public async Task NegativeTestGetAppointmentsByClientIdUnauthorized()
    {
        // Arrange
        const string role = "Client";
        const string authUserId = "client-does-not-exist";

        _appointmentService
            .Setup(s => s.GetAppointmentsByClientId(authUserId))
            .ThrowsAsync(new UnauthorizedAccessException());

        var controller = CreateController(BuildUser(role, authUserId));

        // Act
        var result = await controller.GetAppointmentsByClient();

        // Assert
        Assert.IsType<ForbidResult>(result);

        _appointmentService.Verify(s => s.GetAppointmentsByClientId(authUserId), Times.Once);
    }

    [Fact]
    public async Task PositiveTestGetById()
    {
        // Arrange 
        const int appointmentId = 123;
        const string role = "Client";
        const string authUserId = "auth-abc";
        var expectedDto = new AppointmentViewDto
        {
            Id = appointmentId,
            ClientId = 10,
            HealthcareWorkerId = 20,
            AvailableSlotId = 30,
            Notes = "Some notes",
            AppointmentTasks = new List<AppointmentTaskDto>
            {
                new() { Id = 1, Description = "Task A", IsCompleted = false }
            }
        };
        _appointmentService
            .Setup(s => s.GetById(appointmentId, role, authUserId))
            .ReturnsAsync(expectedDto);

        var controller = CreateController(BuildUser(role, authUserId));

        // Act
        var result = await controller.GetById(appointmentId);

        // Assert 
        var ok = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<AppointmentViewDto>(ok.Value);
        Assert.Equal(expectedDto.Id, dto.Id);
        Assert.Equal(expectedDto.ClientId, dto.ClientId);
        Assert.Equal(expectedDto.HealthcareWorkerId, dto.HealthcareWorkerId);
        Assert.Equal(expectedDto.AvailableSlotId, dto.AvailableSlotId);
        Assert.Equal(expectedDto.Notes, dto.Notes);
        Assert.Single(dto.AppointmentTasks!);
        _appointmentService.Verify(s => s.GetById(appointmentId, role, authUserId), Times.Once);
    }


    [Fact]
    public async Task NegativeTestGetByIdNotFound()
    {
        // Arrange
        const int appointmentId = 123;
        const string role = "Client";
        const string authUserId = "client-abc";

        _appointmentService
            .Setup(s => s.GetById(appointmentId, role, authUserId))
            .ReturnsAsync((AppointmentViewDto?)null);   // Returns null 

        var controller = CreateController(BuildUser(role, authUserId));

        // Act
        var result = await controller.GetById(appointmentId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task PositiveTestCreateAppointment()
    {
        // Arrange
        const string role = "Client";
        const string authUserId = "client-abc";

        var inputDto = new AppointmentDto
        {
            Notes = "Something",
            AvailableSlotId = 55,
            AppointmentTasks = new List<AppointmentTaskDto>
            {
                new() { Description = "Task A", IsCompleted = false }
            }
        };

        // Returned by service
        var createdDto = new AppointmentDto
        {
            Id = 999,
            Notes = "Something",
            AvailableSlotId = 55
        };

        _appointmentService
            .Setup(s => s.Create(inputDto, role, authUserId))
            .ReturnsAsync(createdDto);

        var controller = CreateController(BuildUser(role, authUserId));

        // Act
        var result = await controller.Create(inputDto);


        // Assert 
        var created = Assert.IsType<CreatedAtActionResult>(result);

        // Verifying that we return a CreatedAtActionResult
        Assert.Equal(nameof(AppointmentController.GetById), created.ActionName);

        var returnedDto = Assert.IsType<AppointmentDto>(created.Value);

        Assert.Equal(999, returnedDto.Id);

        _appointmentService.Verify(s => s.Create(inputDto, role, authUserId), Times.Once);
    }


    [Fact]
    public async Task NegativeTestCreateAppointmentForbid()
    {
        // Arrange
        const string role = "Client";
        const string authUserId = "client-abc";

        var inputDto = new AppointmentDto
        {
            Notes = "Something",
            AvailableSlotId = 55,
            AppointmentTasks = new List<AppointmentTaskDto>
            {
                new() { Description = "Task A", IsCompleted = false }
            }
        };

        _appointmentService
            .Setup(s => s.Create(inputDto, role, authUserId))
            .ThrowsAsync(new UnauthorizedAccessException());

        var controller = CreateController(BuildUser(role, authUserId));

        // Act
        var result = await controller.Create(inputDto);

        // Assert 
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task NegativeTestCreateAppointmentThrowsException()
    {
        // Arrange
        const string role = "Client";
        const string authUserId = "client-abc";

        var inputDto = new AppointmentDto
        {
            Notes = "Something",
            AvailableSlotId = 55,
            AppointmentTasks = new List<AppointmentTaskDto>
            {
                new() { Description = "Task A", IsCompleted = false }
            }
        };

        _appointmentService
            .Setup(s => s.Create(inputDto, authUserId, role))
            .ThrowsAsync(new Exception("unexpected"));

        var controller = CreateController(BuildUser(role, authUserId));

        // Act
        var result = await controller.Create(inputDto);

        // Assert 
        var status = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, status.StatusCode);
    }


    [Fact]
    public async Task PositiveTestUpdateAppointment()
    {
        // Arrange
        const int appointmentId = 123;
        const string role = "Client";
        const string authUserId = "client-abc";

        var dto = new AppointmentDto
        {
            Id = appointmentId,
            Notes = "Updated",
            AvailableSlotId = 55,
            AppointmentTasks = new List<AppointmentTaskDto>
            {
                new() { Id = 1, Description = "A", IsCompleted = false }
            }
        };

        _appointmentService
            .Setup(s => s.Update(appointmentId, dto, role, authUserId))
            .ReturnsAsync(true);

        var controller = CreateController(BuildUser(role, authUserId));

        // Act
        var result = await controller.Update(appointmentId, dto);

        // Assert
        Assert.IsType<NoContentResult>(result);

        _appointmentService.Verify(
            s => s.Update(appointmentId, dto, role, authUserId),
            Times.Once
        );
    }

    [Fact]
    public async Task PositveTestDeleteAppointment()
    {
        // Arrange
        const string role = "Client";
        const string authUserId = "client-abc";
        const int appointmentId = 1;

        _appointmentService
            .Setup(s => s.Delete(appointmentId, role, authUserId))
            .ReturnsAsync(true);

        var controller = CreateController(BuildUser(role, authUserId));

        // Act
        var result = await controller.Delete(appointmentId);

        // Assert 
        Assert.IsType<NoContentResult>(result);

        _appointmentService.Verify(
            s => s.Delete(appointmentId, role, authUserId),
            Times.Once
        );
    }
    
    [Fact]
    public async Task NegativeTestDeleteAppointmentUnauthorized()
    {
        // Arrange
        const string role = "Client";
        const string authUserId = "client-abc";
        const int appointmentId = 1;

        _appointmentService
            .Setup(s => s.Delete(appointmentId, role, authUserId))
            .ThrowsAsync(new UnauthorizedAccessException());

        var controller = CreateController(BuildUser(role, authUserId));

        // Act
        var result = await controller.Delete(appointmentId);

        // Assert 
        Assert.IsType<ForbidResult>(result);

        _appointmentService.Verify(
            s => s.Delete(appointmentId, role, authUserId),
            Times.Once
        );
    }


}
