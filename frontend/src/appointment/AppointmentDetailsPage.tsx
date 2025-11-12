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
  // Get appointment ID from URL params
  const { id } = useParams<{ id: string }>();
  // State for appointment data, loading and error
  const [appointment, setAppointment] = useState<AppointmentView | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  // State for delete modal and delete operation
  const [showDelete, setShowDelete] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);
  const [deleteError, setDeleteError] = useState<string | null>(null);

  // Get user roles
  const { hasRole } = useAuth();
  const navigate = useNavigate();

  // Determine roles
  const isAdmin = hasRole("Admin");
  const isClient = hasRole("Client");
  const isWorker = hasRole("HealthcareWorker");

  // Fetch appointment data on mount and when id changes
  useEffect(() => {
    const fetchAppointment = async () => {
      try {
        // Call service to fetch appointment by id
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

  // Confirm delete appointment
  const confirmDelete = async () => {
    // If no appointment to delete, return
    if (!appointment?.id) return;
    // Start loading and clear previous errors
    setDeleteError(null);
    setIsDeleting(true);
    try {
      // Call deleteAppointment service
      await appointmentService.deleteAppointment(appointment.id);
      navigate("/appointment");
    } catch (e) {
      // If error occurs during delete
      console.error("Error deleting appointment:", e);
      setDeleteError("Failed to delete appointment. Try again later.");
      setShowDelete(false);
    } finally {
      setIsDeleting(false); // Stop loading delete state
    }
  };

  // Render component. If loading, show loading spinner. If error, show error alert. Otherwise show appointment details.
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

          {/* Navigation buttons */}
          <div className="d-flex justify-content-center gap-2">
            <Link to="/appointment" className="btn btn-secondary">
              Back
            </Link>
            <Link to={`/appointment/${appointment!.id}/changelog`} className="btn btn-outline-primary">
              View Changes
            </Link>
            <Link to={`/appointment/${appointment.id}/update`} className="btn btn-primary ">
              Edit
            </Link>
            <Button variant="danger" onClick={() => setShowDelete(true)}>
              Cancel appointment
            </Button>
          </div>
          {/* Delete confirmation modal */}
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
