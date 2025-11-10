import React, { useEffect, useState } from "react";
import { ChangeLog } from "../types/appointment";
import * as appointmentService from "./appointmentService";
import { useParams } from "react-router-dom";
import Loading from "../shared/Loading";
import { Alert, Table } from "react-bootstrap";
import AppointmentChangeLogTable from "./AppointmentChangeLogTable";

const AppointmentChangeLogPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();

  const [changeLogs, setChangeLogs] = useState<ChangeLog[]>([]);
  const [isFetching, setIsFetching] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchChangeLogs = async () => {
      try {
        setIsFetching(true);
        const data = await appointmentService.fetchChangeLog(Number(id));
        setChangeLogs(data);
      } catch (error) {
        console.error(error);
        setError("Failed to fetch change logs. Try again later.");
      } finally {
        setIsFetching(false);
      }
    };
    fetchChangeLogs();
  }, [id]);
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
    </div>
  );
};

export default AppointmentChangeLogPage;
