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
  // State for clients and available slots
  const [clients, setClients] = useState<Client[]>([]);
  const [availableSlots, setAvailableSlots] = useState<AvailableSlot[]>([]);

  // State for fetching and submission status/errors
  const [isFetching, setIsFetching] = useState(false);
  const [fetchError, setFetchError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);

  const navigate = useNavigate();

  // Get user roles
  const { hasRole } = useAuth();
  const isAdmin: boolean = hasRole("Admin");

  // Handle appointment creation
  const handleAppointmentCreated = async (appointment: Appointment) => {
    // Reset submission state and errors
    setSubmitError(null);
    setIsSubmitting(true);
    try {
      // Call service to create appointment
      const data = await AppointmentService.createAppointment(appointment);
      console.log("Created: ", data);
      navigate("/appointment");
    } catch (error: any) {
      // If error occurs during creation. Log and set error message
      console.error("Error creating appointment: ", error);
      setSubmitError("Could not create appointment. Try again later");
    } finally {
      // Reset submission state
      setIsSubmitting(false);
    }
  };
  // Fetch clients and available slots on mount. Admins need client list. Based on isAdmin dependency.
  useEffect(() => {
    // Fetch data function
    const fetchData = async () => {
      setFetchError(null);
      setIsFetching(true);
      try {
        // If admin, fetch all clients
        if (isAdmin) {
          const clientData = await ClientService.fetchAllClients();
          setClients(clientData);
        }
        // Fetch all unbooked available slots
        const availableSlotsData = await availableSlotService.fetchAllUnbookedAvailableSlots();
        setAvailableSlots(availableSlotsData);
      } catch (error: any) {
        // If error occurs during fetch. Log and set error message
        console.error(error);
        setFetchError("Failed to fetch Clients for Admin. Try again later");
      } finally {
        // Reset fetching state
        setIsFetching(false);
      }
    };
    fetchData();
  }, [isAdmin]);

  // Render component. If fetching, show loading. If fetch error, show error. Otherwise show create form.
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
        // Show create appointment form. Pass down necessary props like clients, available slots, submission state, submit error and handlers.
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
