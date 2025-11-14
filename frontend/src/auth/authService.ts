import { LoginDto, RegisterAdminDto, RegisterDto } from "../types/auth";
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
  const response = await fetch(`${API_URL}/api/client/`, {
    method: "POST",
    headers: getAuthHeaders(),
    body: JSON.stringify(userData),
  });

  return handleResponse(response);
};

export const registerAdmin = async (userData: RegisterAdminDto): Promise<any> => {
  const token = localStorage.getItem("token");
  if (!token) {
    throw new Error("No authentication token found.");
  }
  const role = userData.role;
  const user: RegisterDto = {
    email: userData.email,
    name: userData.name,
    number: userData.number,
    address: userData.address,
    password: userData.password,
  };
  if (role === "Client") {
    const response = await fetch(`${API_URL}/api/client/register`, {
      method: "POST",
      headers: getAuthHeaders(),
      body: JSON.stringify(user),
    });
    return handleResponse(response);
  } else if (role === "HealthcareWorker") {
    const response = await fetch(`${API_URL}/api/healthcareworker/register`, {
      method: "POST",
      headers: getAuthHeaders(),
      body: JSON.stringify(user),
    });
    return handleResponse(response);
  } else if (role === "Admin") {
    const response = await fetch(`${API_URL}/api/Auth/register`, {
      method: "POST",
      headers: getAuthHeaders(),
      body: JSON.stringify(user),
    });
    return handleResponse(response);
  }
};
