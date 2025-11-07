import React from "react";
import { Card, Col, Container, Row } from "react-bootstrap";
const ServicesPage: React.FC = () => {
    const services = [
    { title: "Consultation", desc: "Book and attend medical appointments with ease." },
    { title: "Medication Tracking", desc: "Keep track of your prescriptions and tasks better." },
    { title: "Health Analytics", desc: "Monitor your progress and receive professional recommendations." },];
    return (
        <Container className="py-5 text-center">
            <h1>Services</h1>
            <p className="text-muted">Overview of the healthcare services we provide.</p>
            <Row>
                {services.map((service, i) => (<Col md={4} key={i}><Card className="shadow-sm mb-4 p-3">
                <Card.Body>
                    <Card.Title className="fw-bold">{service.title}</Card.Title>
                    <Card.Text>{service.desc}</Card.Text>
                    </Card.Body>
                </Card></Col>
        ))}
        </Row>
        </Container>
        );
    };

export default ServicesPage;
