import React from "react";
import { Container } from "react-bootstrap";
const FAQPage: React.FC = () => {
    return (
        <Container className="py-5 text-center">
            <h1 className="mb-4 text-center">FAQs</h1>
            <div className="mb-3 p-3 shadow-sm">
                <h5>1. Is my data secure?</h5>
                <p>Yes. All user data is securely stored and encrypted.</p>
            </div>
            <div className="mb-3 p-3 shadow-sm">
                <h5>2. Can I cancel or reschedule my appointment?</h5>
                <p>Yes, you can reschedule or cancel your appointment before it starts.</p>
            </div>
            <div className="mb-3 p-3 shadow-sm">
                <h5>3. How much does a consultation cost?</h5>
                <p>Most consultations cost around 500 NOK per hour, depending on the service.</p>
            </div>
        </Container>
        );};
export default FAQPage;
