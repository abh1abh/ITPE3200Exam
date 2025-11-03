import React, { createContext, useState, useContext, useEffect, ReactNode } from "react";
import { jwtDecode } from "jwt-decode";
import { User } from "../types/user";
import { LoginDto } from "../types/auth";
import * as authService from "./AuthService";
import { dir } from "console";

interface AuthContextType {
  // Define the shape of the auth context
  user: User | null;
  token: string | null;
  login: (credentials: LoginDto) => Promise<void>;
  logout: () => void;
  isLoading: boolean;
  hasRole: (role: string) => boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

function extractRole(decoded: any): string {
  const uriRole = decoded["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];
  //   const directRole = decoded.role;
  //   const rolesArray = decoded.roles;
  console.log(decoded);
  //   console.log("directRole", directRole);
  console.log("uriRole", uriRole);

  if (typeof uriRole === "string") return uriRole;
  return "";
}

export const AuthProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [token, setToken] = useState<string | null>(localStorage.getItem("token"));
  const [isLoading, setIsLoading] = useState<boolean>(true);

  useEffect(() => {
    // Check token validity on mount and when token changes
    if (token) {
      try {
        const decodedUser: User = jwtDecode(token);
        // Check if the token is expired
        if (decodedUser.exp * 1000 > Date.now()) {
          setUser({ ...decodedUser, role: extractRole(decodedUser) });
        } else {
          // Token is expired, clear it
          console.warn("Token expired");
          localStorage.removeItem("token");
          setUser(null);
          setToken(null);
        }
      } catch (error) {
        console.error("Invalid token", error);
        localStorage.removeItem("token");
      }
    }
    setIsLoading(false);
  }, [token]);

  const login = async (credentials: LoginDto) => {
    // Login and store token
    const { token } = await authService.login(credentials);
    localStorage.setItem("token", token);
    const decodedUser: User = jwtDecode(token);
    // console.log(token); // Debugging line to check the token
    setUser({ ...decodedUser, role: extractRole(decodedUser) });
    setToken(token);
  };

  const logout = () => {
    // Clear token and user data
    localStorage.removeItem("token");
    setUser(null);
    setToken(null);
  };
  const hasRole = (role: string) => user?.role === role;

  return (
    // Provide context to children
    <AuthContext.Provider value={{ user, token, login, logout, isLoading, hasRole }}>
      {!isLoading && children}
    </AuthContext.Provider>
  );
};

export const useAuth = (): AuthContextType => {
  // Custom hook to use auth context
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return context;
};
