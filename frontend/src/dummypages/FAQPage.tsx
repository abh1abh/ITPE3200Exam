import React from "react";
import { Container } from "react-bootstrap";
const FAQPage: React.FC = () => {
    return (
        <Container className="py-5 text-center">
            <h1 className="mb-4 text-center">FAQs</h1>
                <hr className="w-75 mx-auto my-4 border-secondary" />
                <h5>1. Is my data secure?</h5>
                <p>Yes. All user data is securely stored and encrypted.</p>
                <hr className="w-75 mx-auto my-4 border-secondary" />
                <h5>2. Can I cancel or reschedule my appointment?</h5>
                <p>Yes, you can reschedule or cancel your appointment before it starts.</p>
                <hr className="w-75 mx-auto my-4 border-secondary" />
                <h5>3. How much does an appointment cost?</h5>
                <p>Most appointments cost around 400 NOK per hour, depending on the service.</p>
        </Container>
        );};
export default FAQPage;
