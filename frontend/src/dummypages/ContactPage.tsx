import React from "react";
import { Container } from "react-bootstrap";
const ContactPage: React.FC = () => {
  return (
    <Container className="py-5 text-center p-3">
      <h1 className="mb-4">Contact Us</h1>
      <p className="text-muted mb-4">Feel free to reach out to us at any time!</p>
      <p className="fw-bold">Karl Johans gate 1, 0162 Oslo</p>
      <p>homecare@homecare.local</p>
      <p>+47 999 99 999</p>
    </Container>
  );
};
export default ContactPage;
