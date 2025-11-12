import React from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "./AuthContext";
import { Nav, Dropdown } from "react-bootstrap";

interface AuthSectionProps {
  onAnyClick?: () => void;
}

const AuthSection: React.FC<AuthSectionProps> = ({ onAnyClick }) => {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const { hasRole } = useAuth(); // Admin, Client and Worker.

  const handleLogout = () => {
    logout();
    onAnyClick?.(); // close navbar/dropdown if open
    navigate("/");
  };
  const handleProfile = () => {
    onAnyClick?.(); // close navbar/dropdown if open
    navigate("/profile");
  }
  const handleRegister = () => {
    onAnyClick?.(); // close navbar/dropdown if open
    navigate("/admin/register");
  }

  return ( //return html of the auth section of the navbar
    <Nav>
      {user ? (
        <Dropdown align="end"> 
          <Dropdown.Toggle as={Nav.Link} id="dropdown-user">
            Welcome, {user.sub}
          </Dropdown.Toggle>
          <Dropdown.Menu>
            <Dropdown.Item onClick={handleLogout}>Logout</Dropdown.Item>
            {(hasRole("Client") || hasRole("HealthcareWorker")) && (
              <>
                <Dropdown.Item onClick={handleProfile}>Profile</Dropdown.Item>
              </>
            )}
            {(hasRole("Admin")) && (
              <>
                <Dropdown.Item onClick={handleRegister}>Register User</Dropdown.Item>
              </>
            )}

            
          </Dropdown.Menu>
        </Dropdown>
      ) : (
        <>
          <Nav.Link as={Link} to="/login" onClick={onAnyClick}>
            Login
          </Nav.Link>
          <Nav.Link as={Link} to="/register" onClick={onAnyClick}>
            Register
          </Nav.Link>
        </>
      )}
    </Nav>
  );
};

export default AuthSection;
