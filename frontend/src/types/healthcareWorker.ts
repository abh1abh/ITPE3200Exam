export interface HealthcareWorker {
  healthcareWorkerId: number;
  name: string;
  address: string;
  phone: string;
  email: string;
}

export interface UpdateWorkerDto{
  name: string;
  phone: string;
  address: string;
  email: string;
  password?: string;
  healthcareWorkerId: number;
}