import React, { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import * as AppointmentService from "./AppointmentService";

import AppointmentForm from "./AppointmentForm";
import { Appointment } from "../types/appointment";

const AppointmentUpdatePage: React.FC = () => {
  const { id } = useParams<{ id: string }>(); // Hent appointment ID fra URL
  const navigate = useNavigate();
  const [appointment, setAppointment] = useState<Appointment | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  useEffect(() => {
    const fetchAppointment = async () => {
      try {
        const token = localStorage.getItem("token");
        if (!token) throw new Error("try again, no token");
        const data = await AppointmentService.fetchAppointmentById(Number(id), token);
        setAppointment(data);
      } catch (error) {
        console.error("rror fetching appointment:", error);
        setError("failed to fetch");
      } finally {
        setLoading(false);
      }
    };
    if (id) fetchAppointment();
  }, [id]);
  const handleAppointmentUpdated = async (updated: Appointment) => {
    try {
      const token = localStorage.getItem("token");
      if (!token) throw new Error("No token!!!");
      await AppointmentService.updateAppointment(Number(id), updated, token);
      console.log("updated successfully");
      navigate("/appointments");
    } catch (error) {
      console.error("error update appointment:", error);
      setError("failed to update");
    }
  };
  if (error) return <p style={{ color: "red" }}>{error}</p>;
  if (loading) return <p>Loading....</p>;
  if (!appointment) return <p>No appointment found.</p>;
  return (
    <div style={{ padding: "20px" }}>
      <h2>Update Appointment</h2>
      <AppointmentForm onAppointmentChanged={handleAppointmentUpdated} initialData={appointment} isUpdate={true} />
    </div>
  );
};
export default AppointmentUpdatePage;
