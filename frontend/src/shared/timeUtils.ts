//Shared utility for time and date.
//Placed in the 'shared' so that all pages can use it.
//Providing correct date and time formatting.
export const formatDateTime = (d: string | Date) =>
  new Date(d).toLocaleString(undefined, {
    year: "numeric",
    month: "short",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
  });
//Date formatting
export const formatDateOnly = (d: string | Date) =>
  new Date(d).toLocaleDateString(undefined, {
    year: "numeric",
    month: "short",
    day: "2-digit",
  });
//Time formatting
export const formatTimeOnly = (d: string | Date) =>
  new Date(d).toLocaleTimeString(undefined, {
    hour: "2-digit",
    minute: "2-digit",
  });
