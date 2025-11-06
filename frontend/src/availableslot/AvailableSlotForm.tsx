import React, { useEffect, useMemo, useState } from "react";
import { Button, Form, Row, Col, Container } from "react-bootstrap";
import { AvailableSlot } from "../types/availableSlot";
import { useNavigate } from "react-router-dom";
import Loading from "../shared/Loading";
import { HealthcareWorker } from "../types/healthcareWorker";

interface AvailableSlotFormProps {
  onAvailableSlotChanged: (slot: AvailableSlot) => void;
  isUpdating?: boolean;
  initialData?: AvailableSlot; // Include id when updating
  workers?: HealthcareWorker[];
  isAdmin?: boolean;
  availableSlotId?: number;
  serverError?: string | null;
}

// Helper function for date and time
const pad = (n: number) => String(n).padStart(2, "0");

const toDateInput = (d: Date) => `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`;

const addMinutes = (d: Date, mins: number) => {
  const copy = new Date(d);
  copy.setMinutes(copy.getMinutes() + mins);
  return copy;
};

const quarterOptions = Array.from({ length: 24 * 4 }, (_, i) => {
  const h = Math.floor(i / 4);
  const m = (i % 4) * 15;
  return `${pad(h)}:${pad(m)}`;
});

const combineDateTime = (dateStr: string, timeStr: string) => {
  const [y, mo, d] = dateStr.split("-").map(Number);
  const [h, mi] = timeStr.split(":").map(Number);
  return new Date(y, mo - 1, d, h, mi, 0, 0);
};

const AvailableSlotForm: React.FC<AvailableSlotFormProps> = ({
  onAvailableSlotChanged,
  isUpdating = false,
  initialData,
  workers = [],
  isAdmin = false,
  availableSlotId,
  serverError = null,
}) => {
  const [dateStr, setDateStr] = useState<string>("");
  const [timeStr, setTimeStr] = useState<string>("");
  // For admin to select worker
  const [selectedWorkerId, setSelectedWorkerId] = useState<number | null>(null);

  // Handling error and loading
  const [error, setError] = useState<string | null>(null);
  // const [isLoading, setIsLoading] = useState<boolean>(false);
  const navigate = useNavigate();

  // Init date
  useEffect(() => {
    const nowRounded = () => {
      // Round "now" to the next 15-min mark
      const now = new Date();
      const remainder = now.getMinutes() % 15;
      if (remainder) now.setMinutes(now.getMinutes() + (15 - remainder), 0, 0);
      else now.setSeconds(0, 0);
      return now;
    };

    const base =
      isUpdating && initialData
        ? new Date(initialData.start) // ISO string from object to Date object
        : nowRounded();

    setDateStr(toDateInput(base));
    setTimeStr(`${pad(base.getHours())}:${pad(Math.floor(base.getMinutes() / 15) * 15)}`); // Set to nearest quarter
  }, [isUpdating, initialData]);

  console.log(selectedWorkerId);

  // Compute end
  const startDate = combineDateTime(dateStr, timeStr);
  const endDate = addMinutes(startDate, 60);
  const endTimeStr = `${pad(endDate.getHours())}:${pad(endDate.getMinutes())}`;

  const handleStartTimeChange = (value: string) => {
    setTimeStr(value);
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    // Admin must select a worker when creating
    const effectiveWorkerId: number | null = isAdmin
      ? isUpdating
        ? initialData?.healthcareWorkerId ?? null // admin updating: keep existing id
        : selectedWorkerId // admin creating: must pick
      : isUpdating
      ? initialData?.healthcareWorkerId ?? null // worker updating: keep existing id
      : null;

    // Admin must select a worker only when creating a new slot
    if (isAdmin && !isUpdating && !selectedWorkerId) {
      setError("Please choose a healthcare worker.");
      return;
    }

    if (!startDate || !endDate) {
      setError("Start and end are required.");
      return;
    }
    if (endDate <= startDate) {
      setError("End must be after start.");
      return;
    }

    const availableSlot: AvailableSlot = {
      id: availableSlotId,
      healthcareWorkerId: effectiveWorkerId ?? 0, // TODO:
      start: startDate.toISOString(),
      end: endDate.toISOString(),
      isBooked: false,
    };
    onAvailableSlotChanged(availableSlot);
  };

  const onCancel = () => navigate(-1);

  return (
    <Form onSubmit={handleSubmit}>
      <Row className="mb-3 justify-content-center">
        <Col md={4}>
          {/* Admin-only: pick worker */}
          {isAdmin && !isUpdating && (
            <Form.Group controlId="slotWorker" className="mb-3">
              <Form.Label>Healthcare worker</Form.Label>
              <Form.Select
                value={selectedWorkerId!} // always a string
                onChange={(e) => setSelectedWorkerId(Number(e.target.value))}
                required>
                {(!workers || workers?.length === 0) && <option value="">Loading…</option>}
                {workers?.map((w) => (
                  <option key={w.healthcareWorkerId} value={w.healthcareWorkerId}>
                    {`${w.name} - #${w.healthcareWorkerId}`}
                  </option>
                ))}
              </Form.Select>
              <Form.Text muted>Select who this slot belongs to.</Form.Text>
            </Form.Group>
          )}

          {/* Admin + UPDATE read read-only ID */}
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

      {error && <div className="text-danger mb-3">{error}</div>}
      {serverError && <div className="text-danger mb-3">{serverError}</div>}

      <Container className="my-4">
        <Button variant="primary" type="submit" className="me-2">
          {isUpdating ? "Update Available Slot" : "Create Available Slot"}
        </Button>
        <Button variant="secondary" type="button" onClick={onCancel}>
          Cancel
        </Button>
      </Container>
    </Form>
  );
};

export default AvailableSlotForm;
