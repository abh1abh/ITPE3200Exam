import { Route, BrowserRouter as Router, Routes } from "react-router-dom";
import "./App.css";
import ProtectedRoute from "./auth/ProtectedRoute";
import RequireRole from "./auth/RequireRole";
import AppointmentCreatePage from "./appointment/AppointmentCreatePage";
import AppointmentDetailsPage from "./appointment/AppointmentDetailsPage";
import AppointmentPage from "./appointment/AppointmentPage";
import AppointmentUpdatePage from "./appointment/AppointmentUpdatePage";
import { AuthProvider } from "./auth/AuthContext";
import LoginPage from "./auth/LoginPage";
import RegisterAdmin from "./auth/RegisterAdmin";
import RegisterPage from "./auth/RegisterPage";
import AvailableSlotCreatePage from "./availableslot/AvailableSlotCreatePage";
import ProfilePage from "./profile/ProfilePage";
import ClientPage from "./client/ClientPage";
import ClientUpdate from "./client/ClientUpdate";
import ClientDetailsPage from "./client/ClientDetailsPage";
import HealthcareWorkerPage from "./healtcareWorker/HealthcareWorkerPage";
import HealthcareWorkerUpdate from "./healtcareWorker/HealthcareWorkerUpdate";
import HealthcareWorkerDetailsPage from "./healtcareWorker/HealthcareWorkerDetailsPage";
import AvailableSlotPage from "./availableslot/AvailableslotPage";
import AvailableSlotUpdatePage from "./availableslot/AvailableSlotUpdatePage";
import AboutPage from "./dummypages/AboutPage";
import BlogPage from "./dummypages/BlogPage";
import CareersPage from "./dummypages/CareersPage";
import ContactPage from "./dummypages/ContactPage";
import FAQPage from "./dummypages/FAQPage";
import PricingPage from "./dummypages/PricingPage";
import ServicesPage from "./dummypages/ServicesPage";
import TeamPage from "./dummypages/TeamPage";
import HomePage from "./home/HomePage";
import Footer from "./shared/Footer";
import NavMenu from "./shared/NavMenu";
import AppointmentChangeLogPage from "./appointment/AppointmentChangeLogPage";
import AvailableSlotPage from "./availableslot/AvailableslotPage";

function App() {
  return (
    <AuthProvider>
      <div>
        <Router>
          <NavMenu />
          <div className="page-container">
            <Routes>
              {/* Public routes */}
              <Route path="/" element={<HomePage />} />
              <Route path="/footer" element={<Footer />} />
              <Route path="/team" element={<TeamPage />} />
              <Route path="/login" element={<LoginPage />} />
              <Route path="/register" element={<RegisterPage />} />
              <Route element={<ProtectedRoute />}>
                <Route path="/profile" element={<ProfilePage />} />
                <Route element={<RequireRole roles={["Admin"]} />}>
                  {/* Admin Only Routes: */}
                  <Route path="/admin/register" element={<RegisterAdmin />} />
                  <Route path="/clients" element={<ClientPage />} />
                  <Route path="/healthcareworkers" element={<HealthcareWorkerPage />} />
                  <Route path="/healthcareworker/:id/details" element={<HealthcareWorkerDetailsPage />} />
                  <Route path="/client/:id/details" element={<ClientDetailsPage/>} />
                </Route>
                {/* HealthcareWorker and Admin Routes: */}
                <Route element={<RequireRole roles={["Admin", "HealthcareWorker"]} />}>
                  <Route path="/availableslot" element={<AvailableSlotPage />} />
                  <Route path="/availableslot/create" element={<AvailableSlotCreatePage />} />
                  <Route path="/availableslot/:slotId" element={<AvailableSlotUpdatePage />} />
                  <Route path="/healthcareworker/:id/update" element={<HealthcareWorkerUpdate />} />
                </Route>
                {/* Admin and Client Routes: */}
                <Route element={<RequireRole roles={["Admin", "Client"]} />}>
                  <Route path="/appointment/create" element={<AppointmentCreatePage />} />
                  <Route path="/client/:id/update" element={<ClientUpdate />} />
                </Route>
                {/* Admin, HealthcareWorker and Client Routes: */}
                <Route element={<RequireRole roles={["Admin", "HealthcareWorker", "Client"]} />}>
                  <Route path="/appointment/:id/update" element={<AppointmentUpdatePage />} />
                  <Route path="/appointment" element={<AppointmentPage />} />
                  <Route path="/appointment/:id" element={<AppointmentDetailsPage />} />
                  <Route path="/appointment/:id/changelog" element={<AppointmentChangeLogPage />} />
                </Route>
              <Route path="/pricing" element={<PricingPage />} />
              <Route path="/services" element={<ServicesPage />} />
              <Route path="/faq" element={<FAQPage />} />
              <Route path="/contact" element={<ContactPage />} />
              <Route path="/careers" element={<CareersPage />} />
              <Route path="/blog" element={<BlogPage />} />
              <Route path="/about" element={<AboutPage />} />
              </Route>
            </Routes>
          </div>
          <Footer />
        </Router>
      </div>
    </AuthProvider>
  );
}

export default App;
