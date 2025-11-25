export const API_URL = import.meta.env.VITE_API_URL;
// Return headers for api request
export const getAuthHeaders = () => {
  const token = localStorage.getItem("token");
  const headers: HeadersInit = {
    "Content-Type": "application/json",
  };
  // Check if user is logged in and include token
  if (token) headers["Authorization"] = `Bearer ${token}`;
  return headers;
};
// Handle API response and errors
export const handleResponse = async (response: Response) => {
  if (response.ok) {
    if (response.status === 204) return null;
    return response.json();
  } else {
    // Handle unauthorized access
    if (response.status === 403) {
      // Log and throw unauthorized error
      console.error("Error 403: Unauthorized access");
      throw new Error("Unauthorized access");
    }
    const errorData = await response.json();
    console.log(errorData);
    const errorMessage = errorData?.message || errorData?.title || "An error occurred";
    throw new Error(errorMessage);
  }
};
