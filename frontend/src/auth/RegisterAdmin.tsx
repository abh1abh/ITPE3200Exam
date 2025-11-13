import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Form, Button, Container, Alert } from "react-bootstrap";
import * as authService from "./authService";

// TODO: Missing useState type and naming convention

const RegisterAdmin: React.FC = () => {
  /*Saving variables for data transfer to RegisterFromAdminDto*/
  // Missing type
  const [formData, setFormData] = useState({
    email: "",
    password: "",
    name: "",
    number: "",
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
    /*Submitting data form input to authService*/
    e.preventDefault();
    setError(null);
    setSuccess(null);
    try {
      await authService.registerAdmin({ //registerAdmin function from authService
        ...formData,
      });
      setSuccess("Registration successful!");
      setTimeout(() => navigate("/"), 2000); // Redirect after 2 seconds
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
    <div style={{maxWidth: "500px", margin: "0 auto", padding: "20px"}}>
    <Container className="mt-5">
      <h2>Register</h2>
      {error && <Alert variant="danger">{error}</Alert>}
      {success && <Alert variant="success">{success}</Alert>}
      <Form onSubmit={handleSubmit}>
        <Form.Group className="mb-3">
          <Form.Label>Email</Form.Label>
          <Form.Control 
          type="email" 
          name="email" 
          value={formData.email} 
          pattern="^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$" 
          onChange={handleChange} 
          required />
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
          pattern="/^[\p{L} '-]{1,100}$/u´" 
          onChange={handleChange} 
          required />
        </Form.Group>

        <Form.Group className="mb-3">
          <Form.Label>Number</Form.Label>
          <Form.Control 
          type="text" 
          name="number" 
          pattern="^(\+?\d{1,3}[- ]?)?(\(?\d{1,4}\)?[- ]?)?\d{1,4}([- ]?\d{1,9})$" 
          value={formData.number} 
          onChange={handleChange} 
          required />
        </Form.Group>

        <Form.Group className="mb-3">
          <Form.Label>Address</Form.Label>
          <Form.Control 
          type="text" 
          name="address" 
          pattern="^[A-Za-z0-9#.,'\/\-\s]{3,200}$"
          value={formData.address} 
          onChange={handleChange} 
          required />
        </Form.Group>
        <Form.Group className="mb-3">
          <Form.Label>Role</Form.Label>
          <Form.Select 
          name="role" 
          value={formData.role} 
          onChange={handleChange} 
          required>
            <option value="">Select a role</option>*<option value="Admin">Admin</option>
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
