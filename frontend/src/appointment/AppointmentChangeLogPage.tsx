import React, { useEffect, useState } from "react";
import { ChangeLog } from "../types/appointment";
import * as appointmentService from "./appointmentService";
import { useNavigate, useParams } from "react-router-dom";
import Loading from "../shared/Loading";
import { Alert, Button } from "react-bootstrap";
import AppointmentChangeLogTable from "./AppointmentChangeLogTable";

const AppointmentChangeLogPage: React.FC = () => {
  // Get appointment id from URL parameters
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const onBack = () => {
    navigate(-1);
  };

  // State for change logs, fetching status, and errors
  const [changeLogs, setChangeLogs] = useState<ChangeLog[]>([]);
  const [isFetching, setIsFetching] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  // Fetch change logs when component mounts or id changes
  useEffect(() => {
    const fetchChangeLogs = async () => {
      // Set fetching state and clear previous errors
      setIsFetching(true);
      setError(null);
      try {
        // Fetch change logs for the appointment
        const data = await appointmentService.fetchChangeLog(Number(id));
        setChangeLogs(data);
      } catch (error) {
        // Log and set error message
        console.error(error);
        setError("Failed to fetch change logs. Try again later.");
      } finally {
        // Clear fetching state
        setIsFetching(false);
      }
    };
    fetchChangeLogs();
  }, [id]);

  // Render the change log page. Show loading, error, or change log table
  return (
    <div>
      <h2>Change Log</h2>
      {isFetching && <Loading />}
      {error && (
        <Alert variant="danger" className="mt-3">
          {error}
        </Alert>
      )}

      {!isFetching && !error && <AppointmentChangeLogTable changeLogs={changeLogs} />}
      <Button variant="secondary" className="mt-3" onClick={onBack}>
        Back
      </Button>
    </div>
  );
};

export default AppointmentChangeLogPage;
