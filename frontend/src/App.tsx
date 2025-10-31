import "./App.css";
import HomePage from "./home/HomePage";
import Footer from "./shared/Footer";
import { BrowserRouter as Router, Navigate, Route, Routes } from "react-router-dom";
import TeamPage from "./dummypages/TeamPage";

function App() {
  return (
    <div>
      <Router>
        <Routes>
          <Route path="/" element={<HomePage />} />
          <Route path="/footer" element={<Footer />} />
          <Route path="/team" element={<TeamPage />} />
        </Routes>
        <Footer />
      </Router>
    </div>
  );
}

export default App;
