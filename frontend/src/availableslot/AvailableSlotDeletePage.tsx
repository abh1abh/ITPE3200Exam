import React, { useEffect, useState } from "react";
import DeleteAvailableSlotModel from "./AvailableSlotDeleteModal";
import { useNavigate, useParams } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { AvailableSlot } from "../types/AvailableSlot";
import * as AvailableSlotService from "./AvailableSlotService";

const AvailableSlotDeletePage: React.FC = () => {
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

  const onDelete = async () => {
    try {
      await AvailableSlotService.deleteAvailableSlot(Number(slotId));
      navigate("/availableslot"); // Navigate back to the item list page after creation
    } catch (error) {
      console.error("Error deleting available slot:", error);
      // Handle error (e.g., show error message to user)
    }
  };

  return (
    <div>
      <h2>Delete Available Slot</h2>
      {availableSlot && (
        <DeleteAvailableSlotModel
          availableSlot={availableSlot}
          onCancel={() => navigate(-1)}
          onConfirm={onDelete}
        />
      )}
    </div>
  );
};

export default AvailableSlotDeletePage;
