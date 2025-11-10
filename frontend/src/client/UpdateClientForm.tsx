import React, { useRef, useState} from "react";
import { UpdateClientDto } from "../types/client";
import { useNavigate } from "react-router-dom";
import { Badge, Button, Card, Col, Container, Form, InputGroup, Row, Table } from "react-bootstrap";
import { Alert } from "react-bootstrap";
import { Client } from "../types/client";
import { updateClient } from "./clientService";


interface UpdateClientFormProps {
    client: UpdateClientDto
    onClientUpdated: (updated: UpdateClientDto) => void;
    serverError?: string | null;
}
const UpdateClientForm: React.FC<UpdateClientFormProps> = ({
    client: initialData,
    onClientUpdated,
    serverError = null,
}) => {
    const navigate = useNavigate();
    const onCancel = () => navigate(-1);

    const [name, setName] = useState<string>(initialData.name);
    const [email, setEmail] = useState<string>(initialData.email);
    const [password, setPassword] = useState<string>("");
    const [phone, setPhone] = useState<string>(initialData.phone);
    const [address, setAddress] = useState<string>(initialData.address);

    const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    const updatedClient: UpdateClientDto = {
        name: name || initialData.name,
        email: email || initialData.email,
        password: password,
        phone: phone || initialData.phone,
        address: address || initialData.address,
        id: initialData.id
    };
    onClientUpdated(updatedClient);
    };

    return (
        <Container>
        <Card>
            <Card.Body>
            <Form onSubmit={handleSubmit}>
                {serverError && <Alert variant="danger">{serverError}</Alert>}
                <Form.Group className="mb-3" controlId="name">
                <Form.Label>Name</Form.Label>
                <Form.Control
                    type="text"
                    value={name}
                    onChange={(e) => setName(e.target.value)}
                    required
                />
                </Form.Group>
                <Form.Group className="mb-3" controlId="email">
                <Form.Label>Email</Form.Label>
                <Form.Control
                    type="email"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    required
                />
                </Form.Group>
                <Form.Group className="mb-3" controlId="password">
                <Form.Label>Cange Password (leave blank to keep current)</Form.Label>
                <Form.Control
                    type="password"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    />
                </Form.Group>
                <Form.Group className="mb-3" controlId="number">
                <Form.Label>Phone Number</Form.Label>
                <Form.Control
                    type="text"
                    value={phone}
                    onChange={(e) => setPhone(e.target.value)}
                    required
                />
                </Form.Group>
                <Form.Group className="mb-3" controlId="address">
                <Form.Label>Address</Form.Label>
                <Form.Control
                    type="text"
                    value={address}
                    onChange={(e) => setAddress(e.target.value)}
                    required
                />
                </Form.Group>
                <div className="d-flex justify-content-end">
                <Button variant="secondary" className="me-2" onClick={onCancel}>
                    Cancel
                </Button>
                <Button variant="primary" type="submit">
                    Update Client
                </Button>
                </div>
                </Form>
            </Card.Body>
            </Card>
        </Container>
    );
};
export default UpdateClientForm;

