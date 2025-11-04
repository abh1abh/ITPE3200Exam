export interface LoginDto{
    username: string;
    password: string;
}
export interface RegisterDto{
    password: string;
    email: string;
    name: string;
    Number: string;
    Address: string;
}
export interface RegisterFromAdminDto{
    username: string;
    password: string;
    email: string;
    name: string;
    Number: string;
    Address: string;
    role: string;
}