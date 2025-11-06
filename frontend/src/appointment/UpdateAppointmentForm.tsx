import React, { useRef, useState } from "react";
import { Appointment, AppointmentTask, AppointmentView } from "../types/appointment";
import { Client } from "../types/client";
import { useNavigate } from "react-router-dom";
import { Button, Card, Col, Form, InputGroup, ListGroup, Row, Stack } from "react-bootstrap";
import { formatDate } from "../shared/timeUtils";

interface UpdateAppointmentFormProps {
  initialData: AppointmentView;
  onAppointmentChanged: (updated: Appointment) => void;
  serverError?: string | null;
}

const UpdateAppointmentForm: React.FC<UpdateAppointmentFormProps> = ({
  initialData,
  onAppointmentChanged,
  serverError = null,
}) => {
  const navigate = useNavigate();
  const onCancel = () => navigate(-1);

  const [notes, setNotes] = useState<string>(initialData.notes ?? "");
  const [appointmentTasks, setAppointmentTasks] = useState<AppointmentTask[]>(initialData.appointmentTasks);
  const [taskInput, setTaskInput] = useState<string>("");
  const [error, setError] = useState<string | null>(null);

  const lockedTaskKeysRef = useRef(
    new Set(initialData.appointmentTasks.filter((t) => t.isCompleted).map((t) => t.id))
  );

  const isLocked = (t: AppointmentTask) => lockedTaskKeysRef.current.has(t.id);

  const handleToggleCompleted = (index: number, value: boolean) => {
    setAppointmentTasks((prev) =>
      prev.map((task, i) => {
        if (i !== index) return task;
        if (isLocked(task)) return task;
        return { ...task, isCompleted: value };
      })
    );
  };

  const addTask = () => {
    const trimmed = taskInput.trim();
    if (!trimmed) return;
    setAppointmentTasks((prev) => [...prev, { id: undefined, description: trimmed, isCompleted: false }]);
    setTaskInput("");
  };

  const removeTask = (index: number) => {
    setAppointmentTasks((prev) => {
      const task = prev[index];
      if (task && isLocked(task)) return prev;
      return prev.filter((_, i) => i !== index);
    });
  };

  const handleSubmit = (event: React.FormEvent) => {
    event.preventDefault();

    if (appointmentTasks.length < 1) {
      setError("Add task to appointment");
      return;
    }
    const tasksToSave = appointmentTasks.map((t) => ({
      id: t.id, // include id if present
      description: t.description.trim(),
      isCompleted: t.isCompleted,
    }));

    console.log(tasksToSave);

    const appointment: Appointment = {
      ...initialData,
      notes,
      appointmentTasks: tasksToSave,
    };
    onAppointmentChanged(appointment);
  };

  return (
    <Form onSubmit={handleSubmit} style={{ maxWidth: "600px", margin: "0 auto" }}>
      <h3 className="mb-4">{"Update appointment"}</h3>

      <Card className="mb-3">
        <Card.Header className="fw-semibold">Appointment details</Card.Header>
        <Card.Body>
          <Row className="row-cols-1 row-cols-md-2 g-3">
            <Col className="text-start">
              <div className="text-muted small mb-1">Client</div>
              <div className="fw-semibold">{initialData.clientName}</div>
            </Col>
            <Col className="text-start">
              <div className="text-muted small mb-1">Healthcare worker</div>
              <div className="fw-semibold">{initialData.healthcareWorkerName}</div>
            </Col>

            <Col className="text-start">
              <div className="text-muted small mb-1">Start time</div>
              <div className="fw-semibold">{formatDate(initialData.start)}</div>
            </Col>
            <Col className="text-start">
              <div className="text-muted small mb-1">End time</div>
              <div className="fw-semibold">{formatDate(initialData.end)}</div>
            </Col>
          </Row>
        </Card.Body>
      </Card>

      <Form.Group controlId="formNotes" className="mb-3">
        <Form.Label>Notes</Form.Label>
        <Form.Control as="textarea" rows={3} value={notes} onChange={(e) => setNotes(e.target.value)} />
      </Form.Group>

      {appointmentTasks.length > 0 && (
        <div className="d-flex flex-column gap-2">
          <Form.Label>Tasks</Form.Label>
          {appointmentTasks.map((t, idx) => {
            const locked = isLocked(t);
            return (
              <InputGroup key={`${t.description}-${idx}`}>
                <Form.Control value={t.description} disabled />

                <InputGroup.Text>
                  <Form.Check
                    type="checkbox"
                    checked={t.isCompleted}
                    disabled={locked}
                    onChange={(e) => handleToggleCompleted(idx, e.target.checked)}
                    title={locked ? "Completed earlier; cannot change" : "Mark complete"}
                  />
                </InputGroup.Text>

                <Button
                  variant={locked ? "success" : "outline-danger"}
                  onClick={() => removeTask(idx)}
                  disabled={locked}
                  style={{ width: "100px" }}
                  aria-label={`Remove task ${idx + 1}`}
                  title={locked ? "Completed earlier; cannot remove" : "Remove task"}>
                  {locked ? "Completed" : "Remove"}
                </Button>
              </InputGroup>
            );
          })}
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

export default UpdateAppointmentForm;
