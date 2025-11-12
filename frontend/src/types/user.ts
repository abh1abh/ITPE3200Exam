export interface User {
  sub: string;
  email: string;
  jti: string;
  iat: number;
  exp: number;
  iss: string;
  aud: string;
  role: string;
}
export interface UpdateUserDto {
  name: string;
  phone: string;
  address: string;
  email: string;
  password?: string;
  id: number;
}
