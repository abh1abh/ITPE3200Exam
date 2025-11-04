import React, { useEffect, useState } from "react";
import { AvailableSlot } from "../types/AvailableSlot";
import * as AvailableSlotService from "./AvailableSlotService";
import { useNavigate, useParams } from "react-router-dom";
import Loading from "../shared/Loading";
import { useAuth } from "../auth/AuthContext";
import AvailableSlotForm from "./AvailableSlotForm";

const AvailableSlotUpdatePage: React.FC = () => {
  const { slotId } = useParams<{ slotId: string }>();
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [availableSlot, setAvailableSlot] = useState<AvailableSlot | null>(null);
  const { hasRole } = useAuth();
  const isAdmin = hasRole("Admin");
  const navigate = useNavigate();

  useEffect(() => {
    const fetchAvailableSlot = async () => {
      setLoading(true);

      try {
        const slot = await AvailableSlotService.fetchAvailableSlot(Number(slotId));
        setAvailableSlot(slot);
      } catch (error) {
        console.error(error);
        setError("Failed to fetch available slot. Please try again later.");
      } finally {
        setLoading(false);
      }
    };
    fetchAvailableSlot();
  }, [slotId]);

  const handleSlotUpdate = async (updatedSlot: AvailableSlot) => {
    try {
      const data = await AvailableSlotService.updateAvailableSlot(Number(slotId), updatedSlot);
      navigate("/availableslot"); // Navigate back to the slot page after update
    } catch (error) {
      console.error("Error updating available slot:", error);
    }
  };

  if (error) return <div>{error}</div>;

  return (
    <div>
      <h2 className="mb-4">Update available slot</h2>

      {loading ? (
        <Loading />
      ) : (
        <AvailableSlotForm
          availableSlotId={availableSlot?.id}
          initialData={availableSlot ?? undefined}
          isUpdating={true}
          onAvailableSlotChanged={handleSlotUpdate}
          isAdmin={isAdmin}
        />
      )}
    </div>
  );
};

export default AvailableSlotUpdatePage;
