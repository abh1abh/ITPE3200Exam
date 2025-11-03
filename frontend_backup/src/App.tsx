import React from "react";
import { Container } from "react-bootstrap";
import { Navigate, Route, BrowserRouter as Router, Routes } from "react-router-dom";
import "./App.css";
import AppointmentCreatePage from "./appointments/AppointmentCreatePage";
import AppointmentListPage from "./appointments/AppointmentListPage";
import AppointmentUpdatePage from "./appointments/AppointmentUpdatePage";
import { AuthProvider } from "./auth/AuthContext";
import LoginPage from "./auth/LoginPage";
import ProtectedRoute from "./auth/ProtectedRoute";
import RegisterPage from "./auth/RegisterPage";
import NavMenu from "./shared/NavMenu";

const App: React.FC = () => {
  return (
    <AuthProvider>
      <Router>
        <NavMenu />
        <Container className="mt-4">
          <Routes>
            <Route path="/" element={<AppointmentListPage />} />
            <Route path="/login" element={<LoginPage />} />
            <Route path="/register" element={<RegisterPage />} />

            {/* Protected routes */}
            <Route element={<ProtectedRoute />}>
              <Route path="/appointmentcreate" element={<AppointmentCreatePage />} />
              <Route path="/appointmentupdate/:id" element={<AppointmentUpdatePage />} />
            </Route>

            {/* Fallback */}
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </Container>
      </Router>
    </AuthProvider>
  );
};

export default App;