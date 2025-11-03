const API_URL = import.meta.env.VITE_API_URL;

const headers = {
  "Content-Type": "application/json",
};

//feilhåndtering
const handleResponse = async (response: Response) => {
  if (response.ok) {
    return response.json();
  } else {
    const errorText = await response.text();
    throw new Error(errorText || "Network response was not ok");
  }
};

//hent alle slots
export const fetchAvailableSlots = async (token: string) => {
  const response = await fetch(`${API_URL}/api/AvailableSlot`, {
    headers: {
      ...headers,
      Authorization: `Bearer ${token}`,
    },
  });
  return handleResponse(response);
};