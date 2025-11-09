import React from "react";
import { Button, Card, Container } from "react-bootstrap";
const BlogPage: React.FC = () => {
    return (
        <Container className="py-5 text-center">
            <h1 className="text-center mb-4">Blog</h1>
            <p className="text-center text-muted mb-5">
                Stay tuned for updates!</p>
                <Card className="d-flex flex align-items-center justify-content-between p-3 shadow-sm">
                <div>
                    <h5 className="fw-bold mb-0">Clinic</h5>
                    <p className="mb-0 text-muted">New training equipment has been ordered</p>
                    <p>Posted 07.11.2025</p>
                </div>
                <Button variant="dark" size="sm">Read</Button>
            </Card>
                <Card className="d-flex flex align-items-center justify-content-between p-3 shadow-sm">
                <div>
                    <h5 className="fw-bold mb-0">System</h5>
                    <p className="mb-0 text-muted">A new update is coming to our website.</p>
                    <p>Posted 06.11.2025</p>
                </div>
                <Button variant="dark" size="sm">Read</Button>
            </Card>
    </Container>
);
};
export default BlogPage;
