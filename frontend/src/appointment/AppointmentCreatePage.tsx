import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import * as AppointmentService from "./appointmentService";
import { Appointment } from "../types/appointment";
import * as ClientService from "../client/clientService";
import * as availableSlotService from "../availableslot/availableSlotService";

import { Client } from "../types/client";
import { AvailableSlot } from "../types/availableSlot";
import { useAuth } from "../auth/AuthContext";
import Loading from "../shared/Loading";
import CreateAppointmentForm from "./CreateAppointmentForm";
import { Alert } from "react-bootstrap";

const AppointmentCreatePage: React.FC = () => {
  const [clients, setClients] = useState<Client[]>([]);
  const [availableSlots, setAvailableSlots] = useState<AvailableSlot[]>([]);

  const [isFetching, setIsFetching] = useState(false);
  const [fetchError, setFetchError] = useState<string | null>(null);

  const [isSubmitting, setIsSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);

  const navigate = useNavigate();

  const { hasRole } = useAuth();
  const isAdmin = hasRole("Admin");

  const handleAppointmentCreated = async (appointment: Appointment) => {
    setSubmitError(null);
    setIsSubmitting(true);
    try {
      // TODO: Implement at service layer
      // const token = localStorage.getItem("token");
      // if (!token) throw new Error("No token log in again!");

      const data = await AppointmentService.createAppointment(appointment);
      console.log("Created:", data);
      navigate("/appointment");
    } catch (error: any) {
      console.error("Error creating appointment: ", error);
      setSubmitError("Could not create appointment. Try again later");
    } finally {
      setIsSubmitting(false);
    }
  };

  useEffect(() => {
    const fetchData = async () => {
      setFetchError(null);
      setIsFetching(true);
      try {
        if (isAdmin) {
          const clientData = await ClientService.fetchAllClients();
          setClients(clientData);
        }
        const availableSlotsData = await availableSlotService.fetchAllUnbookedAvailableSlots();
        setAvailableSlots(availableSlotsData);
      } catch (error: any) {
        console.error(error);
        setFetchError("Failed to fetch Clients for Admin. Try again later");
      } finally {
        setIsFetching(false);
      }
    };
    fetchData();
  }, [isAdmin]);

  return (
    <div style={{ padding: "20px" }}>
      <h2>Create New Appointment</h2>
      {isFetching ? (
        <Loading />
      ) : fetchError ? (
        <Alert variant="danger" className="mt-3">
          {fetchError}
        </Alert>
      ) : (
        <CreateAppointmentForm
          onAppointmentChanged={handleAppointmentCreated}
          clients={clients}
          unbookedSlots={availableSlots}
          isAdmin={isAdmin}
          submitError={submitError}
          isSubmitting={isSubmitting}
        />
      )}
    </div>
  );
};

export default AppointmentCreatePage;
