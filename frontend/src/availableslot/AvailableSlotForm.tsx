import React, { useEffect, useState } from "react";
import { Button, Form, Row, Col, Container, Spinner } from "react-bootstrap";
import { AvailableSlot } from "../types/availableSlot";
import { useNavigate } from "react-router-dom";
import { HealthcareWorker } from "../types/healthcareWorker";
import { addMinutes, combineDateTime, pad, quarterOptions, toDateInput } from "../shared/timeUtils";

// Props for the AvailableSlotForm component
interface AvailableSlotFormProps {
  onAvailableSlotChanged: (slot: AvailableSlot) => void;
  isUpdating?: boolean;
  initialData?: AvailableSlot; // Include id when updating
  workers?: HealthcareWorker[];
  isAdmin?: boolean;
  availableSlotId?: number;
  serverError?: string | null;
  isSubmitting?: boolean;
}

// Shared Form Component for creating and updating available slots
const AvailableSlotForm: React.FC<AvailableSlotFormProps> = ({
  onAvailableSlotChanged,
  isUpdating = false,
  initialData,
  workers = [],
  isAdmin = false,
  availableSlotId,
  serverError = null,
  isSubmitting,
}) => {
  // State for date and time inputs
  const [dateStr, setDateStr] = useState<string>("");
  const [timeStr, setTimeStr] = useState<string>("");

  // State for admin to select worker
  const [selectedWorkerId, setSelectedWorkerId] = useState<number | null>(null);

  // Handling error and loading
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();

  // Set default selected worker for admin when creating
  useEffect(() => {
    if (isAdmin && !isUpdating && workers.length > 0 && selectedWorkerId === null) {
      setSelectedWorkerId(workers[0].id);
    }
  }, [workers, isAdmin, isUpdating, selectedWorkerId]);

  // Initialize date and time fields
  useEffect(() => {
    // Function to round current time to next quarter-hour
    const nowRounded = () => {
      // Round "now" to the next 15-min mark
      const now = new Date();
      const remainder = now.getMinutes() % 15;
      if (remainder) now.setMinutes(now.getMinutes() + (15 - remainder), 0, 0);
      else now.setSeconds(0, 0);
      return now;
    };

    // If updating, use initial data, otherwise use rounded now. Changes when isUpdating or initialData changes.
    const base =
      isUpdating && initialData
        ? new Date(initialData.start) // ISO string from object to Date object
        : nowRounded();

    // Set date and time strings
    setDateStr(toDateInput(base));
    setTimeStr(`${pad(base.getHours())}:${pad(Math.floor(base.getMinutes() / 15) * 15)}`); // Set to nearest quarter
  }, [isUpdating, initialData]);

  // Calculate end time based on start time
  const startDate = combineDateTime(dateStr, timeStr);
  const endDate = addMinutes(startDate, 60);
  const endTimeStr = `${pad(endDate.getHours())}:${pad(endDate.getMinutes())}`;

  // Handle start time change
  const handleStartTimeChange = (value: string) => {
    setTimeStr(value);
  };

  // Handle form submission
  const handleSubmit = (e: React.FormEvent) => {
    // Prevent default form submission
    e.preventDefault();

    // Clear previous errors
    setError(null);

    // Admin must select a worker when creating
    const effectiveWorkerId: number | null = isAdmin
      ? isUpdating
        ? initialData?.healthcareWorkerId ?? null // If admin updating: keep existing id
        : selectedWorkerId // If admin creating: must pick worker id
      : isUpdating
      ? initialData?.healthcareWorkerId ?? null // If worker updating: keep existing id
      : null; // If worker creating: will be set server-side

    // Admin must select a worker only when creating a new slot
    if (isAdmin && !isUpdating && !selectedWorkerId) {
      setError("Please choose a healthcare worker.");
      return;
    }

    // Validate start and end times
    if (!startDate || !endDate) {
      setError("Start and end are required.");
      return;
    }

    // Ensure end is after start
    if (endDate <= startDate) {
      setError("End must be after start.");
      return;
    }

    // Construct available slot object
    const availableSlot: AvailableSlot = {
      id: availableSlotId,
      healthcareWorkerId: effectiveWorkerId ?? 0, // Will be set server-side if null
      start: startDate.toISOString(),
      end: endDate.toISOString(),
      isBooked: false,
    };
    onAvailableSlotChanged(availableSlot);
  };

  const onCancel = () => navigate(-1);

  // Render the form
  return (
    <Form onSubmit={handleSubmit}>
      <Row className="mb-3 justify-content-center">
        <Col md={4}>
          {/* If admin and creating show healthcare worker selection */}
          {isAdmin && !isUpdating && (
            <Form.Group controlId="slotWorker" className="mb-3">
              <Form.Label>Healthcare worker</Form.Label>
              <Form.Select
                value={selectedWorkerId!} // always a string
                onChange={(e) => setSelectedWorkerId(Number(e.target.value))}
                required>
                {(!workers || workers?.length === 0) && <option value="">Loading…</option>}
                {workers?.map((w) => (
                  <option key={w.id} value={w.id}>
                    {`${w.name} - #${w.id}`}
                  </option>
                ))}
              </Form.Select>
              <Form.Text muted>Select who this slot belongs to.</Form.Text>
            </Form.Group>
          )}

          {/* If admin and updating show read-only healthcare worker ID */}
          {isAdmin && isUpdating && (
            <Form.Group controlId="slotWorkerReadonly" className="mb-3">
              <Form.Label>Healthcare worker</Form.Label>
              <Form.Control
                readOnly
                className="bg-light border-primary fw-semibold text-center"
                value={initialData?.healthcareWorkerId ?? ""}
              />
            </Form.Group>
          )}
          <Form.Group controlId="slotDate" className="mb-3">
            <Form.Label>Date</Form.Label>
            <Form.Control type="date" value={dateStr} onChange={(e) => setDateStr(e.target.value)} required />
          </Form.Group>

          <Form.Group controlId="slotStartTime" className="mb-3">
            <Form.Label>Start (15-min)</Form.Label>
            <Form.Select value={timeStr} onChange={(e) => handleStartTimeChange(e.target.value)} required>
              {quarterOptions.map((t) => (
                <option key={t} value={t}>
                  {t}
                </option>
              ))}
            </Form.Select>
            <Form.Text muted>Only quarter-hour times shown.</Form.Text>
          </Form.Group>

          <Form.Group controlId="slotEndTime" className="mb-3">
            <Form.Label>End</Form.Label>
            <Form.Control value={endTimeStr} readOnly disabled />
            <Form.Text muted>Automatically 1 hour after start.</Form.Text>
          </Form.Group>
        </Col>
      </Row>

      {/* Display errors if any */}
      {error && <div className="text-danger mb-3">{error}</div>}
      {serverError && <div className="text-danger mb-3">{serverError}</div>}

      {/* Form buttons */}
      <Container className="my-4">
        <Button variant="primary" type="submit" className="me-2" disabled={isSubmitting}>
          {/* When submitting, show spinner */}
          {isSubmitting ? (
            <>
              <Spinner as="span" animation="border" size="sm" role="status" aria-hidden="true" className="me-2" />
              Saving…
            </>
          ) : isUpdating ? (
            "Update Available Slot"
          ) : (
            "Create Available Slot"
          )}
        </Button>
        <Button variant="secondary" type="button" disabled={isSubmitting} onClick={onCancel}>
          Cancel
        </Button>
      </Container>
    </Form>
  );
};

export default AvailableSlotForm;
