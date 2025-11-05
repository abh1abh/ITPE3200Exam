import { Client } from "../types/Client";

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

export const fetchAllClients = async () => {
  const response = await fetch(`${API_URL}/api/client/`, {
    method: "GET",
    headers: getAuthHeaders(),
  });
  return handleResponse(response);
};

export const fetchClient = async (id: number) => {
  const response = await fetch(`${API_URL}/api/client/${id}`, {
    method: "GET",
    headers: getAuthHeaders(),
  });
  return handleResponse(response);
};

export const createClient = async (client: Client) => {
  const response = await fetch(`${API_URL}/api/client/`, {
    method: "POST",
    headers: getAuthHeaders(),
    body: JSON.stringify(client),
  });
  return handleResponse(response);
};

export const updateClient = async (id: number, client: Client) => {
  const response = await fetch(`${API_URL}/api/client/${id}`, {
    method: "PUT",
    headers: getAuthHeaders(),
    body: JSON.stringify(client),
  });
  return handleResponse(response);
};

export const deleteClient = async (id: number) => {
  const response = await fetch(`${API_URL}/api/client/${id}`, {
    method: "DELETE",
    headers: getAuthHeaders(),
  });
  return handleResponse(response);
};
