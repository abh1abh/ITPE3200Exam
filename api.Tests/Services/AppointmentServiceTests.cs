using api.DAL;
using api.Models;
using api.Services;
using Moq;
using Microsoft.Extensions.Logging;
using api.DTO;

namespace api.Tests.Services;

public class AppointmentServiceTests
{
    private readonly Mock<IAppointmentRepository> _appointmentRepository = new();
    private readonly Mock<IAvailableSlotRepository> _availableSlotRepository = new();
    private readonly Mock<IClientRepository> _clientRepository = new();
    private readonly Mock<IAppointmentTaskRepository> _appointmentTaskRepository = new();
    private readonly Mock<IChangeLogRepository> _changeLogRepository = new();
    private readonly Mock<IHealthcareWorkerRepository> _healthcareWorkerRepository = new();
    private readonly Mock<ILogger<AppointmentService>> _logger = new();



    private AppointmentService CreateService() => new(
        _appointmentRepository.Object,
        _availableSlotRepository.Object,
        _clientRepository.Object,
        _appointmentTaskRepository.Object,
        _changeLogRepository.Object,
        _healthcareWorkerRepository.Object,
        _logger.Object
    );

    private static Appointment MakeSampleAppointment(int appointmentId = 1, int slotId = 1, int clientId = 1, int workerId = 1)
    {
        return new Appointment
        {
            Id = appointmentId,
            AvailableSlotId = slotId,
            ClientId = clientId,
            HealthcareWorker = new HealthcareWorker { Id = 20, Name = "B" },
            Client = new Client { Id = 10, Name = "A" },
            HealthcareWorkerId = workerId,
            Notes = "Initial notes",
            AppointmentTasks = new List<AppointmentTask>
            {
                new() { Id = 1, Description = "Task A", IsCompleted = false },
            }
        };
    }

