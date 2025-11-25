import React from "react";
import { Button, Table } from "react-bootstrap";
import { Link } from "react-router-dom";
import { AppointmentView } from "../types/appointment";
import { formatDateOnly, formatTimeOnly } from "../shared/timeUtils";

// Props for AppointmentTable component
interface AppointmentTableProps {
  appointments: AppointmentView[];
  onDeleteClick?: (a: AppointmentView) => void;
  isAdmin?: boolean;
  isWorker?: boolean;
  isClient?: boolean;
  isPrev?: boolean;
}

const AppointmentTable: React.FC<AppointmentTableProps> = ({
  appointments,
  onDeleteClick,
  isAdmin = false,
  isWorker = false,
  isClient = false,
  isPrev = false,
}) => {
  // Dynamic column count for proper no appointments found
  const nameCols = isAdmin ? 2 : isWorker || isClient ? 1 : 0;
  const columnsCount = nameCols + 4;
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
          <th>Date</th>
          <th>Start</th>
          <th>End</th>
          {/* <th>Notes</th> */}
          <th>Options</th>
        </tr>
      </thead>
      <tbody>
        {/* If no appointments, show message with dynamic column count */}
        {appointments.length === 0 ? (
          <tr>
            <td colSpan={columnsCount} className="text-center text-muted">
              No appointments found.
            </td>
          </tr>
        ) : (
          // If appointments exist, map and show each appointment row.
          // Show different columns based on roles.
          appointments.map((a) => (
            <tr key={a.id}>
              {isAdmin && (
                <>
                  <td>{a.clientName}</td>
                  <td>{a.healthcareWorkerName}</td>
                </>
              )}

              {isWorker && <td>{a.clientName}</td>}
              {isClient && <td>{a.healthcareWorkerName}</td>}
              <td>{formatDateOnly(a.start)}</td>
              <td>{formatTimeOnly(a.start)}</td>
              <td>{formatTimeOnly(a.end)}</td>
              <td>
                {/* 
                  Link to details and update pages.
                  Show cancel button if onDeleteClick prop is provided
                */}
                <Link to={`/appointment/${a.id}`} className="btn btn-primary btn-sm me-2">
                  Details
                </Link>
                {!isPrev && (
                  <Link to={`/appointment/${a.id}/update`} className="btn btn-primary btn-sm me-2">
                    Update
                  </Link>
                )}
                {onDeleteClick && !isPrev && (
                  <Button
                    variant="danger"
                    size="sm"
                    disabled={new Date(a.start) < new Date()}
                    onClick={() => onDeleteClick(a)}>
                    Cancel
                  </Button>
                )}
              </td>
            </tr>
          ))
        )}
      </tbody>
    </Table>
  );
};
export default AppointmentTable;
