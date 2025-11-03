import React from "react";
import { Dropdown, Nav } from "react-bootstrap";
import { Link } from "react-router-dom";
import { useAuth } from "./AuthContext";
const AuthSection: React.FC = () => {
const { user, logout } = useAuth();
return (
    <Nav>
    {user ? (
        <Dropdown align="end">
        <Dropdown.Toggle as={Nav.Link}>Welcome, {user.sub}</Dropdown.Toggle>
        <Dropdown.Menu>
            <Dropdown.Item onClick={logout}>Logout</Dropdown.Item>
            
        </Dropdown.Menu>
        </Dropdown>
    ) : (
        <>
        <Nav.Link as={Link} to="/login">Login</Nav.Link>
        <Nav.Link as={Link} to="/register">Register</Nav.Link>
        </>
    )}
    </Nav>
);
};
export default AuthSection;