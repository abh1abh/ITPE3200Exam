import React, { useState } from "react";
import { Button, Form } from "react-bootstrap";
import { useNavigate } from "react-router-dom";
import { Appointment } from "../types/appointment";
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
const [healthcareWorkerId, setHealthcareWorkerId] = useState(initialData?.healthcareWorkerId || 0);
const [start, setStart] = useState(initialData?.start || "");
const [end, setEnd] = useState(initialData?.end || "");
const [notes, setNotes] = useState(initialData?.notes || "");
const [appointmentTasks, setAppointmentTasks] = useState(initialData?.appointmentTasks ?.[0]?.description|| "");
const [availableSlotId, setAvailableSlotId] = useState(initialData?.availableSlotId || 0);
const [isCompleted, setIsCompleted] = useState(false);
const [error, setError] = useState<string | null>(null);
const navigate = useNavigate();
const onCancel = () => navigate(-1);
const handleSubmit = (event: React.FormEvent) => {
    event.preventDefault();
    if (!start || !end) {
    setError("Start and end time are required!!");
    return;
    }
    const appointment: Appointment = {
    id: initialData?.id ?? 0,
    clientId,
    healthcareWorkerId,
    start,
    end,
    notes,
    availableSlotId: availableSlotId || 0,
    appointmentTasks: [
        {
        id:0,
        appointmentId: clientId, 
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
    changeDescription: isUpdate? "Appointment updated from frontend!": "Appointment created from frontend",
},];
return (
    <Form onSubmit={handleSubmit} style={{ maxWidth: "600px", margin: "0 auto" }}>
    <h3 className="mb-4">{isUpdate ? "Update Appointment" : "Create Appointment"}</h3>
    <Form.Group controlId="formClientId" className="mb-3">
        <Form.Label>Client ID</Form.Label>
        <Form.Control
        type="number"
        value={clientId}
        onChange={(e) => setClientId(Number(e.target.value))}
        required
        />
    </Form.Group>
    <Form.Group controlId="formWorkerId" className="mb-3">
        <Form.Label>Healthcare Worker ID</Form.Label>
        <Form.Control
        type="number"
        value={healthcareWorkerId}
        onChange={(e) => setHealthcareWorkerId(Number(e.target.value))}
        required
        />
    </Form.Group>
    <Form.Group controlId="formStart" className="mb-3">
        <Form.Label>Start Time</Form.Label>
        <Form.Control
        type="datetime-local"
        value={start}
        onChange={(e) => setStart(e.target.value)}
        required
        />
    </Form.Group>
    <Form.Group controlId="formEnd" className="mb-3">
        <Form.Label>End Time</Form.Label>
        <Form.Control
        type="datetime-local"
        value={end}
        onChange={(e) => setEnd(e.target.value)}
        required
        />
    </Form.Group>
    <Form.Group controlId="formNotes" className="mb-3">
        <Form.Label>Notes</Form.Label>
        <Form.Control
        as="textarea"
        rows={3}
        value={notes}
        onChange={(e) => setNotes(e.target.value)}
        />
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
        onChange={(e) => setIsCompleted(e.target.checked)}/></Form.Group>

<Form.Group controlId="formAvailableSlotId" className="mb-3">
<Form.Label>Available Slot ID</Form.Label>
<Form.Control
    type="number"
    value={availableSlotId}
    onChange={(e) => setAvailableSlotId(Number(e.target.value))
        
    }
    required/></Form.Group>


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