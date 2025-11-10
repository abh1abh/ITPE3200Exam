import React from "react";
import { Container, Image } from "react-bootstrap";
const API_URL = import.meta.env.VITE_API_URL;

const TeamPage: React.FC = () => {
  return (
    <Container className="py-5 text-center">
      <h1 className="mb-4 text-center">Our Team</h1>
        <Image
        src={`${API_URL}/images/women_with_needle.jpg`}
        alt="Healthcare team working together" fluid rounded className="mb-4 shadow-sm"
        style={{ maxHeight: "450px", objectFit: "cover" }}/>
      <p className="text-muted mb-5">
      Our professional healthcare specialists are here to support your wellbeing.</p>
      <hr className="w-75 mx-auto my-4 border-secondary" />
      <div className="mb-3 p-3">
        <h5>Maria H.</h5>
        <p>Doctor</p>
        <p>15 years of experience</p>
      </div>
      <hr className="w-75 mx-auto my-4 border-secondary" />
      <div className="mb-3 p-3">
        <h5>Timmy T.</h5>
        <p>Registered Nurse</p>
        <p>5 years of experience</p>
      </div>
      <hr className="w-75 mx-auto my-4 border-secondary" />
      <div className="mb-3 p-3">
        <h5>Mario M.</h5>
        <p>Physiotherapist</p>
        <p>9 years of experience</p>
      </div>
      </Container>
    );};

export default TeamPage;
