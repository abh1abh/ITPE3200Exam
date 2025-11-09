import { Alert, Button, Modal } from "react-bootstrap";
import { HealthcareWorker } from "../types/healthcareWorker";

type Props = {
    worker: HealthcareWorker;
    onConfirm: () => void;
    onCancel: () => void;
    isDeleting?: boolean;
};

const HealthcareWorkerDeleteModal: React.FC<Props> = ({ worker, onConfirm, onCancel, isDeleting }) => (
    <Modal show onHide={onCancel}>
        <Modal.Header closeButton>Delete Healthcare Worker?</Modal.Header>
        <Modal.Body>
            <p>
                Are you sure you want to delete the healthcare worker <strong>{worker.name}</strong>?
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
export default HealthcareWorkerDeleteModal;