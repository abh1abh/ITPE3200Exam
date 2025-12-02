import { AvailableSlot } from "../types/availableSlot";
import { API_URL, getAuthHeaders, handleResponse } from "../shared/http";

// Fetch all available slots
export const fetchAllAvailableSlots = async () => {
  const response = await fetch(`${API_URL}/api/availableslot/`, {
    method: "GET",
    headers: getAuthHeaders(),
  });
  return handleResponse(response);
};

// Fetch available slots for the authenticated worker
export const fetchAllAvailableSlotsMine = async () => {
  const response = await fetch(`${API_URL}/api/availableslot/mine`, {
    method: "GET",
    headers: getAuthHeaders(),
  });
  return handleResponse(response);
};

// Fetch all unbooked available slots
export const fetchAllUnbookedAvailableSlots = async () => {
  const response = await fetch(`${API_URL}/api/availableslot/unbooked`, {
    method: "GET",
    headers: getAuthHeaders(),
  });
  return handleResponse(response);
};

// Fetch a specific available slot by ID
export const fetchAvailableSlot = async (id: number) => {
  const response = await fetch(`${API_URL}/api/availableslot/${id}`, {
    method: "GET",
    headers: getAuthHeaders(),
  });
  return handleResponse(response);
};

// Create a new available slot
export const createAvailableSlot = async (availableSlot: AvailableSlot) => {
  const response = await fetch(`${API_URL}/api/availableslot/`, {
    method: "POST",
    headers: getAuthHeaders(),
    body: JSON.stringify(availableSlot),
  });
  return handleResponse(response);
};

// Update an existing available slot
export const updateAvailableSlot = async (id: number, availableSlot: AvailableSlot) => {
  const response = await fetch(`${API_URL}/api/availableslot/${id}`, {
    method: "PUT",
    headers: getAuthHeaders(),
    body: JSON.stringify(availableSlot),
  });
  return handleResponse(response);
};

// Delete an available slot by ID
export const deleteAvailableSlot = async (id: number) => {
  const response = await fetch(`${API_URL}/api/availableslot/${id}`, {
    method: "DELETE",
    headers: getAuthHeaders(),
  });
  return handleResponse(response);
};
