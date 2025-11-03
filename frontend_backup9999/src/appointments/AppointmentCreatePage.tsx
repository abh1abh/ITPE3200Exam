import React from "react";
import { useNavigate } from "react-router-dom";
import * as AppointmentService from "../appointments/AppointmentService";
import { Appointment } from "../types/appointment";
import AppointmentForm from "./AppointmentForm";
const AppointmentCreatePage: React.FC = () => {
const navigate = useNavigate();
const handleAppointmentCreated = async (appointment: Appointment) => {
    try {
    const token = localStorage.getItem("token");
    if (!token) throw new Error("No token log in again!");

    const data = await AppointmentService.createAppointment(appointment, token);
    console.log(" Created appointment:", data);

      navigate("/appointments"); 
    } catch (error) {
    console.error("Cant create appointment:", error);
    }
};
return(
    <div style={{ padding:"20px" }}>
    <h2>Create New Appointment</h2>
    <AppointmentForm onAppointmentChanged={handleAppointmentCreated} />
    </div>);
};
export default AppointmentCreatePage;