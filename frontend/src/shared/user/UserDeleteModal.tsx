import { Alert, Button, Modal } from "react-bootstrap";
import { Client } from "../../types/client";
import { HealthcareWorker } from "../../types/healthcareWorker";

type Props = {
  user: Client | HealthcareWorker;
  onConfirm: () => void;
  onCancel: () => void;
  isDeleting?: boolean;
};
// Delete confirmation modal component
const UserDeleteModal: React.FC<Props> = ({ user, onConfirm, onCancel, isDeleting }) => (
  <Modal show onHide={onCancel}>
    <Modal.Header closeButton>Delete User?</Modal.Header> {/* Modal title */}
    <Modal.Body>
      <p>
        Are you sure you want to delete user <strong>{user.name}</strong>?{" "}
        {/* Confirmation message with user name */}
      </p>
      <Alert variant="danger" className="mt-3">
        This action cannot be undone.
      </Alert>
    </Modal.Body>
    <Modal.Footer>
      <Button variant="secondary" onClick={onCancel} disabled={isDeleting}>
        {" "}
        {/* Cancel button */}
        Cancel
      </Button>
      <Button variant="danger" onClick={onConfirm} disabled={isDeleting}>
        {" "}
        {/* Confirm delete button */}
        {isDeleting ? "Deleting..." : "Delete"}
      </Button>
    </Modal.Footer>
  </Modal>
);
export default UserDeleteModal;
