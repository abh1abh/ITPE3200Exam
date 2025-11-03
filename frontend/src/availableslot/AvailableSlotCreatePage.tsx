import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { AvailableSlot } from "../types/AvailableSlot";
import * as AvailableSlotService from "./AvailableSlotService";
import AvailableSlotForm from "./AvailableSlotForm";
import { useAuth } from "../auth/AuthContext";
import { HealthcareWorker } from "../types/HealthcareWorker";
import * as HealthcareWorkerService from "../shared/HealthcareWorkerService";
import Loading from "../shared/Loading";

const AvailableSlotCreatePage: React.FC = () => {
  const navigate = useNavigate();
  const { hasRole } = useAuth();
  const [workers, setWorkers] = useState<HealthcareWorker[]>([]);
  const [loading, setLoading] = useState(false);

  const isAdmin: boolean = hasRole("Admin");

  const handleAvailableSlotCreated = async (availableSlot: AvailableSlot) => {
    try {
      const data = await AvailableSlotService.createAvailableSlot(availableSlot);
      console.log("Available slot created:", data);
      navigate("/availableslot"); // Navigate back to the item list page after creation
    } catch (error) {
      console.error("Error creating available slot:", error);
      // Handle error (e.g., show error message to user)
    }
  };

  // Fetch workers if admin
  useEffect(() => {
    const fetchWorkers = async () => {
      if (!isAdmin) return;
      setLoading(true);
      try {
        const list = await HealthcareWorkerService.fetchAllHealthcareWorkers(); // ← implement in your service
        setWorkers(list);
      } catch (error) {
        console.error(error);
      } finally {
        setLoading(false);
      }
    };
    fetchWorkers();
  }, [isAdmin]);

  return (
    <div>
      <h2>Create Available Slot</h2>
      {loading ? (
        <Loading />
      ) : (
        <AvailableSlotForm
          onAvailableSlotChanged={handleAvailableSlotCreated}
          isAdmin={isAdmin}
          workers={workers}
        />
      )}
    </div>
  );
};

export default AvailableSlotCreatePage;
