import React, { use } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "./AuthContext";
import { Nav, Dropdown } from "react-bootstrap";

const AuthSection: React.FC = () => {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate("/");
  };

  return (
    <Nav>
      {user ? (
        <Dropdown align="end">
          <Dropdown.Toggle as={Nav.Link} id="dropdown-user">
            Welcome, {user.sub}
          </Dropdown.Toggle>
          <Dropdown.Menu>
            <Dropdown.Item onClick={handleLogout}>Logout</Dropdown.Item>
          </Dropdown.Menu>
        </Dropdown>
      ) : (
        <>
          <Nav.Link as={Link} to="/login">
            Login
          </Nav.Link>
          <Nav.Link as={Link} to="/register">
            Register
          </Nav.Link>
        </>
      )}
    </Nav>
  );
};

export default AuthSection;
