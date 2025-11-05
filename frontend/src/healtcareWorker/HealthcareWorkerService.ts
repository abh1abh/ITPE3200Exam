import { HealthcareWorker } from "../types/HealthcareWorker";

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
