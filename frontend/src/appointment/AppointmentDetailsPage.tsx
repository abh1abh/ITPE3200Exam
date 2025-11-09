import React, { useEffect, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import * as appointmentService from "./appointmentService";
import { AppointmentView } from "../types/appointment";
import ViewAppointmentCard from "./ViewAppointmentCard";
import { useAuth } from "../auth/AuthContext";
import { Alert, Button } from "react-bootstrap";
import Loading from "../shared/Loading";
import AppointmentDeleteModal from "./AppointmentDeleteModal";

const AppointmentDetailsPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const [appointment, setAppointment] = useState<AppointmentView | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  const [showDelete, setShowDelete] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);
  const [deleteError, setDeleteError] = useState<string | null>(null);

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

  const confirmDelete = async () => {
    if (!appointment?.id) return;
    setDeleteError(null);
    setIsDeleting(true);
    try {
      await appointmentService.deleteAppointment(appointment.id);
      navigate("/appointment");
    } catch (e) {
      console.error("Error deleting appointment:", e);
      setDeleteError("Failed to delete appointment. Try again later.");
      setShowDelete(false);
    } finally {
      setIsDeleting(false);
    }
  };

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
          {deleteError && (
            <Alert variant="danger" className="mt-3">
              {deleteError}
            </Alert>
          )}

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

            <Button variant="outline-danger" onClick={() => setShowDelete(true)}>
              Delete
            </Button>

            <Button variant="secondary" onClick={onCancel}>
              Back
            </Button>
          </div>

          {showDelete && (
            <AppointmentDeleteModal
              appointment={appointment}
              onCancel={() => setShowDelete(false)}
              onConfirm={confirmDelete}
              isDeleting={isDeleting}
            />
          )}
        </>
      )}
    </div>
  );
};

export default AppointmentDetailsPage;
