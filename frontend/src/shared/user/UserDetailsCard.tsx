import React from "react";
import { Client } from "../../types/client";
import { HealthcareWorker } from "../../types/healthcareWorker";
import { Card, Col, Container, Row } from "react-bootstrap";

interface UserDetailsCardProps {
  user: Client | HealthcareWorker;
}
// User details card component that displays role based user information
const UserDetailsCard: React.FC<UserDetailsCardProps> = ({ user }) => {
  return (
    <Container style={{ maxWidth: "35rem" }}>
      <Card className="mb-3 border-0">
        <Card.Header className="fw-semibold ">Details</Card.Header>
        <Card.Body>
          <Row className="g-3"></Row>
          <Col className="text-start">
            <div className="text-muted small mb-1">Name</div>
            <div className="fw-semibold">{user.name}</div>
          </Col>
          <Row className="g-3 mt-1">
            <Col className="text-start">
              <div className="text-muted small mb-1">Email</div>
              <div className="fw-semibold">{user.email}</div>
            </Col>
          </Row>
          <Row className="g-3 mt-1">
            <Col className="text-start">
              <div className="text-muted small mb-1">Phone Number</div>
              <div className="fw-semibold">{user.phone}</div>
            </Col>
          </Row>
          <Row className="g-3 mt-1">
            <Col className="text-start">
              <div className="text-muted small mb-1">Address</div>
              <div className="fw-semibold">{user.address}</div>
            </Col>
          </Row>
        </Card.Body>
      </Card>
    </Container>
  );
};
export default UserDetailsCard;
