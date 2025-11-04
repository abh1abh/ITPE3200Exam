import React, { useEffect, useState } from "react";
import { AvailableSlot } from "../types/AvailableSlot";
import { useAuth } from "../auth/AuthContext";
import * as AvailableSlotService from "./AvailableSlotService";
import { Alert, Badge, Button, Container, Spinner, Table } from "react-bootstrap";
import Loading from "../shared/Loading";
import DeleteAvailableSlotModal from "./AvailableSlotDeleteModal";
import AvailableSlotTable from "./AvailableSlotTable";

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
  const [toDelete, setToDelete] = useState<AvailableSlot | null>(null);
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
      {loading && <Loading />}
      <Container className="my-4">
        <Button href="/availableslot/create">Create new available slot</Button>
      </Container>

      {!loading && error && <Alert variant="danger">{error}</Alert>}

      {!loading && !error && (
        <>
          <AvailableSlotTable
            availableSlots={availableSlots}
            isAdmin={hasRole("Admin")}
            onDeleteClick={(slot: AvailableSlot) => setToDelete(slot)} // <--- NEW
          />
          {toDelete && (
            <DeleteAvailableSlotModal
              availableSlot={toDelete}
              onCancel={() => setToDelete(null)}
              onConfirm={async () => {
                await AvailableSlotService.deleteAvailableSlot(toDelete.id!);
                setToDelete(null);
                fetchAvailableSlots(); // refresh list
              }}
            />
          )}
        </>
      )}
    </div>
  );
};

export default AvailableSlotPage;
