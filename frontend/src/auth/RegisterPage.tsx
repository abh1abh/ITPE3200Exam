import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Form, Button, Container, Alert } from "react-bootstrap";
import * as authService from "./authService";
import { RegisterDto } from "../types/auth";

const RegisterPage: React.FC = () => {
  // State to hold form data
  const [formData, setFormData] = useState<RegisterDto>({
    email: "",
    password: "",
    name: "",
    phone: "",
    address: "",
  });
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const navigate = useNavigate();

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    // Handling input changes
    setFormData({ ...formData, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    // Handling form submission
    e.preventDefault(); // Prevent default form submission behavior
    setError(null);
    setSuccess(null);
    try {
      await authService.register({
        //register function from authService
        ...formData,
      });
      setSuccess("Registration successful! You can now log in.");
      setTimeout(() => navigate("/login"), 2000); // Redirect after 2 seconds
    } catch (err) {
      if (err instanceof Error) {
        setError(err.message);
      } else {
        setError("An unknown error occurred.");
      }
      console.error(err);
    }
  };

  return (
    <div style={{ maxWidth: "500px", margin: "0 auto", padding: "20px" }}>
      <Container className="mt-5">
        <h2>Register as Client</h2>
        {error && <Alert variant="danger">{error}</Alert>}
        {success && <Alert variant="success">{success}</Alert>}
        <Form onSubmit={handleSubmit}>
          <Form.Group className="mb-3">
            <Form.Label>Email</Form.Label>
            <Form.Control type="email" name="email" value={formData.email} onChange={handleChange} required />
          </Form.Group>

          <Form.Group className="mb-3">
            <Form.Label>Password</Form.Label>
            <Form.Control
              type="password"
              name="password"
              pattern="^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$"
              value={formData.password}
              onChange={handleChange}
              required
            />
          </Form.Group>

          <Form.Group className="mb-3">
            <Form.Label>Name</Form.Label>
            <Form.Control
              type="text"
              name="name"
              pattern="/^[\p{L} '-]{1,100}$/u´"
              value={formData.name}
              onChange={handleChange}
              required
            />
          </Form.Group>

          <Form.Group className="mb-3">
            <Form.Label>Number</Form.Label>
            <Form.Control
              type="text"
              name="phone"
              pattern="^(\+?\d{1,3}[- ]?)?(\(?\d{1,4}\)?[- ]?)?\d{1,4}([- ]?\d{1,9})$"
              value={formData.phone}
              onChange={handleChange}
              required
            />
          </Form.Group>

          <Form.Group className="mb-3">
            <Form.Label>Address</Form.Label>
            <Form.Control
              type="text"
              name="address"
              pattern="^[A-Za-z0-9#.,'\/\-\s]{3,200}$"
              value={formData.address}
              onChange={handleChange}
              required
            />
          </Form.Group>

          <Button variant="primary" type="submit">
            Register
          </Button>
        </Form>
      </Container>
    </div>
  );
};

export default RegisterPage;
