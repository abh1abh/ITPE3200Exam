import React from "react";
import { Nav, Navbar } from "react-bootstrap";
import AuthSection from "../auth/AuthSection";

const NavMenu: React.FC = () => {
return (
    <Navbar expand="lg" className="bg-body-tertiary px-3">
    <Navbar.Brand href="/">Helse</Navbar.Brand>
    <Navbar.Toggle aria-controls="basic-navbar-nav" />
    <Navbar.Collapse id="basic-navbar-nav">
        <Nav className="me-auto">
        <Nav.Link href="./home/HomePage">Home</Nav.Link>
        <Nav.Link href="/appointments">Appointments</Nav.Link>
        </Nav>
        <AuthSection />
    </Navbar.Collapse>
    </Navbar>
);
};

export default NavMenu;