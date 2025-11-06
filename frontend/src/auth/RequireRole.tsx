import { Navigate, Outlet } from "react-router-dom";
import Loading from "../shared/Loading";
import { useAuth } from "./AuthContext";

import React from "react";

interface RequireRoleProps {
  roles: string[];
}

const RequireRole: React.FC<RequireRoleProps> = ({ roles }) => {
  const { hasRole, isLoading } = useAuth();
  if (isLoading) return <Loading />;

  const allowed = roles?.some((r) => hasRole(r)) ?? false;
  return allowed ? <Outlet /> : <Navigate to="/unauthorized" replace />;
};

export default RequireRole;
