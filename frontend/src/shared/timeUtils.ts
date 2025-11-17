// Utility functions for date and time manipulation and formatting

// Pad single digit numbers with leading zero
export const pad = (n: number) => String(n).padStart(2, "0");

// Convert Date object to 'YYYY-MM-DD' format for date input fields
export const toDateInput = (d: Date) => `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`;

// Add minutes to a Date object and return a new Date
export const addMinutes = (d: Date, mins: number) => {
  const copy = new Date(d);
  copy.setMinutes(copy.getMinutes() + mins);
  return copy;
};

// Convert 'YYYY-MM-DD' and 'HH:MM' strings to a Date object
export const combineDateTime = (dateStr: string, timeStr: string) => {
  const [y, mo, d] = dateStr.split("-").map(Number);
  const [h, mi] = timeStr.split(":").map(Number);
  return new Date(Date.UTC(y, mo - 1, d, h, mi, 0, 0)); // Use UTC to avoid timezone issues
};

// Round a Date object to the next quarter-hour
export const roundToNextQuarter = (base = new Date()) => {
  const d = new Date(base);
  const r = d.getMinutes() % 15;
  if (r) d.setMinutes(d.getMinutes() + (15 - r), 0, 0);
  else d.setSeconds(0, 0);
  return d;
};

// Generate options for every quarter-hour in a day. Limit from 07:00 to 20:00
export const quarterOptions = (() => {
  const startHour = 7; // 07:00
  const endHour = 20; // 20:00
  const options: string[] = [];

  // Nested loops to create time strings for each quarter-hour
  for (let h = startHour; h <= endHour; h++) {
    for (let m = 0; m < 60; m += 15) {
      if (h === endHour && m > 0) break; // Stop at 20:00
      options.push(`${pad(h)}:${pad(m)}`);
    }
  }
  return options;
})();

// For rendering date and time in a user-friendly format

// Format date and time as 'MMM DD, YYYY, HH:MM'
export const formatDateTime = (d: string | Date) =>
  new Date(d).toLocaleString(undefined, {
    year: "numeric",
    month: "short",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
  });

// Format date as 'MMM DD, YYYY'
export const formatDateOnly = (d: string | Date) =>
  new Date(d).toLocaleDateString(undefined, {
    year: "numeric",
    month: "short",
    day: "2-digit",
  });

// Format time as 'HH:MM'
export const formatTimeOnly = (d: string | Date) =>
  new Date(d).toLocaleTimeString(undefined, {
    hour: "2-digit",
    minute: "2-digit",
  });
