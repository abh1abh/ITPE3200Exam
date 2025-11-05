export interface AvailableSlot {
  id?: number;
  healthcareWorkerId?: number;
  start: string;
  end: string;
  isBooked: boolean;
}
