export interface User {
  sub: string;
  email: string;
  nameid: string;
  jti: string;
  iat: number;
  exp: number;
  iss: string;
  aud: string;
  role: string;
}
