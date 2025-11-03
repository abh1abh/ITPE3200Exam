// frontend/src/api/AuthService.ts

// 🔹 Hjelpefunksjon for å hente token og lage riktig Authorization-header
export const getAuthHeaders = () => {
  const token = localStorage.getItem("token");

  if (!token) {
    console.warn("⚠️ No token found in localStorage");
    throw new Error("No token found. Please log in again.");
  }

  return {
    "Content-Type": "application/json",
    Authorization: `Bearer ${token}`,
  };
};