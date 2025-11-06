import { Route, BrowserRouter as Router, Routes } from "react-router-dom";
import "./App.css";
import AppointmentPage from "./appointment/AppointmentPage";
import AppointmentCreatePage from "./appointments/AppointmentCreatePage";
import AppointmentUpdatePage from "./appointments/AppointmentUpdatePage";
import { AuthProvider } from "./auth/AuthContext";
import LoginPage from "./auth/LoginPage";
import RegisterAdmin from "./auth/RegisterAdmin";
import AvailableSlotCreatePage from "./availableslot/AvailableSlotCreatePage";
import AvailableSlotDeletePage from "./availableslot/AvailableSlotDeletePage";
import AvailableSlotPage from "./availableslot/AvailableslotPage";
import AvailableSlotUpdatePage from "./availableslot/AvailableSlotUpdatePage";
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
            </Routes>
          </div>
          <Footer />
        </Router>
      </div>
    </AuthProvider>
  );
}

export default App;
