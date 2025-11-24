import React, { useEffect, useMemo, useState } from "react";
import { Alert, Button } from "react-bootstrap";
import { useAuth } from "../auth/AuthContext";
import AppointmentTable from "./AppointmentTable";
import { Appointment, AppointmentView } from "../types/appointment";
import * as appointmentService from "./appointmentService";
import { useNavigate } from "react-router-dom";
import AppointmentDeleteModal from "./AppointmentDeleteModal";
import Loading from "../shared/Loading";

const AppointmentPage: React.FC = () => {
  const [appointments, setAppointments] = useState<Appointment[]>([]);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  // Get user and roles
  const { user, hasRole } = useAuth();
  const isAdmin: boolean = hasRole("Admin");
  const isClient: boolean = hasRole("Client");
  const isWorker: boolean = hasRole("HealthcareWorker");
  const navigate = useNavigate();

  // Deleting state
  const [toDelete, setToDelete] = useState<AppointmentView | null>(null);
  const [isDeleting, setIsDeleting] = useState(false);

  // Fetch appointments based on role
  const fetchAppointments = async () => {
    // Start loading and clear previous errors
    setLoading(true);
    setError(null);
    try {
      // Fetch appointments based on role
      let data: AppointmentView[] = [];
      if (isAdmin) data = await appointmentService.fetchAppointments();
      else if (isClient) data = await appointmentService.fetchAppointmentsByClientId();
      else if (isWorker) data = await appointmentService.fetchAppointmentsByWorkerId();
      else {
        setError("No role matched.");
        return;
      }
      setAppointments(data);
    } catch (error: any) {
      // if error occurs during fetch
      console.error("Error fetching appointments: ", error);
      setError("Failed to fetch appointments.");
    } finally {
      // Set loading to false in both success and error cases
      setLoading(false);
    }
  };

  // Initial fetch, only once on mount (because of empty dependency array)
  useEffect(() => {
    fetchAppointments();
  }, []);

  // Sort most recent. useMemo so it only resorts when appointments change
  // const sortedAppointments = useMemo(
  //   () => [...appointments].sort((a, b) => new Date(a.start).getTime() - new Date(b.start).getTime()),
  //   [appointments]
  // );

  // Confirm delete appointment
  const confirmDelete = async () => {
    // If no appointment to delete, return
    if (!toDelete?.id) return;
    // Start loading and clear previous errors
    setError(null);
    setIsDeleting(true);
    try {
      // Call deleteAppointment service
      await appointmentService.deleteAppointment(toDelete.id);
      fetchAppointments();
      setToDelete(null);
    } catch (error) {
      // If error occurs during delete
      console.error("Error deleting appointment: ", error);
      setError("Error deleting appointment");
      setToDelete(null);
    } finally {
      setIsDeleting(false); // Stop loading delete state
    }
  };
  // Here we render the component. If loading, show loading spinner. If error, show error alert.
  // Otherwise show the appointment table with data and delete modal if needed.
  return (
    <div>
      <h1>Appointments</h1>

      {loading && <Loading />}

      {!loading && error && <Alert variant="danger">{error}</Alert>}

      {!loading && (
        <>
          {user && (isAdmin || isClient) && (
            <Button className="mb-3" onClick={() => navigate("/appointment/create")}>
              Add New Appointment
            </Button>
          )}

          <AppointmentTable
            appointments={appointments}
            onDeleteClick={setToDelete}
            isAdmin={isAdmin}
            isWorker={isWorker}
            isClient={isClient}
          />

          {/* Delete confirmation modal */}
          {toDelete && (
            <AppointmentDeleteModal
              appointment={toDelete}
              onCancel={() => setToDelete(null)}
              onConfirm={confirmDelete}
              isDeleting={isDeleting}
            />
          )}
        </>
      )}
    </div>
  );
};

export default AppointmentPage;
