import React from "react";
import { Button, Card, Col, Row } from "react-bootstrap";

interface AppointmentGridProps {
appointments: any[];
apiUrl: string;
onAppointmentDeleted: (id: number) => void;
}
const AppointmentGrid: React.FC<AppointmentGridProps> = ({appointments,apiUrl,onAppointmentDeleted,}) => {
return (
    <Row xs={1} sm={2} md={3} lg={4} className="g-4">
    {appointments.map(appointment => (
        <Col key={appointment.apiUrl}>
        <Card className="h-100">
            <Card.Body className="d-flex flex-column">
            <Card.Title>
                Appointment {appointment.appointmentId} — Client {appointment.name}
            </Card.Title>
            <Card.Text>
                <strong>Worker:</strong> {appointment.healthcareWorkerId}
                <br />
                <strong>Start:</strong>{appointment.start}
                <br />
                <strong>End:</strong> {appointment.end}
            </Card.Text>
            <div className="mt-auto d-flex gap-2 justify-content-between">
                <Button href={`/appointmentupdate/${appointment.appointmentId}`} variant="primary" size="sm">Update</Button>
                <Button variant="danger" onClick={() => onAppointmentDeleted(appointment.id)}>Delete</Button>
            </div>
            </Card.Body>
        </Card>
        </Col>
    ))}
    </Row>
);
};
export default AppointmentGrid;