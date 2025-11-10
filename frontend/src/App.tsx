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
import ProfileForm from "./shared/ProfileForm";
import ClientPage from "./client/ClientPage";
import ClientUpdate from "./client/ClientUpdate";
import HealthcareWorkerPage from "./healtcareWorker/HealthcareWorkerPage";
import HealthcareWorkerUpdate from "./healtcareWorker/HealthcareWorkerUpdate";
import AvailableSlotPage from "./availableslot/AvailableSlotPage";
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
              <Route path="/pricing" element={<PricingPage />} />
              <Route path="/services" element={<ServicesPage />} />
              <Route path="/faq" element={<FAQPage />} />
              <Route path="/contact" element={<ContactPage />} />
              <Route path="/careers" element={<CareersPage />} />
              <Route path="/blog" element={<BlogPage />} />
              <Route path="/about" element={<AboutPage />} />

              <Route element={<ProtectedRoute />}>
                <Route path="/profile" element={<ProfileForm />} />
                <Route element={<RequireRole roles={["Admin"]} />}>
                  <Route path="/admin/register" element={<RegisterAdmin />} />
                  <Route path="/clients" element={<ClientPage />} />
                  <Route path="/client/:id/update" element={<ClientUpdate />} />
                  <Route path="/healthcareworkers" element={<HealthcareWorkerPage />} />
                  <Route path="/healthcareworker/:id/update" element={<HealthcareWorkerUpdate />} />
                </Route>
                <Route element={<RequireRole roles={["Admin", "HealthcareWorker"]} />}>
                  <Route path="/availableslot" element={<AvailableSlotPage />} />
                  <Route path="/availableslot/create" element={<AvailableSlotCreatePage />} />
                  <Route path="/availableslot/:slotId" element={<AvailableSlotUpdatePage />} />
                </Route>
                <Route element={<RequireRole roles={["Admin", "Client"]} />}>
                  <Route path="/appointment/create" element={<AppointmentCreatePage />} />
                </Route>
                <Route element={<RequireRole roles={["Admin", "HealthcareWorker", "Client"]} />}>
                  <Route path="/appointment/:id/update" element={<AppointmentUpdatePage />} />
                  <Route path="/appointment" element={<AppointmentPage />} />
                  <Route path="/appointment/:id" element={<AppointmentDetailsPage />} />
                </Route>
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
