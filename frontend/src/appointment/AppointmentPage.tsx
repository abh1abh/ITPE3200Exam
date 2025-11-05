import React, { useEffect, useMemo, useState } from "react";
import { Button } from "react-bootstrap";
import { useAuth } from "../auth/AuthContext";
import AppointmentTable from "./AppointmentTable";
import { Appointment } from "../types/appointment";
import * as AppointmentService from "./appointmentService";

const AppointmentPage: React.FC = () => {
  const [appointments, setAppointments] = useState<Appointment[]>([]);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  const { user } = useAuth();

  const fetchAppointments = async () => {
    setLoading(true);
    setError(null);
    try {
      const token = localStorage.getItem("token");
      if (!token) throw new Error("No token,please try again.");
      const data = await AppointmentService.fetchAppointments();
      setAppointments(data);
      console.log("Appointments fetched:", data);
    } catch (error: any) {
      console.error("Error fetching ", error);
      setError("Failed");
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
  // Delete
  const handleAppointmentDeleted = async (id: number) => {
    const confirmDelete = window.confirm(`Are you sure ${id}?`);
    if (!confirmDelete) return;
    try {
      await AppointmentService.deleteAppointment(id);
      setAppointments((prev) => prev.filter((a) => a.id !== id));
      console.log(`Appointment ${id} deleted`);
    } catch (error) {
      console.error("Error deleting:", error);
      setError("Error deleting");
    }
  };

  return (
    <div>
      <h1>Appointments</h1>

      <Button onClick={fetchAppointments} className="btn btn-primary mb-3 me-2" disabled={loading}>
        {loading ? "Loading..." : "Refresh Appointments"}
      </Button>

      {error && <p style={{ color: "red" }}>{error}</p>}
      <AppointmentTable
        appointments={sortedAppointments}
        onAppointmentDeleted={user ? handleAppointmentDeleted : undefined}
      />

      {user && (
        <Button href="/appointment/create" className="btn btn-secondary mt-3">
          Add New Appointment
        </Button>
      )}
    </div>
  );
};

export default AppointmentPage;
