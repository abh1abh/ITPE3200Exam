import "./App.css";
import HomePage from "./home/HomePage";
import Footer from "./shared/Footer";
import { BrowserRouter as Router, Navigate, Route, Routes } from "react-router-dom";
import TeamPage from "./dummypages/TeamPage";
import LoginPage from "./auth/LoginPage";
import RegisterPage from "./auth/RegisterPage";
import { AuthProvider } from "./auth/AuthContext";
import RegisterAdmin from "./auth/RegisterAdmin";
import AvailableSlotUpdatePage from "./availableslot/AvailableSlotUpdatePage";
import AvailableSlotCreatePage from "./availableslot/AvailableSlotCreatePage";
import AvailableSlotPage from "./availableslot/AvailableslotPage";
import AvailableSlotDeletePage from "./availableslot/AvailableSlotDeletePage";
import AppointmentPage from "./appointment/AppointmentPage";
import AppointmentCreatePage from "./appointment/AppointmentCreatePage";
import AppointmentUpdatePage from "./appointment/AppointmentUpdate";
import ProfileForm from "./shared/ProfileForm";
import ClientPage from "./client/ClientPage";
import HealthcareWorkerPage from "./healtcareworker/HealthcareWorkerPage";
import NavMenu from "./shared/NavMenu";

function App() {
  return (
    <AuthProvider>
      <div>
        <Router>
          <NavMenu />
          <div className="page-container">
            <Routes>
              <Route path="/" element={<HomePage />} />
              <Route path="/footer" element={<Footer />} />
              <Route path="/team" element={<TeamPage />} />
              <Route path="/login" element={<LoginPage />} />
              <Route path="/register" element={<RegisterPage />} />
              <Route path="/admin/register" element={<RegisterAdmin />} />
              <Route path="/availableslot" element={<AvailableSlotPage />} />
              <Route path="/availableslot/create" element={<AvailableSlotCreatePage />} />
              <Route path="/availableslot/:slotId" element={<AvailableSlotUpdatePage />} />
              <Route path="/availableslot/:slotId/delete" element={<AvailableSlotDeletePage />} />
              <Route path="/appointment/create" element={<AppointmentCreatePage />} />
              <Route path="/appointment/:id" element={<AppointmentUpdatePage />} />
              <Route path="/appointment" element={<AppointmentPage />} />
              <Route path="/profile" element={<ProfileForm />} />
              <Route path="/clients" element={<ClientPage />} />
              <Route path="/healthcareworkers" element={<HealthcareWorkerPage />} />
            </Routes>
          </div>
          <Footer />
        </Router>
      </div>
    </AuthProvider>
  );
}

export default App;
