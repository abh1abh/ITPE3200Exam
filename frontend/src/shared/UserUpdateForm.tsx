import React, { useRef, useState } from "react";
import { Client } from "../types/client";
import { HealthcareWorker } from "../types/healthcareWorker";
import { useNavigate } from "react-router-dom";
import { Badge, Button, Card, Col, Container, Form, InputGroup, Row, Table } from "react-bootstrap";
import { UpdateUserDto } from "../types/user";

interface UserUpdateFormProps {
    user: Client | HealthcareWorker;
    role: "Client" | "HealthcareWorker";
    onUserChanged: (updated: Client | HealthcareWorker) => void;
    serverError?: string | null;
}
const UserUpdateForm: React.FC<UserUpdateFormProps> = ({
    user,
    role,
    onUserChanged,
    serverError = null,
}) => {
    const navigate = useNavigate();
    const onCancel = () => navigate(-1);

    const [name, setName] = useState<string>(user.name);
    const [email, setEmail] = useState<string>(user.email);
    const [phone, setPhone] = useState<string>(user.phone);
    const [address, setAddress] = useState<string>(user.address);
    const [password, setPassword] = useState<string>("");

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        const updatedUser: UpdateUserDto = {
            name,
            email,
            phone,
            address,
            password: password ? password : undefined,
            id: (user as any).id,
        };
        onUserChanged(updatedUser as Client | HealthcareWorker);
    };

    return (
        <Container style={{ maxWidth: "35rem" }}>
            <Card className="mb-3 border-0">
                <Card.Header className="fw-semibold ">Update {role} Information</Card.Header>
                <Card.Body>
                    <Form onSubmit={handleSubmit}>
                        <Form.Group className="mb-3" controlId="formName">
                            <Form.Label>Name</Form.Label>
                            <Form.Control
                                type="text"
                                value={name}
                                onChange={(e) => setName(e.target.value)}
                                required
                            />
                        </Form.Group>
                        <Form.Group className="mb-3" controlId="formEmail">
                            <Form.Label>Email</Form.Label>
                            <Form.Control
                                type="email"
                                value={email}
                                onChange={(e) => setEmail(e.target.value)}
                                required
                            />
                        </Form.Group>
                        <Form.Group className="mb-3" controlId="formPhone">
                            <Form.Label>Phone Number</Form.Label>
                            <Form.Control
                                type="text"
                                value={phone}
                                onChange={(e) => setPhone(e.target.value)}
                                required
                            />
                        </Form.Group>
                        <Form.Group className="mb-3" controlId="formAddress">
                            <Form.Label>Address</Form.Label>
                            <Form.Control
                                type="text"
                                value={address}
                                onChange={(e) => setAddress(e.target.value)}
                                required
                            />
                        </Form.Group>
                        <Form.Group className="mb-3" controlId="formPassword">
                            <Form.Label>Password (leave blank to keep current password)</Form.Label>
                            <Form.Control
                                type="password"
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
