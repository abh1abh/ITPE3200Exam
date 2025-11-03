import React, { useState } from "react";
import { Alert, Button, Container, Form } from "react-bootstrap";
import { useNavigate } from "react-router-dom";
import { useAuth } from "./AuthContext";

const LoginPage: React.FC = () => {
const { login }= useAuth();
const [formData, setFormData] = useState({ username: "", password: "" });
const [error, setError] = useState<string | null>(null);
const navigate =useNavigate();
const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {    setFormData({ ...formData, [e.target.name]: e.target.value });
};
const handleSubmit= async (e: React.FormEvent) => {
    e.preventDefault();
    try {await login(formData);
    navigate("/");
    } catch {setError("Invalid username or password!!!!");}
};
return (
    <Container className="mt-5">
    <h2>Login</h2>
    {error && <Alert variant="danger">{error}</Alert>}
    <Form onSubmit={handleSubmit}>
        <Form.Group className="mb-3">
        <Form.Label>Username</Form.Label>
        <Form.Control type="text" name="username" value={formData.username} onChange={handleChange} required />
        </Form.Group>
        <Form.Group className="mb-3">
        <Form.Label>Password</Form.Label>
        <Form.Control type="password" name="password" value={formData.password} onChange={handleChange} required />
        </Form.Group>
        <Button variant="primary" type="submit">Login</Button>
    </Form>
    </Container>
);
};
export default LoginPage;