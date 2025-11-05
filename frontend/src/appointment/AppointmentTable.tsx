import React from "react";
import { Button } from "react-bootstrap";
import { Link } from "react-router-dom";
import { Appointment } from "../types/appointment";

interface AppointmentTableProps {
  appointments: Appointment[];
  onAppointmentDeleted?: (id: number) => void;
}
const AppointmentTable: React.FC<AppointmentTableProps> = ({ appointments, onAppointmentDeleted }) => {
  return (
    <table className="table table-striped">
      <thead>
        <tr>
          <th>Client ID</th>
          <th>Worker ID</th>
          <th>Start</th>
          <th>End</th>
          <th>Notes</th>
          <th>Slot Id</th>
          <th>Actions</th>
          <th>Tasks</th>
        </tr>
      </thead>
      <tbody>
        {appointments.map((a) => (
          <tr key={a.id}>
            <td>{a.clientId}</td>
            <td>{a.healthcareWorkerId}</td>
            <td>{a.start}</td>
            <td>{a.end}</td>
            <td>{a.notes}</td>
            <td>{a.availableSlotId}</td>
            <td>
              {a.appointmentTasks && a.appointmentTasks.length > 0
                ? a.appointmentTasks.map((t: any, i: number) => <div key={i}>{t.description}</div>)
                : "No tasks"}
            </td>
            <td>
              <Link to={`/appointment/${a.id}`} className="btn btn-primary btn-sm me-2">
                Update
              </Link>
              {onAppointmentDeleted && a.id && (
                <>
                  <Button variant="danger" size="sm" onClick={() => onAppointmentDeleted(a.id!)}>
                    Delete
                  </Button>
                </>
              )}
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  );
};
export default AppointmentTable;
