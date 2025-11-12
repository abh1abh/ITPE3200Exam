import React, { useState } from "react";
import { Button, Form, InputGroup, Spinner } from "react-bootstrap";
import { useNavigate } from "react-router-dom";
import { Appointment, AppointmentTask } from "../types/appointment";
import { Client } from "../types/client";
import { AvailableSlot } from "../types/availableSlot";
import Loading from "../shared/Loading";

// Props for CreateAppointmentForm
interface CreateAppointmentFormProps {
  onAppointmentChanged: (newAppointment: Appointment) => void;
  clients?: Client[];
  unbookedSlots: AvailableSlot[];
  isAdmin: boolean;
  submitError?: string | null;
  isSubmitting?: boolean;
}

const CreateAppointmentForm: React.FC<CreateAppointmentFormProps> = ({
  onAppointmentChanged,
  clients = [],
  unbookedSlots,
  isAdmin,
  submitError = null,
  isSubmitting,
}) => {
  // Notes, healthcareWorkerId, tasks, availableSlotId, start, end state
  const [notes, setNotes] = useState<string>("");
  const [healthcareWorkerId, setHealthcareWorkerId] = useState<number>(0);
  const [appointmentTasks, setAppointmentTasks] = useState<AppointmentTask[]>([]);
  const [taskInput, setTaskInput] = useState<string>("");
  const [availableSlotId, setAvailableSlotId] = useState<number>(0);
  const [start, setStart] = useState<string>("");
  const [end, setEnd] = useState<string>("");

  // Client id state for admin creating appointment for a client
  const [clientId, setClientId] = useState<number>(0);

  // Error state
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();
  const onCancel = () => navigate(-1);

  // Handle available slot change
  const handleSlotChange = (slotId: number) => {
    // Set available slot id
    setAvailableSlotId(slotId);
    // Find selected slot from unbooked slots
    const slot = unbookedSlots.find((s) => s.id === slotId);
    if (!slot) return;
    // Set start, end and healthcare worker id based on selected slot
    setStart(slot.start);
    setEnd(slot.end);
    // Set healthcare worker id if present
    if (slot.healthcareWorkerId) setHealthcareWorkerId(slot.healthcareWorkerId);
  };

  // Add task to appointment tasks
  const addTask = () => {
    // Trim input and check if not empty
    const trimmed = taskInput.trim();
    // Return if empty
    if (!trimmed) return;
    // Add new task to appointment tasks and reset input
    setAppointmentTasks((prev) => [...prev, { description: trimmed, isCompleted: false }]);
    setTaskInput("");
  };

  // Remove task by index
  const removeTask = (index: number) => {
    // If no tasks, return
    if (appointmentTasks.length < 1) return;
    // Remove task at index
    setAppointmentTasks((prev) => prev.filter((_, i) => i !== index));
  };

  // Handle form submission
  const handleSubmit = (event: React.FormEvent) => {
    // Prevent default form submission
    event.preventDefault();

    // If start time is in the past, show error
    if (new Date(start) < new Date()) {
      setError("Start time must be in the future");
      return;
    }

    // If no available slot selected, show error
    if (!availableSlotId) {
      setError("Select Available Slot");
      return;
    }

    // Validation for admin client selection and tasks
    if (isAdmin && clientId === 0) {
      setError("Pick a client");
      return;
    }

    // Ensure healthcare worker id is set
    if (!healthcareWorkerId) return setError("Selected slot has no healthcare worker");

    // Ensure at least one task is added
    if (appointmentTasks.length < 1) {
      setError("Add task to appointment");
      return;
    }

    // Trim task descriptions and prepare appointment object
    const tasksToSave = appointmentTasks.map((t) => ({ description: t.description.trim(), isCompleted: false }));

    // Create appointment object
    const appointment: Appointment = {
      clientId,
      healthcareWorkerId,
      start: start,
      end: end,
      notes,
      availableSlotId,
      appointmentTasks: tasksToSave,
    };
    // Call onAppointmentChanged prop with new appointment
    onAppointmentChanged(appointment);
  };

  // Render form
  return (
    <Form onSubmit={handleSubmit} style={{ maxWidth: "600px", margin: "0 auto" }}>
      <h3 className="mb-4">Create Appointment</h3>
      {/* Client selection for admin users */}
      {isAdmin && clients && (
        <Form.Group controlId="formClientId" className="mb-3">
          <Form.Label>Client</Form.Label>
          <Form.Select value={clientId} onChange={(e) => setClientId(Number(e.target.value))} required>
            <option value={0}>Select a client</option>
            {clients.map((cl) => (
              <option key={cl.id} value={cl.id}>
                {cl.name || `Client ${cl.id}`}
              </option>
            ))}
          </Form.Select>
        </Form.Group>
      )}
      {/* Available slot selection */}
      <Form.Group controlId="formAvailableSlotId" className="mb-3">
        <Form.Label>Available Slot</Form.Label>
        <Form.Select value={availableSlotId} onChange={(e) => handleSlotChange(Number(e.target.value))} required>
          <option value={0}>Pick an available time slot</option>
          {/* Populate options with unbooked slots. If none, show disabled option */}
          {unbookedSlots.length > 0 ? (
            unbookedSlots.map((slot) => (
              <option key={slot.id} value={slot.id}>
                {new Date(slot.start).toLocaleString()} — {new Date(slot.end).toLocaleString()}
              </option>
            ))
          ) : (
            <option disabled>No available slots</option>
          )}
        </Form.Select>
      </Form.Group>
      {/* Notes section */}
      <Form.Group controlId="formNotes" className="mb-3">
        <Form.Label>Notes</Form.Label>
        <Form.Control as="textarea" rows={3} value={notes} onChange={(e) => setNotes(e.target.value)} required />
      </Form.Group>
      {/* Render task inputs */}
      {appointmentTasks.length > 0 && (
        <div className="d-flex flex-column gap-2">
          <Form.Label>Tasks</Form.Label>
          {/* Render each task as a disabled input */}
          {appointmentTasks.map((t, idx) => (
            <InputGroup key={idx}>
              <Form.Control value={t.description} disabled />
              {/* Button to remove task */}
              <Button
                variant="outline-danger"
                onClick={() => removeTask(idx)}
                aria-label={`Remove task ${idx + 1}`}>
                Remove
              </Button>
            </InputGroup>
          ))}
        </div>
      )}
      {/* Task input section */}
      <Form.Group controlId="formTasks" className="mb-3 mt-2">
        <Form.Label>Add Task</Form.Label>
        <InputGroup className="mb-2">
          {/* Input for task description. If Enter is pressed, add the task */}
          <Form.Control
            type="text"
            placeholder="e.g., Take vitals, Initial consultation"
            value={taskInput}
            onChange={(e) => setTaskInput(e.target.value)}
            onKeyDown={(e) => {
              if (e.key === "Enter") {
                e.preventDefault();
                addTask();
              }
            }}
          />
          {/* Add task button */}
          <Button variant="outline-primary" onClick={addTask}>
            Add task
          </Button>
        </InputGroup>
      </Form.Group>
      {/* Show error messages */}
      {error && <p style={{ color: "red" }}>{error}</p>}
      {submitError && <p style={{ color: "red" }}>{submitError}</p>}

      {/* Form action buttons */}
      <div className="d-flex justify-content-between mt-4">
        <Button variant="secondary" onClick={onCancel}>
          Cancel
        </Button>
        <Button variant="primary" type="submit" disabled={isSubmitting}>
          {/* When submitting, show spinner */}
          {isSubmitting ? (
            <>
              <Spinner as="span" animation="border" size="sm" className="me-2" />
              Creating…
            </>
          ) : (
            "Create"
          )}
        </Button>
      </div>
    </Form>
  );
};

export default CreateAppointmentForm;
