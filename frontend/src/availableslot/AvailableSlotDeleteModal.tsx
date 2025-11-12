import { Alert, Button, Modal } from "react-bootstrap";
import { AvailableSlot } from "../types/availableSlot";

// Props for the DeleteAvailableSlotModal component
type DeleteAvailableSlotModalProps = {
  availableSlot: AvailableSlot;
  onConfirm: () => void;
  onCancel: () => void;
  isDeleting: boolean;
};

const DeleteAvailableSlotModal: React.FC<DeleteAvailableSlotModalProps> = ({
  availableSlot,
  onConfirm,
  onCancel,
  isDeleting,
}) => (
  // Render modal to confirm deletion of available slot
  <Modal show onHide={onCancel}>
    <Modal.Header closeButton>Delete available slot?</Modal.Header>
    <Modal.Body>
      <p>
        <strong>Worker:</strong> #{availableSlot.healthcareWorkerId}
      </p>
      <p>
        <strong>Start:</strong> {new Date(availableSlot.start).toLocaleString()}
      </p>
      <p>
        <strong>End:</strong> {new Date(availableSlot.end).toLocaleString()}
      </p>
      <Alert variant="danger">This action cannot be undone.</Alert>
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

export default DeleteAvailableSlotModal;
