import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Form, Button, Container, Alert } from "react-bootstrap";
import * as authService from "./authService";
import { RegisterAdminDto } from "../types/auth";

const RegisterAdmin: React.FC = () => {
  // State to hold form data
  const [formData, setFormData] = useState<RegisterAdminDto>({
    email: "",
    password: "",
    name: "",
    phone: "",
    address: "",
    role: "",
  });
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const navigate = useNavigate();

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) => {
    /*Saving input in formData from input on onChange*/
    setFormData({ ...formData, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    // Submitting data form input to authService
    e.preventDefault();
    setError(null);
    setSuccess(null);
    try {
      await authService.registerAdmin({
        // RegisterAdmin function from authService
        ...formData,
      });
      setSuccess("Registration successful!");
      let url;
      if (formData.role == "Admin") {
        url = "/";
      } else {
        url = `/${formData.role.toLowerCase()}s`;
      }
      setTimeout(() => navigate(url), 2000); // Redirect after 2 seconds
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
        <h2>Register</h2>
        {error && <Alert variant="danger">{error}</Alert>}
        {success && <Alert variant="success">{success}</Alert>}
        <Form onSubmit={handleSubmit}>
          <Form.Group className="mb-3">
            <Form.Label>Email</Form.Label>
            <Form.Control
              type="email" // Type of email validates input
              name="email"
              value={formData.email}
              onChange={handleChange}
              required
            />
          </Form.Group>

          <Form.Group className="mb-3">
            <Form.Label>Password</Form.Label>
            <Form.Control
              type="password"
              name="password"
              value={formData.password}
              pattern="^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$"
              onChange={handleChange}
              required
            />
          </Form.Group>

          <Form.Group className="mb-3">
            <Form.Label>Name</Form.Label>
            <Form.Control
              type="text"
              name="name"
              value={formData.name}
              pattern="^[A-Za-zÀ-ÖØ-öø-ÿ' -]{1,100}$"
              onChange={handleChange}
              required
            />
          </Form.Group>

          <Form.Group className="mb-3">
            <Form.Label>Phone Number</Form.Label>
            <Form.Control
              type="text"
              name="phone"
              pattern="^\+?[0-9\s-]{3,15}$"
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
              pattern="^[A-Za-z0-9#.,'/ ]{3,200}$"
              value={formData.address}
              onChange={handleChange}
              required
            />
          </Form.Group>
          <Form.Group className="mb-3">
            <Form.Label>Role</Form.Label>
            <Form.Select name="role" value={formData.role} onChange={handleChange} required>
              <option value="">Select a role</option>
              <option value="Admin">Admin</option>
              <option value="HealthcareWorker">HealthcareWorker</option>
              <option value="Client">Client</option>
            </Form.Select>
          </Form.Group>
          <Button variant="primary" type="submit">
            Register
          </Button>
        </Form>
      </Container>
    </div>
  );
};

export default RegisterAdmin;
