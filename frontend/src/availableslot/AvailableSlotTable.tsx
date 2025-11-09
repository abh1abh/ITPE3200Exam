import React from "react";
import { Badge, Button, Table } from "react-bootstrap";
import { AvailableSlot } from "../types/availableSlot";
import { Link } from "react-router-dom";
import { formatDateOnly, formatTimeOnly } from "../shared/timeUtils";
// import { formatDate } from "../shared/timeUtils";

interface Props {
  availableSlots: AvailableSlot[];
  isAdmin: boolean;
  onDeleteClick: (slot: AvailableSlot) => void;
}
const AvailableSlotTable: React.FC<Props> = ({ availableSlots, isAdmin, onDeleteClick }) => {
  return (
    <Table striped bordered hover responsive>
      <thead>
        <tr>
          <th>#</th>
          {isAdmin ? <th>Healthcare Worker Id</th> : null}
          <th>Date</th>
          <th>Start</th>
          <th>End</th>
          <th>Status</th>
          <th>Options</th>
        </tr>
      </thead>
      <tbody>
        {availableSlots.length === 0 ? (
          <tr>
            <td colSpan={7} className="text-center text-muted">
              No available slots found.
            </td>
          </tr>
        ) : (
          availableSlots.map((slot) => (
            <tr key={slot.id}>
              <td>{slot.id}</td>
              {isAdmin ? <td>{slot.healthcareWorkerId}</td> : null}
              <td>{formatDateOnly(slot.start)}</td>
              <td>{formatTimeOnly(slot.start)}</td>
              <td>{formatTimeOnly(slot.end)}</td>
              <td>{slot.isBooked ? <Badge bg="danger">Booked</Badge> : <Badge bg="success">Open</Badge>}</td>
              <td>
                {slot.isBooked ? (
                  <span className="btn btn-sm btn-primary me-2 disabled" aria-disabled="true">
                    Update
                  </span>
                ) : (
                  <Link to={`/availableslot/${slot.id}`} className="btn btn-sm btn-primary me-2">
                    Update
                  </Link>
                )}
                <Button variant="danger" size="sm" disabled={slot.isBooked} onClick={() => onDeleteClick(slot)}>
                  Delete{" "}
                </Button>
              </td>
            </tr>
          ))
        )}
      </tbody>
    </Table>
  );
};

export default AvailableSlotTable;
