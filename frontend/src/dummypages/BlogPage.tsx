import React from "react";
import { Card, Container } from "react-bootstrap";
const BlogPage: React.FC = () => {
    return (
        <Container className="py-5 text-center">
            <h1 className="text-center mb-4">Blog</h1>
            <p className="text-center text-muted mb-5">
                Stay tuned for updates!</p>
                <hr className="w-75 mx-auto my-4 border-secondary" />

                <Card className="shadow-sm mb-3 mx-auto p-3 d-flex flex-row justify-content-between align-items-center" style={{ maxWidth: "900px" }}>
                    <h5 className="fw-bold mb-0">Clinic</h5>
                    <p className="mb-0 text-muted">New training equipment has been ordered</p>
                    <p className="mb-0 text-muted">Posted 07.11.2025</p>
            </Card>
                <hr className="w-75 mx-auto my-4 border-secondary" />

                <Card className="shadow-sm mb-3 mx-auto p-3 d-flex flex-row justify-content-between align-items-center"style={{ maxWidth: "900px" }}>
                    <h5 className="fw-bold mb-0">System</h5>
                    <p className="mb-0 text-muted">A new update is coming to our website.</p>
                    <p className="mb-0 text-muted">Posted 06.11.2025</p>
            </Card>
    </Container>
);
};
export default BlogPage;
