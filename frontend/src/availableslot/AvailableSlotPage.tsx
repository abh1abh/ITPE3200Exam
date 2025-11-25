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
  // State for available slots, loading, errors, and deletion
  const [availableSlots, setAvailableSlots] = useState<AvailableSlot[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [toDelete, setToDelete] = useState<AvailableSlot | null>(null);
  const [isDeleting, setIsDeleting] = useState<boolean>(false);

  // Get user role function
  const { hasRole } = useAuth();

  // Function to fetch available slots based on user role
  const fetchAvailableSlots = async () => {
    // Start loading and reset errors
    setLoading(true);
    setError(null);
    try {
      // Determine which slots to fetch based on role
      let data;
      if (hasRole("HealthcareWorker")) {
        data = await availableSlotService.fetchAllAvailableSlotsMine();
      } else {
        data = await availableSlotService.fetchAllAvailableSlots();
      }
      setAvailableSlots(data);
    } catch (error) {
      // Log and set error message if fetching fails
      console.error("Error fetching available slots:", error);
      setError("Failed to fetch available slots. Please try again later.");
    } finally {
      // Stop loading
      setLoading(false);
    }
  };

  // Fetch available slots on component mount.
  useEffect(() => {
    fetchAvailableSlots();
  }, []);

  // Function to confirm deletion of an available slot
  const confirmDelete = async () => {
    // Ensure there is a slot to delete
    if (!toDelete?.id) return;
    // Reset errors and start deletion process
    setError(null);
    setIsDeleting(true);
    try {
      // Call service to delete the slot
      await availableSlotService.deleteAvailableSlot(toDelete.id);
      fetchAvailableSlots();
      // Clear the slot to delete
      setToDelete(null);
    } catch (error) {
      // Log and set error message if deletion fails
      console.error("Error deleting slot: ", error);
      setError("Error deleting slot");
      setToDelete(null);
    } finally {
      // Stop deletion process
      setIsDeleting(false);
    }
  };

  // Render component. If loading, show loading. If error, show error. Otherwise show slots table.
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
