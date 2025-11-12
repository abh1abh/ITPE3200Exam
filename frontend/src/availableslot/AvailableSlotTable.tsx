import React from "react";
import { Badge, Button, Table } from "react-bootstrap";
import { AvailableSlot } from "../types/availableSlot";
import { Link } from "react-router-dom";
import { formatDateOnly, formatTimeOnly } from "../shared/timeUtils";

// Props interface for AvailableSlotTable component
interface AvailableSlotTableProps {
  availableSlots: AvailableSlot[];
  isAdmin: boolean;
  onDeleteClick: (slot: AvailableSlot) => void;
}
const AvailableSlotTable: React.FC<AvailableSlotTableProps> = ({ availableSlots, isAdmin, onDeleteClick }) => {
  // Render table of available slots
  return (
    <Table striped bordered hover responsive>
      <thead>
        <tr>
          <th>#</th>
          {/* If admin, show Healthcare Worker Id column */}
          {isAdmin ? <th>Healthcare Worker Id</th> : null}
          <th>Date</th>
          <th>Start</th>
          <th>End</th>
          <th>Status</th>
          <th>Options</th>
        </tr>
      </thead>
      <tbody>
        {/* If no slots, show message. Otherwise map slots to table rows */}
        {availableSlots.length === 0 ? (
          <tr>
            <td colSpan={isAdmin ? 7 : 6} className="text-center text-muted">
              No available slots found.
            </td>
          </tr>
        ) : (
          // Map available slots to table rows
          availableSlots.map((slot) => (
            <tr key={slot.id}>
              <td>{slot.id}</td>
              {/* If admin, show Healthcare Worker Id column */}
              {isAdmin ? <td>{slot.healthcareWorkerId}</td> : null}
              <td>{formatDateOnly(slot.start)}</td>
              <td>{formatTimeOnly(slot.start)}</td>
              <td>{formatTimeOnly(slot.end)}</td>
              <td>{slot.isBooked ? <Badge bg="danger">Booked</Badge> : <Badge bg="success">Open</Badge>}</td>
              <td>
                {/* If slot is booked, disable update button */}
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
