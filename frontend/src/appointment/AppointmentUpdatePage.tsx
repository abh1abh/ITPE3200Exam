import React, { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import * as appointmentService from "./appointmentService";
import { Appointment, AppointmentView } from "../types/appointment";
import UpdateAppointmentForm from "./UpdateAppointmentForm";
import { useAuth } from "../auth/AuthContext";
import Loading from "../shared/Loading";
import { Alert } from "react-bootstrap";

const AppointmentUpdatePage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [appointment, setAppointment] = useState<AppointmentView | null>(null);
  const { hasRole } = useAuth();

  const isAdmin = hasRole("Admin");
  const isClient = hasRole("Client");
  const isWorker = hasRole("HealthcareWorker");

  const [loading, setLoading] = useState<boolean>(true);
  const [fetchError, setFetchError] = useState<string | null>(null);
  const [submitError, setSubmitError] = useState<string | null>(null);

  useEffect(() => {
    const fetchAppointment = async () => {
      try {
        const data = await appointmentService.fetchAppointmentById(Number(id));
        setAppointment(data);
      } catch (error) {
        console.error("Error fetching appointment:", error);
        setFetchError("Failed to fetch appointment data.");
      } finally {
        setLoading(false);
      }
    };
    if (id) fetchAppointment();
  }, [id]);

  const handleAppointmentUpdated = async (updated: Appointment) => {
    try {
      await appointmentService.updateAppointment(Number(id), updated);
      console.log("Updated successfully");
      navigate("/appointment");
    } catch (error) {
      console.error("error update appointment:", error);
      setSubmitError("Failed to update appointment.");
    }
  };

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
