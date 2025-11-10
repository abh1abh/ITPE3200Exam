export interface Client {
  clientId: number;
  name: string;
  address: string;
  phone: string;
  email: string;
}

export interface UpdateClientDto{
  name: string;
  phone: string;
  address: string;
  email: string;
  password?: string;
  clientId: number;
}