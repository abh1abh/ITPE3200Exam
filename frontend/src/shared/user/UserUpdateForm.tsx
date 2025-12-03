import React, { useState } from "react";
import { Client } from "../../types/client";
import { HealthcareWorker } from "../../types/healthcareWorker";
import { useNavigate } from "react-router-dom";
import { Button, Card, Container, Form } from "react-bootstrap";
import { UpdateUserDto } from "../../types/user";

interface UserUpdateFormProps {
  profileUser: Client | HealthcareWorker;
  role: string;
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

  // Password states
  const [isEditingPassword, setIsEditingPassword] = useState<boolean>(false);
  const [password, setPassword] = useState<string>("");
  const [confirmPassword, setConfirmPassword] = useState<string>("");
  const [localError, setLocalError] = useState<string | null>(null);

  // Handle form submission
  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault(); // Prevent default form submission behavior

    // If user chose to change password, validate it
    if (isEditingPassword && password !== confirmPassword) {
      setLocalError("Passwords do not match.");
      return;
    }

    // Create updated user object
    const updatedUser: UpdateUserDto = {
      name,
      email,
      phone,
      address,
      // Only include password if user actually chose to change it and filled it
      ...(isEditingPassword && password ? { password } : {}),
      id: profileUser.id,
    };
    // Call the onUserChanged callback with updated conditional user type
    onUserChanged(updatedUser);
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
                pattern="^[A-Za-zÀ-ÖØ-öø-ÿ' -]{1,100}$"
                onChange={(e) => setName(e.target.value)}
                required
              />
            </Form.Group>
            <Form.Group className="mb-3" controlId="formEmail">
              <Form.Label>Email</Form.Label>
              <Form.Control
                type="email" // Type email for validation
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
              />
            </Form.Group>
            <Form.Group className="mb-3" controlId="formPhone">
              <Form.Label>Phone Number</Form.Label>
              <Form.Control
                type="text"
                pattern="^\+?[0-9\s-]{3,15}$"
                value={phone}
                onChange={(e) => setPhone(e.target.value)}
                required
              />
            </Form.Group>
            <Form.Group className="mb-3" controlId="formAddress">
              <Form.Label>Address</Form.Label>
              <Form.Control
                type="text"
                pattern="^[A-Za-z0-9#.,'/ ]{3,200}$"
                value={address}
                onChange={(e) => setAddress(e.target.value)}
                required
              />
            </Form.Group>
            {/* Password section behind a button */}
            {!isEditingPassword ? (
              <Form.Group className="mb-3" controlId="formPassword">
                <Form.Label>Password</Form.Label>
                <div className="d-flex justify-content-between align-items-center">
                  <Form.Text className="text-muted">Current password is unchanged.</Form.Text>
                  <Button
                    variant="outline-secondary"
                    size="sm"
                    type="button"
                    onClick={() => setIsEditingPassword(true)}>
                    Change password
                  </Button>
                </div>
              </Form.Group>
            ) : (
              <>
                <Form.Group className="mb-3" controlId="formPassword">
                  <Form.Label>New password</Form.Label>
                  <Form.Text className="text-muted d-block mb-2">
                    Enter a new password. Leave both fields empty to keep the current password.
                  </Form.Text>
                  <Form.Control
                    className="mt-2"
                    type="password"
                    autoComplete="new-password"
                    placeholder="New password"
                    pattern="^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                  />
                </Form.Group>

                <Form.Group className="mb-3" controlId="formConfirmPassword">
                  <Form.Label>Confirm new password</Form.Label>
                  <Form.Control
                    className="mt-2"
                    type="password"
                    autoComplete="new-password"
                    placeholder="Confirm new password"
                    value={confirmPassword}
                    onChange={(e) => setConfirmPassword(e.target.value)}
                  />
                </Form.Group>
                <Button
                  variant="outline-secondary"
                  className=" mt-1 mb-3"
                  type="button"
                  onClick={() => {
                    setIsEditingPassword(false);
                    setPassword("");
                    setConfirmPassword("");
                    setLocalError(null);
                  }}>
                  Cancel password change
                </Button>
              </>
            )}
            {(localError || serverError) && <div className="text-danger mb-3">{localError || serverError}</div>}{" "}
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
