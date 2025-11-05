import { API_URL, getAuthHeaders, handleResponse } from "../shared/http";
import { Appointment } from "../types/appointment";

// Get all appointments
export const fetchAppointments = async () => {
  const response = await fetch(`${API_URL}/api/Appointment`, {
    headers: getAuthHeaders(),
  });
  return handleResponse(response);
};
// Get by id
export const fetchAppointmentById = async (id: number) => {
  const response = await fetch(`${API_URL}/api/Appointment/${id}`, {
    headers: getAuthHeaders(),
  });
  return handleResponse(response);
};

// Create appointment
export const createAppointment = async (appointment: Appointment) => {
  const response = await fetch(`${API_URL}/api/Appointment`, {
    method: "POST",
    headers: getAuthHeaders(),
    body: JSON.stringify(appointment),
  });
  return handleResponse(response);
};
// Update
export const updateAppointment = async (id: number, appointment: Appointment) => {
  const response = await fetch(`${API_URL}/api/Appointment/${id}`, {
    method: "PUT",
    headers: getAuthHeaders(),
    body: JSON.stringify(appointment),
  });
  return handleResponse(response);
};
// Delete
export const deleteAppointment = async (id: number) => {
  const response = await fetch(`${API_URL}/api/Appointment/${id}`, {
    method: "DELETE",
    headers: getAuthHeaders(),
  });
  return handleResponse(response);
};
