import React, { useEffect, useState } from "react";
import { AvailableSlot } from "../types/availableSlot";
import * as availableSlotService from "./availableSlotService";
import { useNavigate, useParams } from "react-router-dom";
import Loading from "../shared/Loading";
import { useAuth } from "../auth/AuthContext";
import AvailableSlotForm from "./AvailableSlotForm";
import { Alert } from "react-bootstrap";

const AvailableSlotUpdatePage: React.FC = () => {
  const { slotId } = useParams<{ slotId: string }>();
  const [availableSlot, setAvailableSlot] = useState<AvailableSlot | null>(null);

  const [isFetching, setIsFetching] = useState(false);
  const [fetchError, setFetchError] = useState<string | null>(null);

  const [isSubmitting, setIsSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);

  const { hasRole } = useAuth();
  const isAdmin = hasRole("Admin");
  const navigate = useNavigate();

  useEffect(() => {
    const fetchAvailableSlot = async () => {
      setFetchError(null);
      setIsFetching(true);

      try {
        const slot = await availableSlotService.fetchAvailableSlot(Number(slotId));
        setAvailableSlot(slot);
      } catch (error) {
        console.error(error);
        setFetchError("Failed to fetch available slot. Please try again later.");
      } finally {
        setIsFetching(false);
      }
    };
    fetchAvailableSlot();
  }, [slotId]);

  const handleSlotUpdate = async (updatedSlot: AvailableSlot) => {
    setSubmitError(null);
    setIsSubmitting(true);
    try {
      const data = await availableSlotService.updateAvailableSlot(Number(slotId), updatedSlot);
      navigate("/availableslot"); // Navigate back to the slot page after update
    } catch (error) {
      console.error("Error updating available slot:", error);
      setSubmitError("Failed to update available slot. Try again later");
    } finally {
      setIsSubmitting(false);
    }
  };

  // if (error) return <div>{error}</div>;

  return (
    <div>
      <h2 className="mb-4">Update available slot</h2>
      {isFetching ? (
        <Loading />
      ) : fetchError ? (
        <Alert variant="danger" className="mt-3">
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
