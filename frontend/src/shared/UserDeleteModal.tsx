import { Alert, Button, Modal } from "react-bootstrap";
import { Client } from "../types/client";
import { HealthcareWorker } from "../types/healthcareWorker";

type Props = {
    user: Client | HealthcareWorker;
    onConfirm: () => void;
    onCancel: () => void;
    isDeleting?: boolean;
};

const UserDeleteModal: React.FC<Props> = ({ user, onConfirm, onCancel, isDeleting }) => (
    <Modal show onHide={onCancel}>
        <Modal.Header closeButton>Delete Client?</Modal.Header>
        <Modal.Body>
            <p>
                Are you sure you want to delete the user? <strong>{user.name}</strong>?
            </p>
            <Alert variant="danger" className="mt-3">
                This action cannot be undone.
            </Alert>
        </Modal.Body>
        <Modal.Footer>
            <Button variant="secondary" onClick={onCancel} disabled={isDeleting}>
                Cancel
            </Button>
            <Button variant="danger" onClick={onConfirm} disabled={isDeleting}>
                {isDeleting ? "Deleting..." : "Delete"}
            </Button>
        </Modal.Footer>
    </Modal>
);
export default UserDeleteModal;