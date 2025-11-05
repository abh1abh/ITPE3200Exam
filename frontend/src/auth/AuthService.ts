import { LoginDto, RegisterDto, RegisterFromAdminDto } from "../types/auth";
import { API_URL, getAuthHeaders, handleResponse } from "../shared/http";

export const login = async (credentials: LoginDto): Promise<{ token: string }> => {
  const response = await fetch(`${API_URL}/api/Auth/login`, {
    method: "POST",
    headers: getAuthHeaders(),
    body: JSON.stringify(credentials),
  });

  return handleResponse(response);
};

export const register = async (userData: RegisterDto): Promise<any> => {
  const response = await fetch(`${API_URL}/api/Auth/register`, {
    method: "POST",
    headers: getAuthHeaders(),
    body: JSON.stringify(userData),
  });

  return handleResponse(response);
};

export const registerAdmin = async (userData: RegisterFromAdminDto): Promise<any> => {
  const token = localStorage.getItem("token");
  if (!token) {
    throw new Error("No authentication token found.");
  }
  const response = await fetch(`${API_URL}/api/Auth/register-admin`, {
    method: "POST",
    headers: getAuthHeaders(),
    body: JSON.stringify(userData),
  });
  return handleResponse(response);
};
