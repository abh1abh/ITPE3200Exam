import React, { useRef, useState } from "react";
import { Appointment, AppointmentTask, AppointmentView } from "../types/appointment";
import { useNavigate } from "react-router-dom";
import { Badge, Button, Card, Col, Container, Form, InputGroup, Row, Table } from "react-bootstrap";
import { formatDateOnly, formatDateTime, formatTimeOnly } from "../shared/timeUtils";
// import { formatDate } from "../shared/timeUtils";

interface UpdateAppointmentFormProps {
  initialData: AppointmentView;
  onAppointmentChanged: (updated: Appointment) => void;
  serverError?: string | null;
  isAdmin?: boolean;
  isClient?: boolean;
  isWorker?: boolean;
}

const UpdateAppointmentForm: React.FC<UpdateAppointmentFormProps> = ({
  initialData,
  onAppointmentChanged,
  serverError = null,
  isAdmin,
  isClient,
  isWorker,
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
    setError(null);
    const trimmed = taskInput.trim();
    if (!trimmed) {
      setError("Need to fill out description of task");
      return;
    }
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
    <Container style={{ maxWidth: "35rem" }}>
      <Card className="mb-3 border-0 ">
        <Card.Header className="fw-semibold">Details</Card.Header>
        <Card.Body>
          <Row className="g-3">
            {isAdmin && (
              <>
                <Col className="text-start">
                  <div className="text-muted small mb-1">Client</div>
                  <div className="fw-semibold">{initialData.clientName}</div>
                </Col>
                <Col className="text-start">
                  <div className="text-muted small mb-1">Healthcare worker</div>
                  <div className="fw-semibold">{initialData.healthcareWorkerName}</div>
                </Col>
              </>
            )}
            {isClient && (
              <>
                <Col className="text-start">
                  <div className="text-muted small mb-1">Healthcare worker</div>
                  <div className="fw-semibold">{initialData.healthcareWorkerName}</div>
                </Col>
              </>
            )}
            {isWorker && (
              <>
                <Col className="text-start">
                  <div className="text-muted small mb-1">Client</div>
                  <div className="fw-semibold">{initialData.clientName}</div>
                </Col>
              </>
            )}
          </Row>

          <Row className="g-3 mt-1">
            {/* Date full width */}
            <Col xs={12} className="text-start">
              <div className="text-muted small mb-1">Date</div>
              <div className="fw-semibold">{formatDateOnly(initialData.start)}</div>
            </Col>

            {/* Start / End side-by-side */}
            <Col xs={12} md={6} className="text-start">
              <div className="text-muted small mb-1">Start time</div>
              <div className="fw-semibold">{formatTimeOnly(initialData.start)}</div>
            </Col>
            <Col xs={12} md={6} className="text-start">
              <div className="text-muted small mb-1">End time</div>
              <div className="fw-semibold">{formatTimeOnly(initialData.end)}</div>
            </Col>
          </Row>

          <hr className="my-4" />
          <Form onSubmit={handleSubmit}>
            <Form.Group controlId="formNotes" className="mb-3">
              <Form.Label className="fw-semibold small mb-1">Notes</Form.Label>
              <Form.Control as="textarea" rows={3} value={notes} onChange={(e) => setNotes(e.target.value)} />
            </Form.Group>

            {appointmentTasks.length > 0 && (
              <>
                <hr className="my-4" />

                <Form.Label className="fw-semibold small mb-1">Task</Form.Label>
                <Table borderless className="align-middle mb-3">
                  <thead className="small">
                    <tr>
                      <th></th>
                      <th>Description</th>
                      <th>Status</th>
                      <th></th>
                    </tr>
                  </thead>
                  <tbody>
                    {appointmentTasks.map((t, idx) => {
                      const locked = isLocked(t);

                      return (
                        <tr key={`${t.description}-${idx}`}>
                          <td className="text-center">
                            <Form.Check
                              type="checkbox"
                              checked={t.isCompleted}
                              disabled={locked}
                              onChange={(e) => handleToggleCompleted(idx, e.target.checked)}
                            />
                          </td>
                          <td>{t.description}</td>
                          <td>
                            <Badge bg={t.isCompleted ? "success" : "secondary"}>
                              {t.isCompleted ? "Completed" : "Pending"}
                            </Badge>
                          </td>
                          <td>
                            {!locked && (
                              <Button
                                variant="outline-danger"
                                size="sm"
                                className="btn-xs" // Custom class for smaller button (in index.css)
                                onClick={() => removeTask(idx)}>
                                Remove
                              </Button>
                            )}
                          </td>
                        </tr>
                      );
                    })}
                  </tbody>
                </Table>
              </>
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
                Update
              </Button>
            </div>
          </Form>
        </Card.Body>
      </Card>
    </Container>
  );
};

export default UpdateAppointmentForm;
