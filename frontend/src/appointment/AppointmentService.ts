const API_URL = import.meta.env.VITE_API_URL;

const headers = {
  "Content-Type": "application/json",
};
//feilhåndtering
const handleResponse = async (response: Response) => {
  if (response.ok) {
    if (response.status === 204) return null; // sletting
    return response.json();
  } else {
    const errorText = await response.text();
    throw new Error(errorText || "Network response was not ok");
  }
};
//get alle
export const fetchAppointments = async (token: string) => {
  const response = await fetch(`${API_URL}/api/Appointment`, {
    headers: { ...headers, Authorization: `Bearer ${token}` },
  });
  return handleResponse(response);
};
//get by id
export const fetchAppointmentById = async (id: number, token: string) => {
  const response = await fetch(`${API_URL}/api/Appointment/${id}`, {
    headers: {
      ...headers,
      Authorization: `Bearer ${token}`,
    },
  });
  return handleResponse(response);
};

//create appointment
export const createAppointment = async (appointment: any, token: string) => {
  const response = await fetch(`${API_URL}/api/Appointment`, {
    method: "POST",
    headers: { ...headers, Authorization: `Bearer ${token}` },
    body: JSON.stringify(appointment),
  });
  return handleResponse(response);
};
//update
export const updateAppointment = async (id: number, appointment: any, token: string) => {
  const response = await fetch(`${API_URL}/api/Appointment/${id}`, {
    method: "PUT",
    headers: {
      ...headers,
      Authorization: `Bearer ${token}`,
    },
    body: JSON.stringify(appointment),
  });
  return handleResponse(response);
};
//delete
export const deleteAppointment = async (id: number, token: string) => {
  const response = await fetch(`${API_URL}/api/Appointment/${id}`, {
    method: "DELETE",
    headers: { ...headers, Authorization: `Bearer ${token}` },
  });
  return handleResponse(response);
};
export const updateTaskStatus = async (
  appointmentId: number,
  taskId: number,
  isCompleted: boolean,
  token: string
) => {
  const response = await fetch(`${API_URL}/api/Appointment/${appointmentId}`, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
    },
    body: JSON.stringify({
      appointmentTasks: [
        {
          id: taskId,
          isCompleted,
        },
      ],
    }),
  });
  if (!response.ok) throw new Error("Failed to update");
  return response.json();
};
