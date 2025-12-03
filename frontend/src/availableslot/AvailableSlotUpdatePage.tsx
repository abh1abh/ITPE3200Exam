import React, { useEffect, useState } from "react";
import { AvailableSlot } from "../types/availableSlot";
import * as availableSlotService from "./availableSlotService";
import { useNavigate, useParams } from "react-router-dom";
import Loading from "../shared/Loading";
import { useAuth } from "../auth/AuthContext";
import AvailableSlotForm from "./AvailableSlotForm";
import { Alert } from "react-bootstrap";

const AvailableSlotUpdatePage: React.FC = () => {
  // Get slot ID from URL parameters
  const { slotId } = useParams<{ slotId: string }>();

  // State for available slot, fetching, errors, and submission
  const [availableSlot, setAvailableSlot] = useState<AvailableSlot | null>(null);
  const [isFetching, setIsFetching] = useState<boolean>(false);
  const [fetchError, setFetchError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState<boolean>(false);
  const [submitError, setSubmitError] = useState<string | null>(null);

  // Get user role
  const { hasRole } = useAuth();
  const isAdmin = hasRole("Admin");
  const navigate = useNavigate();

  // Fetch available slot data on mount or when slotId changes
  useEffect(() => {
    const fetchAvailableSlot = async () => {
      // Start fetching and reset errors
      setFetchError(null);
      setIsFetching(true);

      try {
        // Call service to fetch available slot by ID
        const slot = await availableSlotService.fetchAvailableSlot(Number(slotId));
        setAvailableSlot(slot);
      } catch (error) {
        // Log and set fetch error
        console.error(error);
        setFetchError("Failed to fetch available slot. Please try again later.");
      } finally {
        // Stop fetching
        setIsFetching(false);
      }
    };
    fetchAvailableSlot();
  }, [slotId]);

  // Handle available slot update submission
  const handleSlotUpdate = async (updatedSlot: AvailableSlot) => {
    // Reset submission state and errors
    setSubmitError(null);
    setIsSubmitting(true);
    try {
      // Call service to update available slots
      const data = await availableSlotService.updateAvailableSlot(Number(slotId), updatedSlot);
      navigate("/availableslot"); // Navigate back to the slot page after update
    } catch (error) {
      // Log and set submission error
      console.error("Error updating available slot:", error);
      setSubmitError("Failed to update available slot. Try again later");
    } finally {
      // Reset submission state
      setIsSubmitting(false);
    }
  };

  // Render component. If fetching, show loading. If fetch error, show error. Otherwise show update form.
  return (
    <div>
      <h2 className="mb-4">Update available slot</h2>
      {isFetching ? (
        <Loading />
      ) : fetchError ? (
        <Alert variant="warning" className="mt-3">
          {fetchError}
        </Alert>
      ) : (
        <AvailableSlotForm
          availableSlotId={availableSlot?.id}
          initialData={availableSlot ?? undefined}
          isUpdating={true}
          onAvailableSlotChanged={handleSlotUpdate}
          isAdmin={isAdmin}
          serverError={submitError}
          isSubmitting={isSubmitting}
        />
      )}
    </div>
  );
};

export default AvailableSlotUpdatePage;
