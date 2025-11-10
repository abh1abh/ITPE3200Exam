import React from "react";
import { Card, Col, Container, Row } from "react-bootstrap";
const PricingPage: React.FC = () => {
    const prices = [
    { title: "Home Consultation", desc: "500 NOK / hour" },
    { title: "Household Help", desc: "350 NOK / hour" },
    { title: "Physical exercise support", desc: "400 NOK / hour" },];
    return (
        <Container className="py-5 text-center ">
            <h1>Pricing</h1>
            <p className="text-muted">Overview of the prices.</p>
            <Row>
            {prices.map((price, i) => (<Col md={4} key={i}><Card className="shadow-sm mb-4 p-3">
                <Card.Body>
                    <Card.Title className="fw-bold">{price.title}</Card.Title>
                    <Card.Text>{price.desc}</Card.Text>
                    </Card.Body>
                </Card></Col>
        ))}</Row>
        </Container>
        );
    };

export default PricingPage;