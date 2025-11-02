import "./App.css";
import HomePage from "./home/HomePage";
import Footer from "./shared/Footer";
import { BrowserRouter as Router, Navigate, Route, Routes } from "react-router-dom";
import TeamPage from "./dummypages/TeamPage";
import LoginPage from "./auth/LoginPage";
import RegisterPage from "./auth/RegisterPage";
import { AuthProvider } from "./auth/AuthContext";
import RegisterAdmin from "./auth/RegisterAdmin";

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
        </Routes>
        <Footer />
      </Router>
    </div>
    </AuthProvider>
  );
}

export default App;
