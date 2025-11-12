import React, { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import * as appointmentService from "./appointmentService";
import { Appointment, AppointmentView } from "../types/appointment";
import UpdateAppointmentForm from "./UpdateAppointmentForm";
import { useAuth } from "../auth/AuthContext";
import Loading from "../shared/Loading";
import { Alert } from "react-bootstrap";

const AppointmentUpdatePage: React.FC = () => {
  // Get appointment id from URL parameters
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  // Appointment state
  const [appointment, setAppointment] = useState<AppointmentView | null>(null);

  // Get user roles
  const { hasRole } = useAuth();
  const isAdmin = hasRole("Admin");
  const isClient = hasRole("Client");
  const isWorker = hasRole("HealthcareWorker");

  // Loading and error states
  const [loading, setLoading] = useState<boolean>(true);
  const [fetchError, setFetchError] = useState<string | null>(null);
  const [submitError, setSubmitError] = useState<string | null>(null);

  // Fetch appointment data on component mount, or when id changes
  useEffect(() => {
    const fetchAppointment = async () => {
      // Set loading state and clear previous errors
      setLoading(true);
      setFetchError(null);
      try {
        // Fetch appointment data by ID
        const data = await appointmentService.fetchAppointmentById(Number(id));
        setAppointment(data);
      } catch (error) {
        // Log and set fetch error
        console.error("Error fetching appointment:", error);
        setFetchError("Failed to fetch appointment data.");
      } finally {
        // Clear loading state
        setLoading(false);
      }
    };
    if (id) fetchAppointment();
  }, [id]);

  // Handle appointment update submission
  const handleAppointmentUpdated = async (updated: Appointment) => {
    // Clear previous submission errors
    setSubmitError(null);
    try {
      // Call service to update appointment
      await appointmentService.updateAppointment(Number(id), updated);
      console.log("Updated successfully");
      navigate("/appointment");
    } catch (error) {
      // Log and set submission error
      console.error("Error updating appointment:", error);
      setSubmitError("Failed to update appointment.");
    }
  };

  // Render component. If loading, show loading. If no appointment, show warning. If fetch error, show error. Otherwise show update form.
  return (
    <div>
      <h2>Update Appointment</h2>
      {loading ? (
        <Loading />
      ) : !appointment ? (
        <Alert variant="warning" className="mt-3">
          No appointment found.
        </Alert>
      ) : fetchError ? (
        <Alert variant="danger" className="mt-3">
          {fetchError}
        </Alert>
      ) : (
        // Show update appointment form. Pass down necessary props like initial data, submission state, submit error and handlers.
        <UpdateAppointmentForm
          initialData={appointment}
          onAppointmentChanged={handleAppointmentUpdated}
          serverError={submitError}
          isAdmin={isAdmin}
          isClient={isClient}
          isWorker={isWorker}
        />
      )}
    </div>
  );
};
export default AppointmentUpdatePage;
