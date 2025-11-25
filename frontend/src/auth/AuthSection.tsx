import React, { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "./AuthContext";
import { Nav, Dropdown } from "react-bootstrap";
import * as HealthcareWorkerService from "../healtcareWorker/healthcareWorkerService";
import * as ClientService from "../client/clientService";

interface AuthSectionProps {
  onAnyClick?: () => void;
}

const AuthSection: React.FC<AuthSectionProps> = ({ onAnyClick }) => {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const { hasRole } = useAuth(); // Admin, Client and Worker.

  // State for user name
  const [name, setName] = useState<string | null>(null);

  const handleLogout = () => {
    logout();
    onAnyClick?.(); // close navbar/dropdown if open
    navigate("/");
  };
  const handleProfile = () => {
    onAnyClick?.(); // close navbar/dropdown if open
    navigate("/profile");
  };
  const handleRegister = () => {
    onAnyClick?.(); // close navbar/dropdown if open
    navigate("/admin/register");
  };

  // Fetch user name based on role on component mount or user change
  useEffect(() => {
    const fetchProfileData = async () => {
      try {
        if (hasRole("HealthcareWorker")) {
          // Fetch healthcare worker data if user is a worker
          const worker = await HealthcareWorkerService.fetchWorkerBySelf();
          setName(worker.name);
        } else if (hasRole("Client")) {
          // Fetch client data if user is a client
          const client = await ClientService.fetchClientBySelf();
          setName(client.name);
        } else {
          setName(null);
        }
      } catch (error: any) {
        console.error("Error fetching profile data:", error);
      }
    };
    fetchProfileData();
  }, [user]);

  return (
    //return html of the auth section of the navbar
    <Nav>
      {user ? (
        <Dropdown align="end">
          <Dropdown.Toggle as={Nav.Link} id="dropdown-user">
            Welcome {name || user.sub}
          </Dropdown.Toggle>
          <Dropdown.Menu>
            <Dropdown.Item onClick={handleLogout}>Logout</Dropdown.Item>
            {(hasRole("Client") || hasRole("HealthcareWorker")) && (
              <>
                <Dropdown.Item onClick={handleProfile}>Profile</Dropdown.Item>{" "}
                {/* Both Client and Worker can access Profile */}
              </>
            )}
            {hasRole("Admin") && (
              <>
                <Dropdown.Item onClick={handleRegister}>Register User</Dropdown.Item>{" "}
                {/* Only Admin can access Register User */}
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
