import React from "react";
import { Container } from "react-bootstrap";

const TeamPage: React.FC = () => {
  return (
    <Container className="py-5 text-center">
      <h1>Our Team</h1>
      <p className="text-muted">Meet the amazing people who make our company thrive.</p>
    </Container>
  );
};

export default TeamPage;
