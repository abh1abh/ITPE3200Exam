import React from "react";
import { Client } from "../types/client";
import { HealthcareWorker } from "../types/healthcareWorker";
import { Badge, Card, Col, Container, Row, Table, Button } from "react-bootstrap";
import { Link } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";

interface UserDetailsCardProps {
  user: Client | HealthcareWorker;
  onDeleteClick: (user: Client | HealthcareWorker) => void;
  isHealthcareWorker?: boolean;
}
// User details card component that displays role based user information and provides options to update or delete the user.
const UserDetailsCard: React.FC<UserDetailsCardProps> = ({ user, isHealthcareWorker, onDeleteClick }) => {
  const { hasRole } = useAuth(); //Handles roles
  if (hasRole("Client")) {
    isHealthcareWorker = false;
  }
  if (hasRole("HealthcareWorker")) {
    isHealthcareWorker = true;
  }
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
          <Link
            to={`/${isHealthcareWorker ? "healthcareworker" : "client"}/${user.id}/update`}
            className="btn btn-sm btn-primary me-2">
            Update
          </Link>
          <Button variant="danger" size="sm" onClick={() => onDeleteClick(user)}>
            Delete
          </Button>
        </Card.Body>
      </Card>
    </Container>
  );
};
export default UserDetailsCard;
