import React, { useEffect, useState } from "react";
import { AvailableSlot } from "../types/availableSlot";
import { useAuth } from "../auth/AuthContext";
import * as availableSlotService from "./availableSlotService";
import { Alert, Container } from "react-bootstrap";
import Loading from "../shared/Loading";
import DeleteAvailableSlotModal from "./AvailableSlotDeleteModal";
import AvailableSlotTable from "./AvailableSlotTable";
import { Link } from "react-router-dom";

const AvailableSlotPage: React.FC = () => {
  const [availableSlots, setAvailableSlots] = useState<AvailableSlot[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [toDelete, setToDelete] = useState<AvailableSlot | null>(null);
  const [isDeleting, setIsDeleting] = useState<boolean>(false);
  const { hasRole } = useAuth();

  const fetchAvailableSlots = async () => {
    setLoading(true);
    setError(null);
    try {
      let data;
      if (hasRole("HealthcareWorker")) {
        data = await availableSlotService.fetchAllAvailableSlotsMine();
      } else {
        data = await availableSlotService.fetchAllAvailableSlots();
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

  const confirmDelete = async () => {
    if (!toDelete?.id) return;
    setError(null);
    setIsDeleting(true);
    try {
      await availableSlotService.deleteAvailableSlot(toDelete.id);
      fetchAvailableSlots();
      setToDelete(null);
    } catch (error) {
      console.error("Error deleting slot: ", error);
      setError("Error deleting slot");
      setToDelete(null);
    } finally {
      setIsDeleting(true);
    }
  };

  return (
    <div>
      <h2>Available Slots</h2>
      {loading && <Loading />}
      <Container className="my-4">
        <Link to="/availableslot/create" className="btn btn-primary">
          Create new available slot
        </Link>
      </Container>

      {!loading && error && <Alert variant="danger">{error}</Alert>}

      {!loading && (
        <>
          <AvailableSlotTable
            availableSlots={availableSlots}
            isAdmin={hasRole("Admin")}
            onDeleteClick={(slot: AvailableSlot) => setToDelete(slot)} // Find the specific slot to delete
          />
          {toDelete && (
            <DeleteAvailableSlotModal
              availableSlot={toDelete}
              onCancel={() => setToDelete(null)}
              onConfirm={confirmDelete}
              isDeleting={isDeleting}
            />
          )}
        </>
      )}
    </div>
  );
};

export default AvailableSlotPage;
