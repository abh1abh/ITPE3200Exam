import React from "react";
import { Card, Container } from "react-bootstrap";
const CareersPage: React.FC = () => {
    return (
        <Container className="py-5 text-center">
            <h1 className="text-center mb-4">Careers</h1>
            <p className="text-center text-muted mb-5">
                We're looking for people to join our team!</p>
            <hr className="my-4" />
            <Card className="d-flex flex-row align-items-center justify-content-between p-3 shadow-sm">
                <div className="d-flex align-items-center gap-3">
                    <h5 className="fw-bold mb-0">Nurse</h5>
                    <p className="mb-0 text-muted">New York, US</p>
                    <p className="mb-0 text-muted">Full Time</p>
                </div>
                <p className="mb-0 text-muted">Apply by sending your CV to: homecare@apply.com</p>
            </Card>
            <hr className="my-4" />
            <Card className="d-flex flex-row align-items-center justify-content-between p-3 shadow-sm">
                <div className="d-flex align-items-center gap-3">
                    <h5 className="fw-bold mb-0">Driver</h5>
                    <p className="mb-0 text-muted">New York, US</p>
                    <p className="mb-0 text-muted">Part Time</p>
                </div>
                <p className="mb-0 text-muted">Apply by sending your CV to: homecare@apply.com</p>
            </Card>
            <hr className="my-4" />
            <Card className="d-flex flex-row align-items-center justify-content-between p-3 shadow-sm">
                <div className="d-flex align-items-center gap-3">
                    <h5 className="fw-bold mb-0">Nurse</h5>
                    <p className="mb-0 text-muted">New York, US</p>
                    <p className="mb-0 text-muted">Part Time</p>
                </div>
                <p className="mb-0 text-muted">Apply by sending your CV to: homecare@apply.com</p>
            </Card>
    </Container>
);
};

export default CareersPage;
