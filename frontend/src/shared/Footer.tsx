import React from "react";
import { Col, Container, Row } from "react-bootstrap";
import { FaEnvelope, FaHome, FaPhone } from "react-icons/fa";
import { Link } from "react-router-dom";

// Insperations:
// https://getbootstrap.com/docs/5.3/examples/footers/#
// https://mdbootstrap.com/docs/standard/navigation/footer/

const Footer: React.FC = () => {
  return (
    //Shared footer.
    //Placed in the 'shared' so that all pages can use it.
    //Main footer element.
    //Used Bootstrap.
    <footer className="bg-light text-center text-lg-start mt-auto border-top">
      <Container className="py-4">
        <Row>
          <Col md={4} className="mb-3 mb-md-0">
            <h6 className="text-uppercase fw-bold mb-3">Products</h6>
            <ul className="list-unstyled">
              <li>
                <Link to="/pricing" className="text-muted text-decoration-none">
                  Pricing
                </Link>
              </li>
              <li>
                <Link to="/services" className="text-muted text-decoration-none">
                  Services
                </Link>
              </li>
              <li>
                <Link to="/faq" className="text-muted text-decoration-none">
                  FAQs
                </Link>
              </li>
              <li>
                <Link to="/about" className="text-muted text-decoration-none">
                  About us
                </Link>
              </li>
            </ul>
          </Col>
          <Col md={4} className="mb-3 mb-md-0">
            <h6 className="text-uppercase fw-bold mb-3">Company</h6>
            <ul className="list-unstyled">
              <li>
                <Link to="/team" className="text-muted text-decoration-none">
                  Team
                </Link>
              </li>
              <li>
                <Link to="/careers" className="text-muted text-decoration-none">
                  Careers
                </Link>
              </li>
              <li>
                <Link to="/blog" className="text-muted text-decoration-none">
                  Blog
                </Link>
              </li>
              <li>
                <Link to="/contact" className="text-muted text-decoration-none">
                  Contact
                </Link>
              </li>
            </ul>
          </Col>
          <Col md={4}>
            <h6 className="text-uppercase fw-bold mb-3">Contact</h6>
            <ul className="list-unstyled">
              <li>
                <p>
                  <FaHome className="me-2" /> New York, NY 10012, US
                </p>
              </li>
              <li>
                <p>
                  <FaEnvelope className="me-2" /> info@example.com
                </p>
              </li>
              <li>
                <p>
                  <FaPhone className="me-2" /> + 01 234 567 88
                </p>
              </li>
            </ul>
          </Col>
        </Row>
      </Container>

      <div className="text-center py-3 border-top text-muted" style={{ fontSize: "0.9rem" }}>
        {new Date().getFullYear()} ITPE3200 Exam 2025
      </div>
    </footer>
  );
};

export default Footer;
