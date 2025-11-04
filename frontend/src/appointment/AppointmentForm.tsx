import React, { useEffect, useState } from "react";
import { Button, Form } from "react-bootstrap";
import { useNavigate } from "react-router-dom";
import { Appointment } from "../types/appointment";
// import { Appointment } from "../types/appointment";
interface AppointmentFormProps {
  onAppointmentChanged: (newAppointment: Appointment) => void;
  initialData?: Appointment;
  isUpdate?: boolean;
}
const AppointmentForm: React.FC<AppointmentFormProps> = ({
  onAppointmentChanged,
  initialData,
  isUpdate = false,
}) => {
  const [clientId, setClientId] = useState(initialData?.clientId || 0);
  const [notes, setNotes] = useState(initialData?.notes || "");
  const [healthcareWorkerId, setHealthcareWorkerId] = useState(initialData?.healthcareWorkerId || 0);
  const [appointmentTasks, setAppointmentTasks] = useState(initialData?.appointmentTasks?.[0]?.description || "");
  const [availableSlotId, setAvailableSlotId] = useState(initialData?.availableSlotId || 0);
  const [isCompleted, setIsCompleted] = useState(initialData?.appointmentTasks?.[0]?.isCompleted || false);
  const [workers, setWorkers] = useState<any[]>([]);
  const [clients, setClients] = useState<any[]>([]);
  const [availableSlots, setAvailableSlots] = useState<any[]>([]);
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();
  const onCancel = () => navigate(-1);
  useEffect(() => {
    const fetchAll = async () => {
      try {
        const token = localStorage.getItem("token");
        const [slotr, workerr, clienr] = await Promise.all([
          fetch(`${import.meta.env.VITE_API_URL}/api/AvailableSlot`, {
            headers: { Authorization: `Bearer ${token}` },
          }),
          fetch(`${import.meta.env.VITE_API_URL}/api/HealthcareWorker`, {
            headers: { Authorization: `Bearer ${token}` },
          }),
          fetch(`${import.meta.env.VITE_API_URL}/api/Client`, { headers: { Authorization: `Bearer ${token}` } }),
        ]);
        if (!slotr.ok || workerr.ok || !clienr.ok) throw new Error("failed");
        const [slotData, workerData, clientData] = await Promise.all([
          slotr.json(),
          workerr.json(),
          clienr.json(),
        ]);
        setAvailableSlots(slotData.filter((s: any) => !s.isBooked));
        setWorkers(workerData);
        setClients(clientData);
      } catch (err) {
        console.error("failed.. :", err);
        setAvailableSlots([]);
      }
    };
    fetchAll();
  }, []);
  const [start, setStart] = useState(initialData?.start || "");
  const [end, setEnd] = useState(initialData?.end || "");
  const handleSlotChange = (slotId: number) => {
    setAvailableSlotId(slotId);
    const selected = availableSlots.find((s) => s.id === slotId);
    if (selected) {
      setStart(selected.start);
      setEnd(selected.end);
    }
  };
  const handleSubmit = (event: React.FormEvent) => {
    event.preventDefault();
    if (!availableSlotId) {
      setError("select time!!!");
      return;
    }
    const appointment: Appointment = {
      id: initialData?.id ?? 0,
      clientId,
      healthcareWorkerId,
      start,
      end,
      notes,
      availableSlotId,
      appointmentTasks: [
        {
          id: initialData?.appointmentTasks?.[0]?.id || 0,
          appointmentId: initialData?.id ?? 0,
          description: appointmentTasks || "Initial consultation",
          isCompleted: isCompleted,
        },
      ],
      changeLogs,
    };
    onAppointmentChanged(appointment);
  };
  const changeLogs = [
    {
      id: 0,
      appointmentId: 0,
      changeDate: new Date().toISOString(),
      changedByUserId: "frontend",
      changeDescription: isUpdate ? "updated from frontend" : "ccreated from frontend",
    },
  ];
  return (
    <Form onSubmit={handleSubmit} style={{ maxWidth: "600px", margin: "0 auto" }}>
      <h3 className="mb-4">{isUpdate ? "Update Appointment" : "Create Appointment"}</h3>

      <Form.Group controlId="formClientId" className="mb-3">
        <Form.Label>Client</Form.Label>
        <Form.Select value={clientId} onChange={(e) => setClientId(Number(e.target.value))} required>
          <option value="">Select a client</option>
          {clients.map((cl) => (
            <option key={cl.clientId} value={cl.clientId}>
              {cl.name || `Client ${cl.clientId}`}
            </option>
          ))}
        </Form.Select>
      </Form.Group>
      <Form.Group controlId="formWorkerId" className="mb-3">
        <Form.Label>Healthcare Worker</Form.Label>
        <Form.Select
          value={healthcareWorkerId}
          onChange={(e) => setHealthcareWorkerId(Number(e.target.value))}
          required>
          <option value="">Select a healthcare worker</option>
          {workers.map((workm) => (
            <option key={workm.healthcareWorkerId} value={workm.healthcareWorkerId}>
              {workm.name || `Worker ${workm.healthcareWorkerId}`}
            </option>
          ))}
        </Form.Select>
      </Form.Group>

      <Form.Group controlId="formAvailableSlotId" className="mb-3">
        <Form.Label>Available Slot</Form.Label>
        <Form.Select value={availableSlotId} onChange={(e) => handleSlotChange(Number(e.target.value))} required>
          <option value="">Pick an available time slot</option>
          {availableSlots.length > 0 ? (
            availableSlots.map((slot) => (
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
