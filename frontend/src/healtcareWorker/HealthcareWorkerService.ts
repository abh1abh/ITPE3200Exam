import { HealthcareWorker } from "../types/healthcareWorker";
import { API_URL, getAuthHeaders, handleResponse } from "../shared/http";

export const fetchAllWorkers = async () => {
  const response = await fetch(`${API_URL}/api/healthcareworker/`, {
    method: "GET",
    headers: getAuthHeaders(),
  });
  return handleResponse(response);
};

export const fetchWorker = async (id: number) => {
  const response = await fetch(`${API_URL}/api/healthcareworker/${id}`, {
    method: "GET",
    headers: getAuthHeaders(),
  });
  return handleResponse(response);
};

export const createWorker = async (worker: HealthcareWorker) => {
  const response = await fetch(`${API_URL}/api/healthcareworker/`, {
    method: "POST",
    headers: getAuthHeaders(),
    body: JSON.stringify(worker),
  });
  return handleResponse(response);
};

export const updateWorker = async (id: number, worker: HealthcareWorker) => {
  const response = await fetch(`${API_URL}/api/healthcareworker/${id}`, {
    method: "PUT",
    headers: getAuthHeaders(),
    body: JSON.stringify(worker),
  });
  return handleResponse(response);
};

export const deleteWorker = async (id: number) => {
  const response = await fetch(`${API_URL}/api/healthcareworker/${id}`, {
    method: "DELETE",
    headers: getAuthHeaders(),
  });
  return handleResponse(response);
};
