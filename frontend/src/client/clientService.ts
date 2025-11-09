import { Client, UpdateClientDto } from "../types/client";
import { API_URL, getAuthHeaders, handleResponse } from "../shared/http";

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

export const updateClient = async (id: number, client: UpdateClientDto) => {
  const response = await fetch(`${API_URL}/api/client/${id}`, {
    method: "PUT",
    headers: getAuthHeaders(),
    body: JSON.stringify(client),
  });
  return handleResponse(response);
};

export const fetchClientByAuthId = async (authId: string) => {
  const response = await fetch(`${API_URL}/api/Client/clientauth/${authId}`, {
    method: "GET",
    headers: getAuthHeaders(),
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
