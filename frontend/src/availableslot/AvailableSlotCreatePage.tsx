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
  const [loadingWorkers, setLoadingWorkers] = useState(false);

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
      try {
        const list = await HealthcareWorkerService.fetchAllHealthcareWorkers(); // ← implement in your service
        setWorkers(list);
      } catch (error) {
        console.error(error);
      }
    };
    fetchWorkers();
  }, [isAdmin]);

  return (
    <div>
      <h2>Create Available Slot</h2>
      <AvailableSlotForm onAvailableSlotChanged={handleAvailableSlotCreated} isAdmin={isAdmin} workers={workers} />
      {isAdmin && loadingWorkers && <Loading />}
    </div>
  );
};

export default AvailableSlotCreatePage;
