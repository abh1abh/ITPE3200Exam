import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import * as AppointmentService from "./appointmentService";
import { Appointment } from "../types/appointment";
import * as ClientService from "../client/clientService";
import * as AvailableSlotService from "../availableslot/availableSlotService";
import { Client } from "../types/client";
import { AvailableSlot } from "../types/availableSlot";
import { useAuth } from "../auth/AuthContext";
import Loading from "../shared/Loading";
import CreateAppointmentForm from "./CreateAppointmentForm";

const AppointmentCreatePage: React.FC = () => {
  const [clients, setClients] = useState<Client[]>([]);
  const [availableSlots, setAvailableSlots] = useState<AvailableSlot[]>([]);
  const [serverError, setServerError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();

  const { hasRole } = useAuth();
  const isAdmin = hasRole("Admin");

  const handleAppointmentCreated = async (appointment: Appointment) => {
    setServerError(null);
    setLoading(true);
    try {
      // TODO: Implement at service layer
      // const token = localStorage.getItem("token");
      // if (!token) throw new Error("No token log in again!");

      const data = await AppointmentService.createAppointment(appointment);
      console.log("Created:", data);
      navigate("/appointment");
    } catch (error: any) {
      console.error("Error creating appointment: ", error);
      setServerError("Could not create appointment. Try again later");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    const fetchData = async () => {
      setServerError(null);
      setLoading(true);
      try {
        if (isAdmin) {
          const clientData = await ClientService.fetchAllClients();
          setClients(clientData);
        }
        const availableSlotsData = await AvailableSlotService.fetchAllUnbookedAvailableSlots();
        setAvailableSlots(availableSlotsData);
      } catch (error: any) {
        console.error(error);
        setServerError("Failed to fetch Clients for Admin. Try again later");
      } finally {
        setLoading(false);
      }
    };
    fetchData();
  }, [isAdmin]);

  return (
    <div style={{ padding: "20px" }}>
      <h2>Create New Appointment</h2>
      {loading ? (
        <Loading />
      ) : (
        <CreateAppointmentForm
          onAppointmentChanged={handleAppointmentCreated}
          clients={clients}
          unbookedSlots={availableSlots}
          isAdmin={isAdmin}
          serverError={serverError}
        />
      )}
    </div>
  );
};

export default AppointmentCreatePage;
