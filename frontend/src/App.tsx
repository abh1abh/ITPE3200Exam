import "./App.css";
import HomePage from "./home/HomePage";
import Footer from "./shared/Footer";
import { BrowserRouter as Router, Navigate, Route, Routes } from "react-router-dom";
import TeamPage from "./dummypages/TeamPage";
import LoginPage from "./auth/LoginPage";
import RegisterPage from "./auth/RegisterPage";
import { AuthProvider } from "./auth/AuthContext";
import RegisterAdmin from "./auth/RegisterAdmin";
import AvailableslotPage from "./availableSlot/AvailableSlotPage";
import AvailableSlotCreatePage from "./availableSlot/AvailableSlotCreatePage";
import AvailableSlotUpdatePage from "./availableSlot/AvailableSlotUpdatePage";
import AvailableSlotDeletePage from "./availableSlot/AvailableSlotDeletePage";

function App() {
  return (
    <AuthProvider>
      <div>
        <Router>
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
          </Routes>
          <Footer />
        </Router>
      </div>
    </AuthProvider>
  );
}

export default App;