    [Fact]
    public async Task PositiveTestGetByIdAdmin()
    {
        // Arrange 
        var appt = MakeSampleAppointment();
        _appointmentRepository.Setup(r => r.GetById(appt.Id)).ReturnsAsync(appt);

        var service = CreateService();

        // Act
        var dto = await service.GetById(appt.Id, role: "Admin", authUserId: "any");

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(appt.Id, dto!.Id);
        Assert.Equal(appt.ClientId, dto.ClientId);
        Assert.Equal(appt.HealthcareWorkerId, dto.HealthcareWorkerId);
        Assert.Equal(appt.Notes, dto.Notes);
        Assert.Equal(appt.AvailableSlotId, dto.AvailableSlotId);
        Assert.Single(dto.AppointmentTasks!);

        // Asserts if the repository methods are never called during the GetById execution
        _clientRepository.Verify(r => r.GetByAuthUserId(It.IsAny<string>()), Times.Never);
        _healthcareWorkerRepository.Verify(r => r.GetByAuthUserId(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task PositiveTestGetByIdClient()
    {
        // Arrange 
        var appt = MakeSampleAppointment();
        _appointmentRepository.Setup(r => r.GetById(appt.Id)).ReturnsAsync(appt);

        var clientAuthId = "client-auth-100";
        _clientRepository
            .Setup(r => r.GetByAuthUserId(clientAuthId))
            .ReturnsAsync(new Client { Id = appt.ClientId, AuthUserId = clientAuthId });

        var service = CreateService();

        // Act
        var dto = await service.GetById(appt.Id, role: "Client", authUserId: clientAuthId);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(appt.Id, dto!.Id);
        Assert.Equal(appt.ClientId, dto.ClientId);
        Assert.Equal(appt.HealthcareWorkerId, dto.HealthcareWorkerId);
        Assert.Equal(appt.Notes, dto.Notes);
        Assert.Equal(appt.AvailableSlotId, dto.AvailableSlotId);
        Assert.Single(dto.AppointmentTasks!);

        // Verify calls 
        _clientRepository.Verify(r => r.GetByAuthUserId(clientAuthId), Times.Once);
        _healthcareWorkerRepository.Verify(r => r.GetByAuthUserId(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task PositiveTestGetByIdWorker()
    {
        // Arrange 
        var appt = MakeSampleAppointment();
        _appointmentRepository.Setup(r => r.GetById(appt.Id)).ReturnsAsync(appt);

        var workerAuthId = "worker-auth-654";
        _healthcareWorkerRepository
            .Setup(r => r.GetByAuthUserId(workerAuthId))
            .ReturnsAsync(new HealthcareWorker { Id = appt.HealthcareWorkerId, AuthUserId = workerAuthId });

        var service = CreateService();

        // Act
        var dto = await service.GetById(appt.Id, role: "HealthcareWorker", authUserId: workerAuthId);

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(appt.Id, dto!.Id);
        Assert.Equal(appt.ClientId, dto.ClientId);
        Assert.Equal(appt.HealthcareWorkerId, dto.HealthcareWorkerId);
        Assert.Equal(appt.Notes, dto.Notes);
        Assert.Equal(appt.AvailableSlotId, dto.AvailableSlotId);
        Assert.Single(dto.AppointmentTasks!);
        
        // Verify calls 
        _healthcareWorkerRepository.Verify(r => r.GetByAuthUserId(workerAuthId), Times.Once);
        _clientRepository.Verify(r => r.GetByAuthUserId(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task NegativeTestGetByIdWorker()
    {
        // Arrange 
        var appt = MakeSampleAppointment();
        _appointmentRepository.Setup(r => r.GetById(appt.Id)).ReturnsAsync(appt);

        var unrelatedId = "unauthorized-id";

        var service = CreateService();

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
        {
            await service.GetById(appt.Id, role: "HealthcareWorker", authUserId: unrelatedId);
        });

        // Verify calls 
        _healthcareWorkerRepository.Verify(r => r.GetByAuthUserId(unrelatedId), Times.Once);
        _clientRepository.Verify(r => r.GetByAuthUserId(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task PositiveTestCreateClient()
    {
        // Arrange 
        var authUserId = "client-auth-100";
        var clientId = 100;
        var slotId = 10;
        var workerId = 777;
        var start = DateTime.UtcNow.AddHours(2);
        var end = start.AddHours(1);

        var dto = new AppointmentDto
        {
            AvailableSlotId = slotId,
            Notes = "Initial notes",
            AppointmentTasks = new List<AppointmentTaskDto>
            {
                new() { Description = "Task A", IsCompleted = false },
                new() { Description = "Task B", IsCompleted = false },
            }
        };

        // Client lookup for authUserId
        _clientRepository.Setup(r => r.GetByAuthUserId(authUserId))
            .ReturnsAsync(new Client { Id = clientId, AuthUserId = authUserId });

        // Slot lookup. Slot is free and in the future
        var slot = new AvailableSlot
        {
            Id = slotId,
            HealthcareWorkerId = workerId,
            Start = start,
            End = end,
            IsBooked = false
        };
        _availableSlotRepository.Setup(r => r.GetById(slotId))
            .ReturnsAsync(slot);

        // Capture the slot passed to Update to assert it was booked = true
        AvailableSlot? updatedSlot = null;
        _availableSlotRepository.Setup(r => r.Update(It.IsAny<AvailableSlot>()))
            .Callback<AvailableSlot>(s => updatedSlot = s)
            .ReturnsAsync(true);

        // Appointment creation. Set ID on callback so service can re-fetch
        var createdAppointmentId = 99;
        _appointmentRepository.Setup(r => r.Create(It.IsAny<Appointment>()))
            .Callback<Appointment>(a => a.Id = createdAppointmentId)
            .ReturnsAsync(true);


        // Tasks are created successfully
        _appointmentTaskRepository.Setup(r => r.Create(It.IsAny<AppointmentTask>()))
            .ReturnsAsync(true);


        // Fresh read after create (with tasks)
        _appointmentRepository.Setup(r => r.GetById(createdAppointmentId))
        .ReturnsAsync(new Appointment
        {
            Id = createdAppointmentId,
            ClientId = clientId,
            HealthcareWorkerId = workerId,
            Start = start,
            End = end,
            Notes = dto.Notes!,
            AvailableSlotId = slotId,
            AppointmentTasks = new List<AppointmentTask>
            {
                new() { Id = 1, AppointmentId = createdAppointmentId, Description = "Task A", IsCompleted = false },
                new() { Id = 2, AppointmentId = createdAppointmentId, Description = "Task B", IsCompleted = true }
            },
            ChangeLogs = new List<ChangeLog>()
        });


        var service = CreateService();

        // Act
        var result = await service.Create(dto, role: "Client", authUserId: authUserId);

        // Assert 
        // Returned DTO matches the appointment
        Assert.NotNull(result);
        Assert.Equal(createdAppointmentId, result.Id);
        Assert.Equal(clientId, result.ClientId);
        Assert.Equal(workerId, result.HealthcareWorkerId);
        Assert.Equal(slotId, result.AvailableSlotId);
        Assert.Equal(dto.Notes, result.Notes);
        Assert.NotNull(result.AppointmentTasks);
        Assert.Equal(2, result.AppointmentTasks!.Count);

        // Slot was marked as booked
        Assert.NotNull(updatedSlot);
        Assert.Equal(slotId, updatedSlot!.Id);
        Assert.True(updatedSlot.IsBooked);

        // Verify calls
        _clientRepository.Verify(r => r.GetByAuthUserId(authUserId), Times.Once);
        _availableSlotRepository.Verify(r => r.GetById(slotId), Times.Once);
        _availableSlotRepository.Verify(r => r.Update(It.IsAny<AvailableSlot>()), Times.Once);
        _appointmentRepository.Verify(r => r.Create(It.IsAny<Appointment>()), Times.Once);
        _appointmentTaskRepository.Verify(r => r.Create(It.IsAny<AppointmentTask>()), Times.Exactly(2));
        _appointmentRepository.Verify(r => r.GetById(createdAppointmentId), Times.Once);
    }

    [Fact]
    public async Task NegativeTestCreateBookedSlot()
    {
        // Arrange 
        var authUserId = "client-auth-100";
        var clientId = 100;
        var slotId = 10;
        var workerId = 777;
        var start = DateTime.UtcNow.AddHours(2);
        var end = start.AddHours(1);

        var dto = new AppointmentDto
        {
            AvailableSlotId = slotId,
            Notes = "Initial notes",
            AppointmentTasks = new List<AppointmentTaskDto>
            {
                new() { Description = "Task A", IsCompleted = false },
                new() { Description = "Task B", IsCompleted = false },
            }
        };

        // Client lookup for authUserId
        _clientRepository.Setup(r => r.GetByAuthUserId(authUserId))
            .ReturnsAsync(new Client { Id = clientId, AuthUserId = authUserId });

        // Slot lookup. Slot is already booked
        var slot = new AvailableSlot
        {
            Id = slotId,
            HealthcareWorkerId = workerId,
            Start = start,
            End = end,
            IsBooked = true
        };
        _availableSlotRepository.Setup(r => r.GetById(slotId))
            .ReturnsAsync(slot);


        var service = CreateService();

        // Act
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                service.Create(dto, role: "Client", authUserId: authUserId));

        // Assert 
        // Check if exception contains message
        Assert.Contains("That slot is no longer available.", ex.Message, StringComparison.OrdinalIgnoreCase);

        // Nothing should proceed after validation fails
        _clientRepository.Verify(r => r.GetByAuthUserId(authUserId), Times.Once);
        _availableSlotRepository.Verify(r => r.GetById(slotId), Times.Once);
        _availableSlotRepository.Verify(r => r.Update(It.IsAny<AvailableSlot>()), Times.Never);
        _appointmentRepository.Verify(r => r.Create(It.IsAny<Appointment>()), Times.Never);
        _appointmentTaskRepository.Verify(r => r.Create(It.IsAny<AppointmentTask>()), Times.Never);
    }

    [Fact]
    public async Task NegativeTestCreateUnauthorized()
    {
        // Arrange 
        var authUserId = "unauthorized-id";
        var slotId = 10;

        var dto = new AppointmentDto
        {
            AvailableSlotId = slotId,
            Notes = "Initial notes",
            AppointmentTasks = new List<AppointmentTaskDto>
            {
                new() { Description = "Task A", IsCompleted = false },
                new() { Description = "Task B", IsCompleted = false },
            }
        };
        
        // Client lookup for authUserId returns null
        _clientRepository
            .Setup(r => r.GetByAuthUserId(authUserId))
            .ReturnsAsync((Client?)null);
        
        var service = CreateService();

        // Act and Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                service.Create(dto, role: "Client", authUserId: authUserId));

        // Verify calls
        _clientRepository.Verify(r => r.GetByAuthUserId(authUserId), Times.Once);
        _availableSlotRepository.Verify(r => r.GetById(slotId), Times.Never);
        _availableSlotRepository.Verify(r => r.Update(It.IsAny<AvailableSlot>()), Times.Never);
        _appointmentRepository.Verify(r => r.Create(It.IsAny<Appointment>()), Times.Never);
        _appointmentTaskRepository.Verify(r => r.Create(It.IsAny<AppointmentTask>()), Times.Never);
    }

    [Fact]
    public async Task NegativeTestCreateFailed()
    {
        // Arrange 
        var authUserId = "client-auth-100";
        var clientId = 100;
        var slotId = 10;
        var workerId = 777;
        var start = DateTime.UtcNow.AddHours(2);
        var end = start.AddHours(1);

        var dto = new AppointmentDto
        {
            AvailableSlotId = slotId,
            Notes = "Initial notes",
            AppointmentTasks = new List<AppointmentTaskDto>
            {
                new() { Description = "Task A", IsCompleted = false },
                new() { Description = "Task B", IsCompleted = false },
            }
        };

        // Client lookup for authUserId
        _clientRepository.Setup(r => r.GetByAuthUserId(authUserId))
            .ReturnsAsync(new Client { Id = clientId, AuthUserId = authUserId });

        // Slot lookup. Slot is free and in the future
        var slot = new AvailableSlot
        {
            Id = slotId,
            HealthcareWorkerId = workerId,
            Start = start,
            End = end,
            IsBooked = false
        };
        _availableSlotRepository.Setup(r => r.GetById(slotId))
            .ReturnsAsync(slot);

        // Capture the IsBooked states passed into Update()
        var bookedStates = new List<bool>(); // Idea came from AI. We can verify both states with a list of bools
        _availableSlotRepository.Setup(r => r.Update(It.IsAny<AvailableSlot>()))
            .Callback<AvailableSlot>(s => bookedStates.Add(s.IsBooked))
            .ReturnsAsync(true);


        // Force appointment creation to fail -> service should catch and unbook
        _appointmentRepository.Setup(r => r.Create(It.IsAny<Appointment>()))
            .ReturnsAsync(false);

        var service = CreateService();

        // Act
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.Create(dto, role: "Client", authUserId: authUserId));

        // Assert 
        // Capture the IsBooked states passed into Update() // Idea came from AI. We can verify both states with a list of bools  
        Assert.Contains("Could not create appointment", ex.Message, StringComparison.OrdinalIgnoreCase);


        // We expect two updates: first to book (true), then to unbook (false) on failure
        Assert.Equal(2, bookedStates.Count);
        Assert.True(bookedStates[0]);   // booked before create
        Assert.False(bookedStates[1]);  // unbooked after failure

        // Assert no tasks should be created
        _clientRepository.Verify(r => r.GetByAuthUserId(authUserId), Times.Once);
        _availableSlotRepository.Verify(r => r.GetById(slotId), Times.Once);
        _availableSlotRepository.Verify(r => r.Update(It.IsAny<AvailableSlot>()), Times.Exactly(2));
        _appointmentRepository.Verify(r => r.Create(It.IsAny<Appointment>()), Times.Once);
        _appointmentTaskRepository.Verify(r => r.Create(It.IsAny<AppointmentTask>()), Times.Never);
    }

    [Fact]
    public async Task PositiveTestUpdate()
    {
        // Arrange 
        var appointmentId = 1;
        var authUserId = "client-auth-100";
        var clientId = 100;
        var slotId = 10;
        var workerId = 777;

        // Create existing appointment
        var appointment = new Appointment
        {
            Id = appointmentId,
            AvailableSlotId = slotId,
            ClientId = clientId,
            HealthcareWorkerId = workerId,
            Notes = "Initial notes",
            AppointmentTasks = new List<AppointmentTask>
            {
                new() { Id = 1, Description = "Task A", IsCompleted = false },
            }
        };
        _appointmentRepository.Setup(r => r.GetById(appointment.Id)).ReturnsAsync(appointment);

        // Incoming DTO with updated values
        var dto = new AppointmentDto
        {
            Id = appointmentId,
            AvailableSlotId = slotId,
            ClientId = clientId,
            HealthcareWorkerId = workerId,
            Notes = "New Notes",
            AppointmentTasks = new List<AppointmentTaskDto>
            {
                new() { Id = 1, Description = "Task A Edited", IsCompleted = false },
                new() { Description = "New Task Z", IsCompleted = false },

            }
        };

        // Client lookup for authUserId
        _clientRepository.Setup(r => r.GetByAuthUserId(authUserId))
            .ReturnsAsync(new Client { Id = clientId, AuthUserId = authUserId });

        // Repo updates succeed
        _appointmentTaskRepository.Setup(r => r.Update(It.IsAny<AppointmentTask>())).ReturnsAsync(true);
        _appointmentTaskRepository.Setup(r => r.Create(It.IsAny<AppointmentTask>())).ReturnsAsync(true);
        _appointmentRepository.Setup(r => r.Update(It.IsAny<Appointment>())).ReturnsAsync(true);

        // Capture the changelog to assert contents
        ChangeLog? capturedLog = null;
        _changeLogRepository.Setup(r => r.Create(It.IsAny<ChangeLog>()))
            .Callback<ChangeLog>(cl => capturedLog = cl)
            .ReturnsAsync(true);

        var service = CreateService();

        // Act
        var ok = await service.Update(id: appointmentId, appointmentDto: dto, role: "Client", authUserId: authUserId);

        // Assert 
        Assert.True(ok);

        // Assert. Notes should be updated
        Assert.Equal("New Notes", appointment.Notes);

        // Assert task update (existing task id=1) and task creation (the new one without id)
        _appointmentTaskRepository.Verify(r => r.Update(It.Is<AppointmentTask>(t =>
            t.Id == 1 && t.Description == "Task A Edited" && t.IsCompleted == false
        )), Times.Once);

        _appointmentTaskRepository.Verify(r => r.Create(It.Is<AppointmentTask>(t =>
            t.Id == 0 && t.AppointmentId == appointmentId && t.Description == "New Task Z" && t.IsCompleted == false
        )), Times.Once);

        // Assert appointment itself updated
        _appointmentRepository.Verify(r => r.Update(It.Is<Appointment>(a =>
            a.Id == appointmentId && a.Notes == "New Notes"
        )), Times.Once);

        // Change log created with expected fields and descriptive text
        Assert.NotNull(capturedLog);
        Assert.Equal(appointmentId, capturedLog!.AppointmentId);
        Assert.Equal(authUserId, capturedLog.ChangedByUserId);
        Assert.False(string.IsNullOrWhiteSpace(capturedLog.ChangeDescription));
        Assert.Contains("Notes:", capturedLog.ChangeDescription);
        Assert.Contains("Task #1:", capturedLog.ChangeDescription);
        Assert.Contains("Task + \"New Task Z\" (new)", capturedLog.ChangeDescription);

        // Assert that Changelog Create ran once
        _changeLogRepository.Verify(r => r.Create(It.IsAny<ChangeLog>()), Times.Once);
    }

    [Fact]
    public async Task NegativeTestUpdateAppointmentNotExist()
    {
        // Arrange 
        var appointmentId = 1;
        var authUserId = "client-auth-100";
        var clientId = 100;
        var slotId = 10;
        var workerId = 777;

        // Return false when trying to fetch appointment
        _appointmentRepository.Setup(r => r.GetById(It.IsAny<int>())).ReturnsAsync((Appointment?)null);

        // Incoming DTO with updated values
        var dto = new AppointmentDto
        {
            Id = appointmentId,
            AvailableSlotId = slotId,
            ClientId = clientId,
            HealthcareWorkerId = workerId,
            Notes = "New Notes",
            AppointmentTasks = new List<AppointmentTaskDto>
            {
                new() { Id = 1, Description = "Task A Edited", IsCompleted = false },
                new() { Description = "New Task Z", IsCompleted = false },

            }
        };

        var service = CreateService();

        // Act
        var ok = await service.Update(id: appointmentId, appointmentDto: dto, role: "Client", authUserId: authUserId);

        // Assert 
        Assert.False(ok);

        // Verify calls
        _appointmentRepository.Verify(r => r.GetById(It.IsAny<int>()), Times.Once);
        _clientRepository.Verify(r => r.GetByAuthUserId(authUserId), Times.Never);
        _appointmentRepository.Verify(r => r.Update(It.IsAny<Appointment>()), Times.Never);
        _appointmentTaskRepository.Verify(r => r.Update(It.IsAny<AppointmentTask>()), Times.Never);
    }

    [Fact]
    public async Task NegativeTestUpdateUnauthorized()
    {
        // Arrange 
        var appointmentId = 1;
        var authUserId = "unauthorized-id";
        var clientId = 100;
        var slotId = 10;
        var workerId = 777;

        // Create existing appointment
        var appointment = new Appointment
        {
            Id = appointmentId,
            AvailableSlotId = slotId,
            ClientId = clientId,
            HealthcareWorkerId = workerId,
            Notes = "Initial notes",
            AppointmentTasks = new List<AppointmentTask>
            {
                new() { Id = 1, Description = "Task A", IsCompleted = false },
            }
        };
        _appointmentRepository.Setup(r => r.GetById(appointment.Id)).ReturnsAsync(appointment);

        // Incoming DTO with updated values
        var dto = new AppointmentDto
        {
            Id = appointmentId,
            AvailableSlotId = slotId,
            ClientId = clientId,
            HealthcareWorkerId = workerId,
            Notes = "New Notes",
            AppointmentTasks = new List<AppointmentTaskDto>
            {
                new() { Id = 1, Description = "Task A Edited", IsCompleted = false },
                new() { Description = "New Task Z", IsCompleted = false },

            }
        };

        var service = CreateService();

        // Act and Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                service.Update(id: appointmentId, appointmentDto: dto, role: "HealthcareWorker", authUserId: authUserId));

        // Verify calls 
        _appointmentRepository.Verify(r => r.GetById(It.IsAny<int>()), Times.Once);
        _healthcareWorkerRepository.Verify(r => r.GetByAuthUserId(authUserId), Times.Once);
        _clientRepository.Verify(r => r.GetByAuthUserId(It.IsAny<string>()), Times.Never);
        _appointmentRepository.Verify(r => r.Update(It.IsAny<Appointment>()), Times.Never);
        _appointmentTaskRepository.Verify(r => r.Update(It.IsAny<AppointmentTask>()), Times.Never);
    }

    [Fact]
    public async Task PositiveTestDelete()
    {
        // Arrange 
        var appointmentId = 1;
        var authUserId = "client-auth-100";
        var clientId = 100;
        var slotId = 10;
        var workerId = 777;
        var start = DateTime.UtcNow.AddHours(2);
        var end = start.AddHours(1);

        // Create existing appointment
        var appointment = new Appointment
        {
            Id = appointmentId,
            AvailableSlotId = slotId,
            ClientId = clientId,
            HealthcareWorkerId = workerId,
            Notes = "Initial notes",
            AppointmentTasks = new List<AppointmentTask>
            {
                new() { Id = 1, Description = "Task A", IsCompleted = false },
            }
        };
        _appointmentRepository.Setup(r => r.GetById(appointment.Id)).ReturnsAsync(appointment);

        // Auth for Client role
        _clientRepository.Setup(r => r.GetByAuthUserId(authUserId))
            .ReturnsAsync(new Client { Id = clientId, AuthUserId = authUserId });

        // Slot lookup
        var slot = new AvailableSlot
        {
            Id = slotId,
            HealthcareWorkerId = workerId,
            Start = start,
            End = end,
            IsBooked = true
        };
        _availableSlotRepository.Setup(r => r.GetById(slotId)).ReturnsAsync(slot);

        // Update from booked to unbooked
        AvailableSlot? updatedSlot = null;
        _availableSlotRepository.Setup(r => r.Update(It.IsAny<AvailableSlot>()))
            .Callback<AvailableSlot>(s => updatedSlot = s)
            .ReturnsAsync(true);

        // Capture the changelog to assert contents
        ChangeLog? capturedLog = null;
        _changeLogRepository.Setup(r => r.Create(It.IsAny<ChangeLog>()))
            .Callback<ChangeLog>(cl => capturedLog = cl)
            .ReturnsAsync(true);

        // Delete return true
        _appointmentRepository.Setup(r => r.Delete(appointmentId)).ReturnsAsync(true);

        var service = CreateService();

        // Act 
        var ok = await service.Delete(id: appointmentId, role: "Client", authUserId: authUserId);

        // Assert
        Assert.True(ok);

        // Assert that available slot is not booked
        Assert.Equal(slotId, updatedSlot!.Id);
        Assert.False(updatedSlot!.IsBooked);


        // Assert that Changelog is created properly
        Assert.NotNull(capturedLog);
        Assert.Equal(appointmentId, capturedLog!.AppointmentIdSnapshot);
        Assert.Equal(authUserId, capturedLog.ChangedByUserId);
        Assert.Contains("Appointment deleted.", capturedLog.ChangeDescription);

        // Verify calls
        _appointmentRepository.Verify(r => r.GetById(appointmentId), Times.Once);
        _clientRepository.Verify(r => r.GetByAuthUserId(authUserId), Times.Once);
        _availableSlotRepository.Verify(r => r.GetById(slotId), Times.Once);
        _availableSlotRepository.Verify(r => r.Update(It.IsAny<AvailableSlot>()), Times.Once);
        _changeLogRepository.Verify(r => r.Create(It.IsAny<ChangeLog>()), Times.Once);
        _appointmentRepository.Verify(r => r.Delete(appointmentId), Times.Once);
    }

    [Fact]
    public async Task NegativeTestDeleteUnauthorized()
    {
        // Arrange 
        var appointmentId = 1;
        var authUserId = "unauthorized-id";
        var clientId = 100;
        var slotId = 10;
        var workerId = 777;

        // Create existing appointment
        var appointment = new Appointment
        {
            Id = appointmentId,
            AvailableSlotId = slotId,
            ClientId = clientId,
            HealthcareWorkerId = workerId,
            Notes = "Initial notes",
            AppointmentTasks = new List<AppointmentTask>
            {
                new() { Id = 1, Description = "Task A", IsCompleted = false },
            }
        };
        _appointmentRepository.Setup(r => r.GetById(appointment.Id)).ReturnsAsync(appointment);


        var service = CreateService();

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.Delete(id: appointmentId, role: "HealthcareWorker", authUserId: authUserId));

        // Verify calls
        _appointmentRepository.Verify(r => r.GetById(appointmentId), Times.Once);
        _healthcareWorkerRepository.Verify(r => r.GetByAuthUserId(authUserId), Times.Once);
        _availableSlotRepository.Verify(r => r.GetById(slotId), Times.Never);
        _availableSlotRepository.Verify(r => r.Update(It.IsAny<AvailableSlot>()), Times.Never);
        _changeLogRepository.Verify(r => r.Create(It.IsAny<ChangeLog>()), Times.Never);
        _appointmentRepository.Verify(r => r.Delete(appointmentId), Times.Never);
    }

    [Fact]
    public async Task NegativeTestDeleteFail()
    {
        // Arrange 
        var appointmentId = 1;
        var authUserId = "client-auth-100";
        var clientId = 100;
        var slotId = 10;
        var workerId = 777;
        var start = DateTime.UtcNow.AddHours(2);
        var end = start.AddHours(1);

        // Create existing appointment
        var appointment = new Appointment
        {
            Id = appointmentId,
            AvailableSlotId = slotId,
            ClientId = clientId,
            HealthcareWorkerId = workerId,
            Notes = "Initial notes",
            AppointmentTasks = new List<AppointmentTask>
            {
                new() { Id = 1, Description = "Task A", IsCompleted = false },
            }
        };
        _appointmentRepository.Setup(r => r.GetById(appointment.Id)).ReturnsAsync(appointment);

        // Auth for Client role
        _clientRepository.Setup(r => r.GetByAuthUserId(authUserId))
            .ReturnsAsync(new Client { Id = clientId, AuthUserId = authUserId });    

        // Delete return false
        _appointmentRepository.Setup(r => r.Delete(appointmentId)).ReturnsAsync(false);

        var service = CreateService();

        // Act
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.Delete(id: appointmentId, role: "Client", authUserId: authUserId));

        // Assert
        Assert.Contains("Appointment deletion failed.", ex.Message, StringComparison.OrdinalIgnoreCase);

        // Verify calls
        _appointmentRepository.Verify(r => r.GetById(appointmentId), Times.Once);
        _clientRepository.Verify(r => r.GetByAuthUserId(authUserId), Times.Once);
        _appointmentRepository.Verify(r => r.Delete(appointmentId), Times.Once);
    }


}
