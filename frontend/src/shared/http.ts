export const API_URL = import.meta.env.VITE_API_URL;

export const getAuthHeaders = () => {
  const token = localStorage.getItem("token");
  const headers: HeadersInit = {
    "Content-Type": "application/json",
  };
  if (token) headers["Authorization"] = `Bearer ${token}`;
  return headers;
};

export const handleResponse = async (response: Response) => {
  if (response.ok) {
    if (response.status === 204) return null;
    return response.json();
  } else {
    const errorData = await response.json();
    console.log(errorData);
    const errorMessage = errorData?.message || errorData?.title || "An error occurred";
    throw new Error(errorMessage);
  }
};
