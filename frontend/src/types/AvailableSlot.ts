export interface AvailableSlot {
  id?: number;
  healthcareWorkerId?: number;
  start: Date;
  end: Date;
  isBooked: boolean;
}
