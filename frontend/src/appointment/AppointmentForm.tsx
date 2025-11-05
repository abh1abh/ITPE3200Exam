import React, { useEffect, useState } from "react";
import { Button, Form } from "react-bootstrap";
import { useNavigate } from "react-router-dom";
import { Appointment } from "../types/appointment";
import * as HealthcareWorkerService from "../healtcareWorker/HealthcareWorkerService";
import * as ClientService from "../client/ClientService";
import * as AvailableSlotService from "../availableslot/AvailableSlotService";
import { HealthcareWorker } from "../types/HealthcareWorker";
import { Client } from "../types/Client";
import { AvailableSlot } from "../types/AvailableSlot";

interface AppointmentFormProps {
  onAppointmentChanged: (newAppointment: Appointment) => void;
  initialData?: Appointment;
  isUpdate?: boolean;
  clients?: Client[];
  unbookedSlots: AvailableSlot[];
  isAdmin: boolean;
  serverError?: string | null;
}
const AppointmentForm: React.FC<AppointmentFormProps> = ({
  onAppointmentChanged,
  initialData,
  isUpdate = false,
  clients = [],
  unbookedSlots,
  isAdmin,
  serverError = null,
}) => {
  const [clientId, setClientId] = useState(initialData?.clientId || 0);
  const [notes, setNotes] = useState(initialData?.notes || "");
  const [healthcareWorkerId, setHealthcareWorkerId] = useState(initialData?.healthcareWorkerId || 0);
  const [appointmentTasks, setAppointmentTasks] = useState(initialData?.appointmentTasks?.[0]?.description || "");
  const [availableSlotId, setAvailableSlotId] = useState(initialData?.availableSlotId || 0);
  const [isCompleted, setIsCompleted] = useState(initialData?.appointmentTasks?.[0]?.isCompleted || false);
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

  const handleSubmit = (event: React.FormEvent) => {
    event.preventDefault();

    if (new Date(start) < new Date()) {
      setError("Start time need to before now");
      return;
    }

    if (!availableSlotId) {
      setError("select time!!!");
      return;
    }

    if (isAdmin && clientId === 0) {
      setError("Pick a client");
      return;
    }
    if (!healthcareWorkerId) return setError("Selected slot has no healthcare worker");

    const appointment: Appointment = {
      clientId,
      healthcareWorkerId,
      start: start,
      end: end,
      notes,
      availableSlotId,
      appointmentTasks: [
        {
          description: appointmentTasks || "Initial consultation",
          isCompleted: isCompleted,
        },
      ],
    };
    onAppointmentChanged(appointment);
  };

  return (
    <Form onSubmit={handleSubmit} style={{ maxWidth: "600px", margin: "0 auto" }}>
      <h3 className="mb-4">{isUpdate ? "Update Appointment" : "Create Appointment"}</h3>
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

      <Form.Group controlId="formTask" className="mb-3">
        <Form.Label>Task</Form.Label>
        <Form.Control
          type="text"
          placeholder="e.g., Initial consultation"
          value={appointmentTasks}
          onChange={(e) => setAppointmentTasks(e.target.value)}
          required
        />
      </Form.Group>

      <Form.Group controlId="formTaskCompleted" className="mb-3">
        <Form.Check
          type="checkbox"
          label="Task completed?"
          checked={isCompleted}
          onChange={(e) => setIsCompleted(e.target.checked)}
        />
      </Form.Group>

      {error && <p style={{ color: "red" }}>{error}</p>}
      {serverError && <p style={{ color: "red" }}>{serverError}</p>}

      <div className="d-flex justify-content-between mt-4">
        <Button variant="secondary" onClick={onCancel}>
          Cancel
        </Button>
        <Button variant="dark" type="submit">
          {isUpdate ? "Update" : "Create"}
        </Button>
      </div>
    </Form>
  );
};

export default AppointmentForm;
