const API_URL = import.meta.env.VITE_API_URL;

const getAuthHeaders = () => {
  const token = localStorage.getItem("token");
  const headers: HeadersInit = {
    "Content-Type": "application/json",
  };
  if (token) {
    headers["Authorization"] = `Bearer ${token}`;
  }
  return headers;
};

const handleResponse = async (response: Response) => {
  if (response.ok) {
    if (response.status === 204) {
      return null; // No content to return
    }
    return response.json();
  } else {
    const errorData = await response.json();
    const errorMessage = errorData?.message || "An error occurred";
    throw new Error(errorMessage);
  }
};

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
export const createAppointment = async (appointment: any) => {
  const response = await fetch(`${API_URL}/api/Appointment`, {
    method: "POST",
    headers: getAuthHeaders(),
    body: JSON.stringify(appointment),
  });
  return handleResponse(response);
};
// Update
export const updateAppointment = async (id: number, appointment: any) => {
  const response = await fetch(`${API_URL}/api/Appointment/${id}`, {
    method: "PUT",
    headers: getAuthHeaders(),
    body: JSON.stringify(appointment),
  });
  return handleResponse(response);
};
// Delete
export const deleteAppointment = async (id: number, token: string) => {
  const response = await fetch(`${API_URL}/api/Appointment/${id}`, {
    method: "DELETE",
    headers: getAuthHeaders(),
  });
  return handleResponse(response);
};
// export const updateTaskStatus = async (
//   appointmentId: number,
//   taskId: number,
//   isCompleted: boolean,
//   token: string
// ) => {
//   const response = await fetch(`${API_URL}/api/Appointment/${appointmentId}`, {
//     method: "PUT",
//     headers: getAuthHeaders(),
//     body: JSON.stringify({
//       appointmentTasks: [
//         {
//           id: taskId,
//           isCompleted,
//         },
//       ],
//     }),
//   });
//   if (!response.ok) throw new Error("Failed to update");
//   return response.json();
// };
