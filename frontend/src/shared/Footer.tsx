import React from "react";
import { Container, Row, Col } from "react-bootstrap";
import { FaHome, FaEnvelope, FaPhone } from "react-icons/fa";

const Footer: React.FC = () => {
  return (
    <footer className="bg-light text-center text-lg-start mt-auto border-top">
      <Container className="py-4">
        <Row>
          <Col md={4} className="mb-3 mb-md-0">
            <h6 className="text-uppercase fw-bold mb-3">Products</h6>
            <ul className="list-unstyled">
              <li>
                <a href="#" className="text-muted text-decoration-none">
                  Pricing
                </a>
              </li>
              <li>
                <a href="#" className="text-muted text-decoration-none">
                  Services
                </a>
              </li>
              <li>
                <a href="#" className="text-muted text-decoration-none">
                  FAQs
                </a>
              </li>
              <li>
                <a href="#" className="text-muted text-decoration-none">
                  About us
                </a>
              </li>
            </ul>
          </Col>
          <Col md={4} className="mb-3 mb-md-0">
            <h6 className="text-uppercase fw-bold mb-3">Company</h6>
            <ul className="list-unstyled">
              <li>
                <a href="/team" className="text-muted text-decoration-none">
                  Team
                </a>
              </li>
              <li>
                <a href="#" className="text-muted text-decoration-none">
                  Careers
                </a>
              </li>
              <li>
                <a href="#" className="text-muted text-decoration-none">
                  Blog
                </a>
              </li>
              <li>
                <a href="#" className="text-muted text-decoration-none">
                  Contact
                </a>
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
