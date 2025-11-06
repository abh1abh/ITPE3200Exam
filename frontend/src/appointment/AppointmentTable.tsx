import React from "react";
import { Button, Table } from "react-bootstrap";
import { Link } from "react-router-dom";
import { AppointmentView } from "../types/appointment";
import { formatDate } from "../shared/timeUtils";

interface AppointmentTableProps {
  appointments: AppointmentView[];
  onAppointmentDeleted?: (id: number) => void;
  isAdmin?: boolean;
  isWorker?: boolean;
  isClient?: boolean;
}
const AppointmentTable: React.FC<AppointmentTableProps> = ({
  appointments,
  onAppointmentDeleted,
  isAdmin = false,
  isWorker = false,
  isClient = false,
}) => {
  return (
    <Table striped bordered hover responsive>
      <thead>
        <tr>
          {isAdmin && (
            <>
              <th>Client</th>
              <th>Worker</th>
            </>
          )}

          {isWorker && <th>Client</th>}
          {isClient && <th>Worker</th>}
          <th>Start</th>
          <th>End</th>
          <th>Notes</th>
          <th>Actions</th>
        </tr>
      </thead>
      <tbody>
        {appointments.map((a) => (
          <tr key={a.id}>
            {isAdmin && (
              <>
                <td>{a.clientName}</td>
                <td>{a.healthcareWorkerName}</td>
              </>
            )}

            {isWorker && <td>{a.clientName}</td>}
            {isClient && <td>{a.healthcareWorkerName}</td>}
            <td>{formatDate(a.start)}</td>
            <td>{formatDate(a.end)}</td>
            <td>{a.notes}</td>
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
    </Table>
  );
};
export default AppointmentTable;
