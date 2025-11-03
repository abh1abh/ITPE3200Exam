import React, { useEffect, useState } from "react";
import { AvailableSlot } from "../types/AvailableSlot";
import { useAuth } from "../auth/AuthContext";
import * as AvailableSlotService from "./AvailableSlotService";
import { Alert, Badge, Spinner, Table } from "react-bootstrap";

const formatDate = (d: string | Date) =>
  new Date(d).toLocaleString(undefined, {
    year: "numeric",
    month: "short",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
  });

const AvailableSlotPage: React.FC = () => {
  const [availableSlots, setAvailableSlots] = useState<AvailableSlot[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const { hasRole } = useAuth();

  const fetchAvailableSlots = async () => {
    setLoading(true);
    setError(null);
    try {
      let data;
      if (hasRole("HealthcareWorker")) {
        data = await AvailableSlotService.fetchAllAvailableSlotsMine();
      } else {
        data = await AvailableSlotService.fetchAllAvailableSlots();
      }
      setAvailableSlots(data);
      console.log(data);
    } catch (error: unknown) {
      if (error instanceof Error) {
        console.error("Error fetching available slots:", error.message);
      } else {
        console.error("Unknown error ", error);
      }
      setError("Failed to fetch available slots. Please try again later.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchAvailableSlots();
  }, []);

  return (
    <div>
      <h2>Available Slots</h2>
      {loading && (
        <div className="d-flex align-items-center gap-2">
          <Spinner animation="border" role="status" />
          <span>Loading…</span>
        </div>
      )}

      {!loading && error && <Alert variant="danger">{error}</Alert>}

      {!loading && !error && (
        <Table striped bordered hover responsive>
          <thead>
            <tr>
              <th>#</th>
              <th>Healthcare Worker</th>
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
                  <td>{slot.healthcareWorkerId}</td>
                  <td>{formatDate(slot.start)}</td>
                  <td>{formatDate(slot.end)}</td>
                  <td>{slot.isBooked ? <Badge bg="danger">Booked</Badge> : <Badge bg="success">Open</Badge>}</td>
                </tr>
              ))
            )}
          </tbody>
        </Table>
      )}
    </div>
  );
};

export default AvailableSlotPage;
