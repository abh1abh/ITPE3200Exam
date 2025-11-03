export interface AvailableSlot {
  id: number;
  healthCareWorkerId: number;
  start: Date;
  end: Date;
  isBooked: boolean;
}
