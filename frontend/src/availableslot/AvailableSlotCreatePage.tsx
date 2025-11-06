import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { AvailableSlot } from "../types/availableSlot";
import * as AvailableSlotService from "./availableSlotService";
import AvailableSlotForm from "./AvailableSlotForm";
import { useAuth } from "../auth/AuthContext";
import { HealthcareWorker } from "../types/healthcareWorker";
import Loading from "../shared/Loading";
import * as healthcareWorkerService from "../healtcareWorker/healthcareWorkerService";

const AvailableSlotCreatePage: React.FC = () => {
  const navigate = useNavigate();
  const { hasRole } = useAuth();
  const [workers, setWorkers] = useState<HealthcareWorker[]>([]);
  const [loading, setLoading] = useState(false);
  const [serverError, setServerError] = useState<string | null>(null);

  const isAdmin: boolean = hasRole("Admin");

  const handleAvailableSlotCreated = async (availableSlot: AvailableSlot) => {
    setServerError(null);
    setLoading(true);
    try {
      const data = await AvailableSlotService.createAvailableSlot(availableSlot);
      console.log("Available slot created:", data);
      navigate("/availableslot"); // Navigate back to the item list page after creation
    } catch (error: any) {
      console.error("Error creating available slot:", error);
      setServerError("Failed to create available slot. Try again later");
    } finally {
      setLoading(false);
    }
  };

  // Fetch workers if admin
  useEffect(() => {
    const fetchWorkers = async () => {
      if (!isAdmin) return;
      setLoading(true);
      try {
        const list = await healthcareWorkerService.fetchAllWorkers();
        setWorkers(list);
      } catch (error) {
        console.error(error);
      } finally {
        setLoading(false);
      }
    };
    fetchWorkers();
  }, [isAdmin]);

  console.log(workers);

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
          serverError={serverError}
        />
      )}
    </div>
  );
};

export default AvailableSlotCreatePage;
