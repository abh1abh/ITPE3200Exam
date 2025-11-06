import React, { useEffect, useState } from "react";
import { Button, Form, InputGroup } from "react-bootstrap";
import { useNavigate } from "react-router-dom";
import { Appointment, AppointmentTask } from "../types/appointment";
import { Client } from "../types/client";
import { AvailableSlot } from "../types/availableSlot";

interface CreateAppointmentFormProps {
  onAppointmentChanged: (newAppointment: Appointment) => void;
  clients?: Client[];
  unbookedSlots: AvailableSlot[];
  isAdmin: boolean;
  serverError?: string | null;
}
const CreateAppointmentForm: React.FC<CreateAppointmentFormProps> = ({
  onAppointmentChanged,
  clients = [],
  unbookedSlots,
  isAdmin,
  serverError = null,
}) => {
  const [clientId, setClientId] = useState<number>(0);
  const [notes, setNotes] = useState<string>("");
  const [healthcareWorkerId, setHealthcareWorkerId] = useState<number>(0);

  const [appointmentTasks, setAppointmentTasks] = useState<AppointmentTask[]>([]);
  const [taskInput, setTaskInput] = useState<string>("");

  const [availableSlotId, setAvailableSlotId] = useState<number>(0);
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();
  const onCancel = () => navigate(-1);

  const [start, setStart] = useState<string>("");
  const [end, setEnd] = useState<string>("");

  const handleSlotChange = (slotId: number) => {
    setAvailableSlotId(slotId);
    const slot = unbookedSlots.find((s) => s.id === slotId);
    if (!slot) return;
    setStart(slot.start);
    setEnd(slot.end);
    if (slot.healthcareWorkerId) setHealthcareWorkerId(slot.healthcareWorkerId);
  };

  const addTask = () => {
    const trimmed = taskInput.trim();
    if (!trimmed) return;
    setAppointmentTasks((prev) => [...prev, { description: trimmed, isCompleted: false }]);
    setTaskInput("");
  };

  const removeTask = (index: number) => {
    if (appointmentTasks.length < 1) return;
    setAppointmentTasks((prev) => prev.filter((_, i) => i !== index));
  };

  const handleSubmit = (event: React.FormEvent) => {
    event.preventDefault();

    if (new Date(start) < new Date()) {
      setError("Start time need to before now");
      return;
    }

    if (!availableSlotId) {
      setError("Select Available Slot");
      return;
    }

    if (isAdmin && clientId === 0) {
      setError("Pick a client");
      return;
    }
    if (!healthcareWorkerId) return setError("Selected slot has no healthcare worker");

    if (appointmentTasks.length < 1) {
      setError("Add task to appointment");
      return;
    }
    const tasksToSave = appointmentTasks.map((t) => ({ description: t.description.trim(), isCompleted: false }));

    const appointment: Appointment = {
      clientId,
      healthcareWorkerId,
      start: start,
      end: end,
      notes,
      availableSlotId,
      appointmentTasks: tasksToSave,
    };
    onAppointmentChanged(appointment);
  };

  return (
    <Form onSubmit={handleSubmit} style={{ maxWidth: "600px", margin: "0 auto" }}>
      <h3 className="mb-4">{"Create Appointment"}</h3>
      {isAdmin && clients && (
        <Form.Group controlId="formClientId" className="mb-3">
          <Form.Label>Client</Form.Label>
          <Form.Select value={clientId} onChange={(e) => setClientId(Number(e.target.value))} required>
            <option value={0}>Select a client</option>
            {clients.map((cl) => (
              <option key={cl.clientId} value={cl.clientId}>
                {cl.name || `Client ${cl.clientId}`}
              </option>
            ))}
          </Form.Select>
        </Form.Group>
      )}
      <Form.Group controlId="formAvailableSlotId" className="mb-3">
        <Form.Label>Available Slot</Form.Label>
        <Form.Select value={availableSlotId} onChange={(e) => handleSlotChange(Number(e.target.value))} required>
          <option value={0}>Pick an available time slot</option>
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

      <Form.Group controlId="formNotes" className="mb-3">
        <Form.Label>Notes</Form.Label>
        <Form.Control as="textarea" rows={3} value={notes} onChange={(e) => setNotes(e.target.value)} />
      </Form.Group>

      {appointmentTasks.length > 0 && (
        <div className="d-flex flex-column gap-2">
          <Form.Label>Tasks</Form.Label>
          {appointmentTasks.map((t, idx) => (
            <InputGroup key={idx}>
              <Form.Control value={t.description} disabled />
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

      <Form.Group controlId="formTasks" className="mb-3 mt-2">
        <Form.Label>Add Task</Form.Label>
        <InputGroup className="mb-2">
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
          <Button variant="outline-primary" onClick={addTask}>
            Add task
          </Button>
        </InputGroup>
      </Form.Group>

      {error && <p style={{ color: "red" }}>{error}</p>}
      {serverError && <p style={{ color: "red" }}>{serverError}</p>}

      <div className="d-flex justify-content-between mt-4">
        <Button variant="secondary" onClick={onCancel}>
          Cancel
        </Button>
        <Button variant="primary" type="submit">
          Create
        </Button>
      </div>
    </Form>
  );
};

export default CreateAppointmentForm;
