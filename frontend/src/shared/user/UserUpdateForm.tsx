import React, { useState } from "react";
import { Client } from "../../types/client";
import { HealthcareWorker } from "../../types/healthcareWorker";
import { useNavigate } from "react-router-dom";
import { Button, Card, Container, Form } from "react-bootstrap";
import { UpdateUserDto } from "../../types/user";

interface UserUpdateFormProps {
  profileUser: Client | HealthcareWorker;
  role: "Client" | "HealthcareWorker";
  onUserChanged: (updated: Client | HealthcareWorker) => void;
  serverError?: string | null;
}
const UserUpdateForm: React.FC<UserUpdateFormProps> = ({
  profileUser,
  role,
  onUserChanged,
  serverError = null,
}) => {
  const navigate = useNavigate();
  const onCancel = () => navigate(-1);

  const [name, setName] = useState<string>(profileUser.name);
  const [email, setEmail] = useState<string>(profileUser.email);
  const [phone, setPhone] = useState<string>(profileUser.phone);
  const [address, setAddress] = useState<string>(profileUser.address);
  const [password, setPassword] = useState<string>("");

  // Handle form submission
  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault(); // Prevent default form submission behavior
    const updatedUser: UpdateUserDto = {
      // Create updated user object
      name,
      email,
      phone,
      address,
      password: password ? password : undefined,
      id: (profileUser as any).id,
    };
    onUserChanged(updatedUser as Client | HealthcareWorker); // Call the onUserChanged callback with updated conditional user type
  };

  return (
    <Container style={{ maxWidth: "35rem" }}>
      <Card className="mb-3 border-0">
        <Card.Header className="fw-semibold ">Update {role} Information</Card.Header>{" "}
        {/* Card header displaying the role being updated */}
        <Card.Body>
          <Form onSubmit={handleSubmit}>
            <Form.Group className="mb-3" controlId="formName">
              <Form.Label>Name</Form.Label>
              <Form.Control
                type="text"
                value={name}
                pattern="/^[\p{L} '-]{1,100}$/u´"
                onChange={(e) => setName(e.target.value)}
                required
              />
            </Form.Group>
            <Form.Group className="mb-3" controlId="formEmail">
              <Form.Label>Email</Form.Label>
              <Form.Control
                type="email"
                pattern="^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
              />
            </Form.Group>
            <Form.Group className="mb-3" controlId="formPhone">
              <Form.Label>Phone Number</Form.Label>
              <Form.Control
                type="text"
                pattern="^(\+?\d{1,3}[- ]?)?(\(?\d{1,4}\)?[- ]?)?\d{1,4}([- ]?\d{1,9})$"
                value={phone}
                onChange={(e) => setPhone(e.target.value)}
                required
              />
            </Form.Group>
            <Form.Group className="mb-3" controlId="formAddress">
              <Form.Label>Address</Form.Label>
              <Form.Control
                type="text"
                pattern="^[A-Za-z0-9#.,'\/\-\s]{3,200}$"
                value={address}
                onChange={(e) => setAddress(e.target.value)}
                required
              />
            </Form.Group>
            <Form.Group className="mb-3" controlId="formPassword">
              <Form.Label>Password (leave blank to keep current password)</Form.Label>{" "}
              {/* Password field with note. Leave empty to keep current password*/}
              <Form.Control
                type="password"
                pattern="^(|(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,})$"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
              />
            </Form.Group>
            {serverError && <div className="text-danger mb-3">{serverError}</div>}
            <div className="d-flex justify-content-end">
              <Button variant="secondary" className="me-2" onClick={onCancel}>
                Cancel
              </Button>
              <Button variant="primary" type="submit">
                Save Changes
              </Button>
            </div>
          </Form>
        </Card.Body>
      </Card>
    </Container>
  );
};
export default UserUpdateForm;
