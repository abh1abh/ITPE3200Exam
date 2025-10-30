import React, { useEffect, useState } from "react";
import { Button } from "react-bootstrap";
import * as AppointmentService from "../appointments/AppointmentService";
import { useAuth } from "../auth/AuthContext";
import { Appointment } from "../types/appointment";
import AppointmentGrid from "./AppointmentGrid";
import AppointmentTable from "./AppointmentTable";
const AppointmentListPage: React.FC = () => {
  const [appointments, setAppointments] = useState<Appointment[]>([]);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  const [showTable, setShowTable] =useState<boolean>(true);
  const [searchQuery, setSearchQuery] = useState<string>("");
  const { user } = useAuth();
  const toggleView = () => setShowTable((prev) => !prev);
  const fetchAppointments =async () => {
    setLoading(true);
    setError(null);
    try {
      const token = localStorage.getItem("token");
      if (!token) throw new Error("No token,please try again.");
      const data = await AppointmentService.fetchAppointments(token);
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
    const savedViewMode = localStorage.getItem("appointmentViewMode");
    if (savedViewMode === "grid") setShowTable(false);
    fetchAppointments();
  }, []);
  //bestem view mode
  useEffect(() => {
    localStorage.setItem("appointmentViewMode", showTable ? "table" : "grid");
  }, [showTable]);
  //filtrer  
  const filteredAppointments = appointments.filter(
    (a) =>
      a.notes.toLowerCase().includes(searchQuery.toLowerCase()) ||
      a.clientId?.toString().includes(searchQuery)||
      a.healthcareWorkerId?.toString().includes(searchQuery)
  );
  //fjern appointment
  const handleAppointmentDeleted = async (id: number) => {
    const confirmDelete = window.confirm(`Are you sure ${id}?`);
    if (!confirmDelete) return;
    try {
      const token = localStorage.getItem("token");
      if (!token) throw new Error("No token found.");
      await AppointmentService.deleteAppointment(id, token);
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
      <Button onClick={fetchAppointments}className="btn btn-primary mb-3 me-2"
        disabled={loading}>{loading ? "Loading..." : "Refresh Appointments"}
      </Button>
      <Button onClick={toggleView} className="btn btn-secondary mb-3 me-2">
        {showTable ? "Display Grid" : "Display Table"}
      </Button>

      {error && <p style={{ color: "red" }}>{error}</p>}
      {showTable ? (
        <AppointmentTable
          appointments={filteredAppointments}
          onAppointmentDeleted={user ? handleAppointmentDeleted : undefined}/>
      ) : (
        <AppointmentGrid
          appointments={filteredAppointments}
          onAppointmentDeleted={user ? handleAppointmentDeleted : undefined}
        />
      )}

      {user && (<Button href="/appointmentcreate" className="btn btn-secondary mt-3">
          Add New Appointment</Button>
      )}
    </div>
  );
};

export default AppointmentListPage;