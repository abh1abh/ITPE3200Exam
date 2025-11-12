import { HealthcareWorker } from "../types/healthcareWorker";
import { API_URL, getAuthHeaders, handleResponse } from "../shared/http";
import { UpdateUserDto } from "../types/user";

export const fetchAllWorkers = async () => {
  const response = await fetch(`${API_URL}/api/HealthcareWorker/`, {
    method: "GET",
    headers: getAuthHeaders(),
  });
  return handleResponse(response);
};

export const fetchWorker = async (id: number) => {
  const response = await fetch(`${API_URL}/api/HealthcareWorker/${id}`, {
    method: "GET",
    headers: getAuthHeaders(),
  });
  return handleResponse(response);
};
export const fetchWorkerBySelf = async () => {
  const response = await fetch(`${API_URL}/api/HealthcareWorker/workerauth`, {
    method: "GET",
    headers: getAuthHeaders(),
  });
  return handleResponse(response);
}
export const updateWorker = async (id: number, worker: UpdateUserDto) => {
  const response = await fetch(`${API_URL}/api/auth/worker/${id}`, {
    method: "PUT",
    headers: getAuthHeaders(),
    body: JSON.stringify(worker),
  });
  return handleResponse(response);
};

export const deleteWorker = async (id: number) => {
  const response = await fetch(`${API_URL}/api/HealthcareWorker/${id}`, {
    method: "DELETE",
    headers: getAuthHeaders(),
  });
  return handleResponse(response);
};
