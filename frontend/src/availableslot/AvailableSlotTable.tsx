import React from "react";
import { Badge, Table } from "react-bootstrap";
import { AvailableSlot } from "../types/AvailableSlot";

interface Props {
  availableSlots: AvailableSlot[];
  isAdmin: boolean;
}
const AvailableSlotTable: React.FC<Props> = ({ availableSlots, isAdmin }) => {
  const formatDate = (d: string | Date) =>
    new Date(d).toLocaleString(undefined, {
      year: "numeric",
      month: "short",
      day: "2-digit",
      hour: "2-digit",
      minute: "2-digit",
    });

  return (
    <Table striped bordered hover responsive>
      <thead>
        <tr>
          <th>#</th>
          {isAdmin ? <th>Healthcare Worker Id</th> : null}
          <th>Start</th>
          <th>End</th>
          <th>Status</th>
        </tr>
      </thead>
      <tbody>
        {availableSlots.length === 0 ? (
          <tr>
            <td colSpan={5} className="text-center text-muted">
              No available slots found.
            </td>
          </tr>
        ) : (
          availableSlots.map((slot) => (
            <tr key={slot.id}>
              <td>{slot.id}</td>
              {isAdmin ? <td>{slot.healthcareWorkerId}</td> : null}
              <td>{formatDate(slot.start)}</td>
              <td>{formatDate(slot.end)}</td>
              <td>{slot.isBooked ? <Badge bg="danger">Booked</Badge> : <Badge bg="success">Open</Badge>}</td>
            </tr>
          ))
        )}
      </tbody>
    </Table>
  );
};

export default AvailableSlotTable;
