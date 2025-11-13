import React, { useState } from "react";
import { Container, Nav, Navbar } from "react-bootstrap";
import { NavLink } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import AuthSection from "../auth/AuthSection";
//Shared Navigation.
//Placed in the 'shared' so that all pages can use it.
//Get role based permissions.
const NavMenu: React.FC = () => {
  const { hasRole, user } = useAuth(); // Admin, Client and Worker.
  const [expanded, setExpanded] = useState(false);
  const handleNavClick = () => setExpanded(false);

  return (
    <Navbar
      expand="lg"
      collapseOnSelect
      expanded={expanded}
      onToggle={setExpanded}
      style={{ backgroundColor: "#E8F1FA" }}
      variant="light"
      fixed="top"
      className="shadow-sm mb-4">
      <Container fluid>
        <Navbar.Toggle aria-controls="basic-navbar-nav" />
        <Navbar.Collapse id="basic-navbar-nav" className="justify-content-between">
          <Nav className="me-auto">
            <Nav.Link as={NavLink} to="/" onClick={handleNavClick} className="text-dark fw-medium mx-6">
              Home
            </Nav.Link>
            {user && (
              <>
                <Nav.Link
                  as={NavLink}
                  to="/appointment"
                  onClick={handleNavClick}
                  className="text-dark fw-medium mx-2">
                  Appointments
                </Nav.Link>
              </>
            )}
            {(hasRole("Admin") || hasRole("Client")) && (
              <>
                <Nav.Link
                  as={NavLink}
                  to="/appointment/create"
                  onClick={handleNavClick}
                  className="text-dark fw-medium mx-2">
                  Create Appointment
                </Nav.Link>
              </>
            )}
            {(hasRole("Admin") || hasRole("HealthcareWorker")) && (
              <>
                <Nav.Link
                  as={NavLink}
                  to="/availableslot"
                  onClick={handleNavClick}
                  className="text-dark fw-medium mx-2">
                  Available slots
                </Nav.Link>
                <Nav.Link
                  as={NavLink}
                  to="/availableslot/create"
                  onClick={handleNavClick}
                  className="text-dark fw-medium mx-2">
                  Create Available Slot
                </Nav.Link>
              </>
            )}
            {hasRole("Admin") && (
              <>
                <Nav.Link as={NavLink} to="/clients" onClick={handleNavClick} className="text-dark fw-medium mx-2">
                  Clients
                </Nav.Link>

                <Nav.Link
                  as={NavLink}
                  to="/healthcareworkers"
                  onClick={handleNavClick}
                  className="text-dark fw-medium mx-2">
                  Healthcare workers
                </Nav.Link>
              </>
            )}
          </Nav>
          <div className="d-flex justify-content-center justify-content-lg-end mt-4 mt-lg-0">
            <AuthSection onAnyClick={handleNavClick} />
          </div>
        </Navbar.Collapse>
      </Container>
    </Navbar>
  );
};
export default NavMenu;
