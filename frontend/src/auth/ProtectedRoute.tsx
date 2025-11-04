import React from "react";
import { Navigate, Outlet, useLocation } from "react-router-dom";
import { useAuth } from "./AuthContext";
import Loading from "../shared/Loading";

const ProtectedRoute: React.FC = () => {
  const { token, isLoading } = useAuth();
  const location = useLocation();

  if (isLoading) return <Loading />;

  if (!token) {
    // Redirect them to the /login page, but save the current location they were
    // trying to go to. This allows us to send them along to that page after they login.
    return <Navigate to="/login" replace state={{ from: location }} />;
  }

  return <Outlet />;
};

export default ProtectedRoute;
