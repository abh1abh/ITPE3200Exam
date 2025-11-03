import { Alert, Button, Modal } from "react-bootstrap";
import { AvailableSlot } from "../types/AvailableSlot";

type DeleteDialogProps = {
  availableSlot: AvailableSlot;
  onConfirm: () => void;
  onCancel: () => void;
};

const DeleteAvailableSlotModal: React.FC<DeleteDialogProps> = ({ availableSlot, onConfirm, onCancel }) => (
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
      <Button variant="secondary" onClick={onCancel}>
        Cancel
      </Button>
      <Button variant="danger" onClick={onConfirm} disabled={availableSlot.isBooked}>
        Delete
      </Button>
    </Modal.Footer>
  </Modal>
);

export default DeleteAvailableSlotModal;
