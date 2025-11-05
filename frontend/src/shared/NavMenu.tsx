import React from "react";
import { Container, Nav, Navbar } from "react-bootstrap";
import AuthSection from "../auth/AuthSection";
import { useAuth } from "../auth/AuthContext";
const NavMenu: React.FC = () => {
  const { hasRole, user } = useAuth(); // Admin, Client and Worker.
  return (
    <Navbar
      expand="lg"
      style={{ backgroundColor: "#E8F1FA" }}
      variant="light"
      fixed="top"
      className="shadow-sm mb-4">
      <Container fluid>
        <Navbar.Toggle aria-controls="basic-navbar-nav" />
        <Navbar.Collapse id="basic-navbar-nav" className="justify-content-between">
          <Nav className="me-auto">
            <Nav.Link href="/" className="text-dark fw-medium mx-6">
              Home
            </Nav.Link>
            {user && (
              <>
                <Nav.Link href="/appointment" className="text-dark fw-medium mx-2">
                  Appointments
                </Nav.Link>
              </>
            )}
            {(hasRole("Admin") || hasRole("HealthcareWorker")) && (
              <>
                <Nav.Link href="/availableslot" className="text-dark fw-medium mx-2">
                  Available slots
                </Nav.Link>
              </>
            )}
            {hasRole("Admin") && (
              <>
                <Nav.Link href="/clients" className="text-dark fw-medium mx-2">
                  Clients
                </Nav.Link>

                <Nav.Link href="/healthcareworkers" className="text-dark fw-medium mx-2">
                  Healthcare workers
                </Nav.Link>
              </>
            )}
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
