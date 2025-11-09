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
  const navigate = useNavigate();
  const { hasRole } = useAuth();
  const [workers, setWorkers] = useState<HealthcareWorker[]>([]);

  const [isFetching, setIsFetching] = useState(false);
  const [fetchError, setFetchError] = useState<string | null>(null);

  const [isSubmitting, setIsSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);

  const isAdmin: boolean = hasRole("Admin");

  const handleAvailableSlotCreated = async (availableSlot: AvailableSlot) => {
    setSubmitError(null);
    setIsSubmitting(true);
    try {
      const data = await availableSlotService.createAvailableSlot(availableSlot);
      console.log("Available slot created:", data);
      navigate("/availableslot"); // Navigate back to the item list page after creation
    } catch (error: any) {
      console.error("Error creating available slot:", error);
      setSubmitError("Failed to create available slot. Try again later");
    } finally {
      setIsSubmitting(false);
    }
  };

  // Fetch workers if admin
  useEffect(() => {
    const fetchWorkers = async () => {
      if (!isAdmin) return;
      setIsFetching(true);
      try {
        const list = await healthcareWorkerService.fetchAllWorkers();
        setWorkers(list);
      } catch (error) {
        console.error(error);
        setFetchError("Failed to load healthcare workers. Try again later.");
      } finally {
        setIsFetching(false);
      }
    };
    fetchWorkers();
  }, [isAdmin]);

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
