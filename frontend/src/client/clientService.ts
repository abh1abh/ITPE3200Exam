import { Client } from "../types/client";
import { UpdateUserDto } from "../types/user";
import { API_URL, getAuthHeaders, handleResponse } from "../shared/http";

export const fetchAllClients = async () => {
  // Fetch all clients
  const response = await fetch(`${API_URL}/api/client/`, {
    method: "GET",
    headers: getAuthHeaders(),
  });
  return handleResponse(response); // Return list of clients
};

export const fetchClient = async (id: number) => {
  // Fetch client by ID
  const response = await fetch(`${API_URL}/api/client/${id}`, {
    method: "GET",
    headers: getAuthHeaders(),
  });
  return handleResponse(response); // Return client data
};

export const updateClient = async (id: number, client: UpdateUserDto) => {
  // Update client by ID
  const response = await fetch(`${API_URL}/api/client/update/${id}`, {
    method: "PUT",
    headers: getAuthHeaders(),
    body: JSON.stringify(client),
  });
  return handleResponse(response); // Return updated client data
};

export const fetchClientBySelf = async () => {
  const response = await fetch(`${API_URL}/api/Client/me`, {
    // Fetch client data for the authenticated user
    method: "GET",
    headers: getAuthHeaders(),
  });
  return handleResponse(response); // Return client data
};
export const deleteClient = async (id: number) => {
  // Delete client by ID
  const response = await fetch(`${API_URL}/api/client/${id}`, {
    method: "DELETE",
    headers: getAuthHeaders(),
  });
  return handleResponse(response); // Return response
};
