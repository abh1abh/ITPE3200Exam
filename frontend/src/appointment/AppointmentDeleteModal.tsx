import { Alert, Button, Modal, Spinner } from "react-bootstrap";
import { AppointmentView } from "../types/appointment";
import { formatDateOnly, formatTimeOnly } from "../shared/timeUtils";

// Props for AppointmentDeleteModal component
type AppointmentDeleteModalProps = {
  appointment: AppointmentView;
  onConfirm: () => void;
  onCancel: () => void;
  isDeleting?: boolean;
};

const AppointmentDeleteModal: React.FC<AppointmentDeleteModalProps> = ({
  appointment,
  onConfirm,
  onCancel,
  isDeleting,
}) => (
  // Modal for confirming appointment deletion
  <Modal show onHide={onCancel}>
    <Modal.Header closeButton>Cancel appointment?</Modal.Header>
    <Modal.Body>
      <p>
        <strong>Client:</strong> {appointment.clientName}
      </p>
      <p>
        <strong>Worker:</strong> {appointment.healthcareWorkerName}
      </p>
      <p>
        <strong>Date:</strong> {formatDateOnly(appointment.start)}
      </p>
      <p>
        <strong>Time:</strong> {formatTimeOnly(appointment.start)} - {formatTimeOnly(appointment.end)}
      </p>
      <Alert variant="danger" className="mt-3">
        This action cannot be undone.
      </Alert>
    </Modal.Body>
    {/* Buttons for confirming or cancelling deletion */}
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

export default AppointmentDeleteModal;
