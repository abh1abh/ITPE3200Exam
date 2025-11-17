import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { AvailableSlot } from "../types/availableSlot";
import * as availableSlotService from "./availableSlotService";
import AvailableSlotForm from "./AvailableSlotForm";
import { useAuth } from "../auth/AuthContext";
import { HealthcareWorker } from "../types/healthcareWorker";
import Loading from "../shared/Loading";
import * as healthcareWorkerService from "../healtcareWorker/healthcareWorkerService";
import { Alert } from "react-bootstrap";

const AvailableSlotCreatePage: React.FC = () => {
  // Navigation hook
  const navigate = useNavigate();
  // Get user role
  const { hasRole } = useAuth();
  const isAdmin: boolean = hasRole("Admin");

  // State for healthcare workers, loading, and errors
  const [workers, setWorkers] = useState<HealthcareWorker[]>([]);
  const [isFetching, setIsFetching] = useState(false);
  const [fetchError, setFetchError] = useState<string | null>(null);

  // State for submission
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);

  // Handle available slot creation
  const handleAvailableSlotCreated = async (availableSlot: AvailableSlot) => {
    // Reset submission state and errors
    setSubmitError(null);
    setIsSubmitting(true);
    try {
      // Call service to create available slot
      await availableSlotService.createAvailableSlot(availableSlot);
      navigate("/availableslot"); // Navigate back to the item list page after creation
    } catch (error: any) {
      // Log and set error message if creation fails
      console.error("Error creating available slot:", error);
      setSubmitError("Failed to create available slot. Try again later");
    } finally {
      // Reset submission state
      setIsSubmitting(false);
    }
  };

  // Fetch workers if admin
  useEffect(() => {
    const fetchWorkers = async () => {
      // Check if user is admin
      if (!isAdmin) return;
      // Start fetching and reset errors
      setFetchError(null);
      setIsFetching(true);
      try {
        // Fetch all healthcare workers
        const list = await healthcareWorkerService.fetchAllWorkers();
        setWorkers(list);
      } catch (error) {
        // Log and set error message if fetching fails
        console.error(error);
        setFetchError("Failed to load healthcare workers. Try again later.");
      } finally {
        // Stop fetching
        setIsFetching(false);
      }
    };
    fetchWorkers();
  }, [isAdmin]);

  // Render component. If fetching, show loading. If fetch error, show error. Otherwise show create form.
  return (
    <div>
      <h2>Create Available Slot</h2>
      {isFetching ? (
        <Loading />
      ) : fetchError ? (
        <Alert variant="danger" className="mt-3">
          {fetchError}
        </Alert>
      ) : (
        <AvailableSlotForm
          onAvailableSlotChanged={handleAvailableSlotCreated}
          isAdmin={isAdmin}
          workers={workers}
          serverError={submitError}
          isSubmitting={isSubmitting}
        />
      )}
    </div>
  );
};

export default AvailableSlotCreatePage;
