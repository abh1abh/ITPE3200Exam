import React from "react";
import { Appointment, AppointmentView } from "../types/appointment";
import { Badge, Card, Col, Container, ListGroup, Row, Table } from "react-bootstrap";
import { formatDateOnly, formatDateTime, formatTimeOnly } from "../shared/timeUtils";

interface ViewAppointmentCard {
  initialData: AppointmentView;
  serverError?: string | null;
  isAdmin?: boolean;
  isClient?: boolean;
  isWorker?: boolean;
}

const ViewAppointmentCard: React.FC<ViewAppointmentCard> = ({
  initialData,
  serverError = null,
  isAdmin,
  isClient,
  isWorker,
}) => {
  console.log(isAdmin);
  return (
    <Container style={{ maxWidth: "35rem" }}>
      <Card className="mb-3 border-0">
        <Card.Header className="fw-semibold ">Details</Card.Header>
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

          {initialData.notes?.trim() && (
            <>
              <hr className="my-4" />
              <div className="fw-semibold small mb-1">Notes</div>
              <div className="" style={{ whiteSpace: "pre-wrap" }}>
                {initialData.notes}
              </div>
            </>
          )}

          {initialData.appointmentTasks.length > 0 && (
            <>
              <hr className="my-4" />
              <div className="fw-semibold small mb-1">Task</div>
              <Table borderless className="align-middle mb-3">
                <thead className="small">
                  <tr>
                    <th>Description</th>
                    <th>Status</th>
                  </tr>
                </thead>
                <tbody>
                  {initialData.appointmentTasks.map((t, idx) => {
                    return (
                      <tr key={`${t.description}-${idx}`}>
                        <td>{t.description}</td>
                        <td>
                          <Badge bg={t.isCompleted ? "success" : "secondary"}>
                            {t.isCompleted ? "Completed" : "Pending"}
                          </Badge>
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </Table>
            </>
          )}
        </Card.Body>
      </Card>
    </Container>
  );
};

export default ViewAppointmentCard;
