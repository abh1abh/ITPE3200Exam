import { LoginDto, RegisterDto } from "../types/auth";

const API_URL = import.meta.env.VITE_API_URL;


const handleResponse = async (response: Response) => {
if (response.ok) {
    return response.json(); // Returner data hvis alt gikk bra
}
  //feilhåndtering
let errorMessage = "Something went wrong";
try {
    const errorData = await response.json();
    if (Array.isArray(errorData)) {
    errorMessage =errorData.map((err: any) => err.description).join(", ");
    } else if (errorData.message) {
    errorMessage = errorData.message;
    } else if (typeof errorData=== "string") {
    errorMessage = errorData;
    }
} catch {
    //hvis ikke json
    errorMessage = await response.text();
}
throw new Error(errorMessage ||"Request failed");
};
//sende brukernavn og passord 
export const login = async (
credentials: LoginDto
): Promise<{token?: string; result?: string }> => {
const response = await fetch(`${API_URL}/api/Auth/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(credentials),
});
return handleResponse(response);
};
//registrer ny 
export const register =async (userData: RegisterDto): Promise<any> => {
const response = await fetch(`${API_URL}/api/Auth/register`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(userData),
});
return handleResponse(response);
};
//logout
export const logout = () => {
localStorage.removeItem("token");
};