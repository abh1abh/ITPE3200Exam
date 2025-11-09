export interface LoginDto{
    username: string;
    password: string;
}
export interface RegisterDto{
    password: string;
    email: string;
    name: string;
    phone: string;
    address: string;
}
export interface RegisterFromAdminDto{
    password: string;
    email: string;
    name: string;
    phone: string;
    address: string;
    role: string;
}