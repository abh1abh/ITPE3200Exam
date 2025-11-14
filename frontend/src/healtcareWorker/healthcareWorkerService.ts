import { HealthcareWorker } from "../types/healthcareWorker";
import { API_URL, getAuthHeaders, handleResponse } from "../shared/http";
import { UpdateUserDto } from "../types/user";

export const fetchAllWorkers = async () => {
  // Fetch all healthcare workers
  const response = await fetch(`${API_URL}/api/HealthcareWorker/`, {
    method: "GET",
    headers: getAuthHeaders(), // Authorization headers
  });
  return handleResponse(response); // Return list of healthcare workers
};

export const fetchWorker = async (id: number) => {
  // Fetch healthcare worker by ID
  const response = await fetch(`${API_URL}/api/HealthcareWorker/${id}`, {
    method: "GET",
    headers: getAuthHeaders(),
  });
  return handleResponse(response); // Return healthcare worker data
};
export const fetchWorkerBySelf = async () => {
  // Fetch healthcare worker data for the authenticated user
  const response = await fetch(`${API_URL}/api/HealthcareWorker/me`, {
    method: "GET",
    headers: getAuthHeaders(),
  });
  return handleResponse(response); // Return healthcare worker data
};
export const updateWorker = async (id: number, worker: UpdateUserDto) => {
  // Update healthcare worker by ID
  const response = await fetch(`${API_URL}/api/healthcareworker/update/${id}`, {
    method: "PUT",
    headers: getAuthHeaders(),
    body: JSON.stringify(worker),
  });
  return handleResponse(response); // Return updated healthcare worker data
};

export const deleteWorker = async (id: number) => {
  // Delete healthcare worker by ID
  const response = await fetch(`${API_URL}/api/HealthcareWorker/${id}`, {
    method: "DELETE",
    headers: getAuthHeaders(),
  });
  return handleResponse(response); // Return response
};
