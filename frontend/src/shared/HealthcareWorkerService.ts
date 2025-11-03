import { AvailableSlot } from "../types/AvailableSlot";

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

export const fetchAllHealthcareWorkers = async () => {
  const response = await fetch(`${API_URL}/api/healthcareworker/`, {
    method: "GET",
    headers: getAuthHeaders(),
  });
  return handleResponse(response);
};
