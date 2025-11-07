import React from "react";
import { Container } from "react-bootstrap";

const TeamPage: React.FC = () => {
  return (
    <Container className="py-5 text-center">
      <h1 className="mb-4 text-center">Our Team</h1>
      <p className="text-muted mb-5">
      Our professional healthcare specialists are here to support your wellbeing.</p>
      <div className="mb-3 p-3 shadow-sm">
        <h5>Maria H.</h5>
        <p>Doctor</p>
        <p>15 years of experience</p>
      </div>
      <div className="mb-3 p-3 shadow-sm">
        <h5>Timmy T.</h5>
        <p>Registered Nurse</p>
        <p>5 years of experience</p>
      </div>
      <div className="mb-3 p-3 shadow-sm">
        <h5>Mario M.</h5>
        <p>Physiotherapist</p>
        <p>9 years of experience</p>
      </div>
      </Container>
    );};

export default TeamPage;
