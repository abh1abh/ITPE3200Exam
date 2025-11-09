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
  const { user, hasRole } = useAuth();
  const isAdmin = hasRole("Admin");
  const isClient = hasRole("Client");
  const isWorker = hasRole("HealthcareWorker");
  const navigate = useNavigate();

  const [toDelete, setToDelete] = useState<AppointmentView | null>(null);
  const [isDeleting, setIsDeleting] = useState(false);

  const fetchAppointments = async () => {
    setLoading(true);
    setError(null);
    try {
      let data: AppointmentView[] = [];
      if (isAdmin) data = await appointmentService.fetchAppointments();
      else if (isClient) data = await appointmentService.fetchAppointmentsByClientId();
      else if (isWorker) data = await appointmentService.fetchAppointmentsByWorkerId();
      else {
        setError("No role matched.");
        return;
      }
      setAppointments(data);
      console.log("Appointments fetched:", data);
    } catch (error: any) {
      console.error("Error fetching ", error);
      setError("Failed to fetch appointments.");
    } finally {
      setLoading(false);
    }
  };
  useEffect(() => {
    fetchAppointments();
  }, []);

  // Sort most recent. useMemo so it only resorts when appointments change
  const sortedAppointments = useMemo(
    () => [...appointments].sort((a, b) => new Date(a.start).getTime() - new Date(b.start).getTime()),
    [appointments]
  );

  const confirmDelete = async () => {
    if (!toDelete?.id) return;
    setError(null);
    setIsDeleting(true);
    try {
      await appointmentService.deleteAppointment(toDelete.id);
      fetchAppointments();
      setToDelete(null);
    } catch (error) {
      console.error("Error deleting appointment: ", error);
      setError("Error deleting appointment");
      setToDelete(null);
    } finally {
      setIsDeleting(false);
    }
  };

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
            appointments={sortedAppointments}
            onDeleteClick={setToDelete}
            isAdmin={isAdmin}
            isWorker={isWorker}
            isClient={isClient}
          />

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
      {/* Dont href with buttons */}
    </div>
  );
};

export default AppointmentPage;
