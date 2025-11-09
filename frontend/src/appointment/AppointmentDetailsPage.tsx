import React, { useEffect, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import * as appointmentService from "./appointmentService";
import { AppointmentView } from "../types/appointment";
import ViewAppointmentCard from "./ViewAppointmentCard";
import { useAuth } from "../auth/AuthContext";
import { Alert, Button } from "react-bootstrap";
import Loading from "../shared/Loading";

const AppointmentDetailsPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const [appointment, setAppointment] = useState<AppointmentView | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const { hasRole } = useAuth();
  const navigate = useNavigate();
  const onCancel = () => {
    navigate(-1);
  };

  const isAdmin = hasRole("Admin");
  const isClient = hasRole("Client");
  const isWorker = hasRole("HealthcareWorker");

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

  return (
    <div>
      <h2>Appointment Details</h2>
      {loading ? (
        <Loading />
      ) : !appointment ? (
        <Alert variant="warning" className="mt-3">
          No appointment found.
        </Alert>
      ) : error ? (
        <Alert variant="danger" className="mt-3">
          {error}
        </Alert>
      ) : (
        <>
          <ViewAppointmentCard
            initialData={appointment}
            isAdmin={isAdmin}
            isClient={isClient}
            isWorker={isWorker}
          />
          <div className="d-flex justify-content-center gap-2">
            <Link to={`/appointment/${appointment.id}/update`} className="btn btn-primary ">
              Edit
            </Link>

            <Button variant="outline-danger">Delete</Button>

            <Button variant="secondary" onClick={onCancel}>
              Back
            </Button>
          </div>
        </>
      )}
    </div>
  );
};

export default AppointmentDetailsPage;
