import { LoginDto, RegisterAdminDto, RegisterDto } from "../types/auth";
import { API_URL, getAuthHeaders, handleResponse } from "../shared/http";

// Service functions for authentication-related API calls
export const login = async (credentials: LoginDto): Promise<{ token: string }> => {
  const response = await fetch(`${API_URL}/api/Auth/login`, {
    //login endpoint
    method: "POST", //POST method
    headers: getAuthHeaders(), //set headers
    body: JSON.stringify(credentials), //send login data as json
  });

  return handleResponse(response); //handle the response
};

// Service function for user registration
export const register = async (userData: RegisterDto): Promise<any> => {
  const response = await fetch(`${API_URL}/api/client/register`, {
    //registration endpoint
    method: "POST", //POST method
    headers: getAuthHeaders(), //set headers
    body: JSON.stringify(userData), //send registration data as json
  });

  return handleResponse(response); //handle the response
};

export const registerAdmin = async (userData: RegisterAdminDto): Promise<any> => {
  const role = userData.role; //get role from userData
  const user: RegisterDto = {
    //create user object to map correctly to backend
    email: userData.email,
    name: userData.name,
    phone: userData.phone,
    address: userData.address,
    password: userData.password,
  };
  //register based on role to corresponding endpoint
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
