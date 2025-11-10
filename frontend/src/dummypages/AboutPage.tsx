import React from "react";
import { Col, Container, Image, Row } from "react-bootstrap";
const API_URL = import.meta.env.VITE_API_URL;
const AboutPage: React.FC = () => {
    return (
        <Container className="py-5 text-center p-3">
            <Row className="align-items-center">
                <Col md={6} className="mb-4 mb-md-0">
                    <Image
                    src={`${API_URL}/images/doctor.jpg`}
                    alt="Our Team" fluid rounded className="shadow-sm"/>
                </Col>
                <Col md={6}>
                    <h1>About us</h1>
                    <p className="mb-3">
                    We are a dedicated healthcare company focused on making appointments simple,
                    effective, and easy for everyone. Our mission is to connect patients with
                    professional healthcare workers through a user-friendly and safe system.</p>
                    <p className="mb-3">
                    We help you stay organized and manage your healthcare needs anytime,
                    anywhere.</p>
                </Col>
            </Row>
    </Container>
);
};
export default AboutPage;
