import { Route, BrowserRouter as Router, Routes } from "react-router-dom";
import "./App.css";
import AppointmentCreatePage from "./appointments/AppointmentCreatePage";
import AppointmentListPage from "./appointments/AppointmentListPage";
import AppointmentUpdatePage from "./appointments/AppointmentUpdatePage";
import { AuthProvider } from "./auth/AuthContext";
import LoginPage from "./auth/LoginPage";
import RegisterAdmin from "./auth/RegisterAdmin";
import RegisterPage from "./auth/RegisterPage";
import AvailableSlotCreatePage from "./availableSlot/AvailableSlotCreatePage";
import AvailableSlotDeletePage from "./availableSlot/AvailableSlotDeletePage";
import AvailableslotPage from "./availableSlot/AvailableSlotPage";
import AvailableSlotUpdatePage from "./availableSlot/AvailableSlotUpdatePage";
import TeamPage from "./dummypages/TeamPage";
import HomePage from "./home/HomePage";
import Footer from "./shared/Footer";
import NavMenu from "./shared/NavMenu";


function App() {
  return (
    <AuthProvider>
      <div>
        <Router>
          <NavMenu />
          <Routes>
            <Route path="/" element={<HomePage />} />
            <Route path="/footer" element={<Footer />} />
            <Route path="/team" element={<TeamPage />} />
            <Route path="/login" element={<LoginPage />} />
            <Route path="/register" element={<RegisterPage />} />
            <Route path="/admin/register" element={<RegisterAdmin />} />
            <Route path="/availableslot" element={<AvailableslotPage />} />
            <Route path="/availableslot/create" element={<AvailableSlotCreatePage />} />
            <Route path="/availableslot/:slotId" element={<AvailableSlotUpdatePage />} />
            <Route path="/availableslot/:slotId/delete" element={<AvailableSlotDeletePage />} />
            <Route path="/appointments" element={<AppointmentListPage />} />
            <Route path="/appointments/create" element={<AppointmentCreatePage />} />
            <Route path="/appointments/update/:id" element={<AppointmentUpdatePage />} />
          </Routes>
          <Footer />
        </Router>
      </div>
    </AuthProvider>
  );
}

export default App;
