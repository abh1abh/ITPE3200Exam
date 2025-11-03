import { jwtDecode } from "jwt-decode";
import React, { createContext, ReactNode, useContext, useEffect, useState } from "react";
import { LoginDto } from "../types/auth";
import { User } from "../types/user";
import * as authService from "./AuthService";

interface AuthContextType {
  user: User | null;
  token: string | null;
  login: (credentials: LoginDto) => Promise<void>;
  logout: () => void;
  isLoading: boolean;
}
const AuthContext = createContext<AuthContextType | undefined>(undefined);
export const AuthProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
  const [user, setUser] =useState<User | null>(null);
  const [token, setToken] = useState<string |null>(localStorage.getItem("token"));
  const [isLoading, setIsLoading] =useState(true);

  useEffect(() => {
    if (token) {
      try {
        const decoded: User = jwtDecode(token);
        if (decoded.exp *1000 > Date.now()) {
          setUser(decoded);
        } else {
          localStorage.removeItem("token");
          setToken(null);
          setUser(null);
        }
      } catch {
        localStorage.removeItem("token");
        setToken(null);
        setUser(null);
      }
    }setIsLoading(false);
  }, [token]);

  const login = async (credentials: LoginDto) => {
    const response: any = await authService.login(credentials);
    const token:string =
      response?.token?.result ||
      response?.token ||
      response?.result||
      response;
    if (!token || typeof token !=="string") {
      throw new Error("Login failed: token not found in response");
    }
    localStorage.setItem("token", token);
    setUser(jwtDecode(token));
    setToken(token);
  };
  const logout =() => {
    localStorage.removeItem("token");
    setUser(null);
    setToken(null);
  };
  return (
    <AuthContext.Provider value={{ user, token, login, logout, isLoading }}>
      {!isLoading && children}
    </AuthContext.Provider>
  );
};
export const useAuth = () => {
  const context =useContext(AuthContext);
  if (!context)throw new Error("useauth must be used inside an authprovider");
  return context;
};