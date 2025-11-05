export interface ChangeLog {
  id: number;
  appointmentId: number;
  changeDate: string;
  changedByUserId: string;
  changeDescription: string;
}

export interface AppointmentTask {
  id?: number;
  appointmentId?: number;
  description: string;
  isCompleted?: boolean;
}

export interface Appointment {
  id?: number;
  clientId?: number;
  healthcareWorkerId: number;
  availableSlotId?: number;
  start: string;
  end: string;
  notes: string;
  appointmentTasks: AppointmentTask[];
  changeLogs?: ChangeLog[];
}

export interface AppointmentView extends Appointment {
  clientName?: string;
  healthcareWorkerName?: string;
}
