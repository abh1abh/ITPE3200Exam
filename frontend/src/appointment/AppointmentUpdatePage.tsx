import React, { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import * as appointmentService from "./appointmentService";
import * as clientService from "../client/clientService";

import AppointmentForm from "./CreateAppointmentForm";
import { Appointment, AppointmentView } from "../types/appointment";
import { Client } from "../types/client";
import { HealthcareWorker } from "../types/healthcareWorker";
import UpdateAppointmentForm from "./UpdateAppointmentForm";

const AppointmentUpdatePage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [appointment, setAppointment] = useState<AppointmentView | null>(null);

  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  useEffect(() => {
    const fetchAppointment = async () => {
      try {
        const data = await appointmentService.fetchAppointmentById(Number(id));
        setAppointment(data);
      } catch (error) {
        console.error("Error fetching appointment:", error);
        setError("Failed to fetch appointment data");
      } finally {
        setLoading(false);
      }
    };
    if (id) fetchAppointment();
  }, [id]);

  console.log(appointment);

  const handleAppointmentUpdated = async (updated: Appointment) => {
    try {
      await appointmentService.updateAppointment(Number(id), updated);
      console.log("Updated successfully");
      navigate("/appointment");
    } catch (error) {
      console.error("error update appointment:", error);
      setError("failed to update");
    }
  };

  if (error) return <p style={{ color: "red" }}>{error}</p>;
  if (loading) return <p>Loading....</p>;
  if (!appointment) return <p>No appointment found.</p>;
  return (
    <div>
      <h2>Update Appointment</h2>
      <UpdateAppointmentForm initialData={appointment} onAppointmentChanged={handleAppointmentUpdated} />
    </div>
  );
};
export default AppointmentUpdatePage;
