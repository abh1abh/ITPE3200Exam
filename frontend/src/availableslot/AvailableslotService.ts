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

export const fetchAllAvailableSlots = async () => {
  const response = await fetch(`${API_URL}/api/availableslot/`, {
    method: "GET",
    headers: getAuthHeaders(),
  });
  return handleResponse(response);
};

export const fetchAllAvailableSlotsMine = async () => {
  const response = await fetch(`${API_URL}/api/availableslot/mine`, {
    method: "GET",
    headers: getAuthHeaders(),
  });
  return handleResponse(response);
};

export const fetchAllUnbookedAvailableSlots = async () => {
  const response = await fetch(`${API_URL}/api/availableslot/unbooked`, {
    method: "GET",
    headers: getAuthHeaders(),
  });
  return handleResponse(response);
};

export const fetchAvailableSlot = async (id: number) => {
  const response = await fetch(`${API_URL}/api/availableslot/${id}`, {
    method: "GET",
    headers: getAuthHeaders(),
  });
  return handleResponse(response);
};

export const createAvailableSlot = async (availableSlot: AvailableSlot) => {
  const response = await fetch(`${API_URL}/api/availableslot/`, {
    method: "POST",
    headers: getAuthHeaders(),
    body: JSON.stringify(availableSlot),
  });
  return handleResponse(response);
};

export const updateAvailableSlot = async (id: number, availableSlot: AvailableSlot) => {
  const response = await fetch(`${API_URL}/api/availableslot/${id}`, {
    method: "PUT",
    headers: getAuthHeaders(),
    body: JSON.stringify(availableSlot),
  });
  return handleResponse(response);
};

export const deleteAvailableSlot = async (id: number) => {
  const response = await fetch(`${API_URL}/api/availableslot/${id}`, {
    method: "DELETE",
    headers: getAuthHeaders(),
  });
  return handleResponse(response);
};
