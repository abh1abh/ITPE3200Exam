import React from "react";
import { Container, Nav, Navbar } from "react-bootstrap";
import AuthSection from "../auth/AuthSection";
const NavMenu: React.FC = () => {
  return (
    <Navbar expand="lg" style={{ backgroundColor: "#DDEB9D" }} variant="light" fixed="top" className="shadow-sm">
      <Container fluid>
        {/* <Navbar.Brand href="/" className="fw-bold fs- text-dark">
          Helse
        </Navbar.Brand> */}
        <Navbar.Toggle aria-controls="basic-navbar-nav" />
        <Navbar.Collapse id="basic-navbar-nav" className="justify-content-between">
          <Nav className="me-auto">
            <Nav.Link href="/" className="text-dark fw-medium mx-6">
              Home
            </Nav.Link>
            <Nav.Link href="/appointments" className="text-dark fw-medium mx-2">
              Appointments
            </Nav.Link>
          </Nav>
          <div className="d-flex justify-content-center justify-content-lg-end mt-4 mt-lg-0">
            <AuthSection />
          </div>
        </Navbar.Collapse>
      </Container>
    </Navbar>
  );
};
export default NavMenu;
