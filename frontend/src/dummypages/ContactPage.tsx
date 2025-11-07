import React from "react";
import { Container } from "react-bootstrap";
const ContactPage: React.FC = () => {
    return (
        <Container className="py-5 text-center p-3 shadow-sm">
            <h1 className="mb-4">Contact Us</h1>
                <p className="text-muted mb-4">
                Feel free to reach out to us at any time!</p>
                <p className="fw-bold">New York, NY 10012, US</p>
                <p>info@example.com</p>
                <p>info2@example.com</p>
                <p>+01 234 567 88</p>
        </Container>
);};
export default ContactPage;
