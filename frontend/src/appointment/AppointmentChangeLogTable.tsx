import React from "react";
import { Table } from "react-bootstrap";
import { ChangeLog } from "../types/appointment";

interface AppointmentChangeLogTableProps {
  changeLogs: ChangeLog[];
}

const AppointmentChangeLogTable: React.FC<AppointmentChangeLogTableProps> = ({ changeLogs }) => {
  return (
    <Table striped bordered hover size="sm" className="mt-3">
      <thead>
        <tr>
          <th>Date</th>
          <th>Changed by</th>
          <th>Description</th>
        </tr>
      </thead>

      <tbody>
        {changeLogs.length === 0 ? (
          <tr>
            <td colSpan={3} className="text-center text-muted">
              No changes
            </td>
          </tr>
        ) : (
          changeLogs.map((log) => (
            <tr key={log.id}>
              <td>{new Date(log.changeDate).toLocaleString()}</td>
              <td>{log.changedByUserId}</td>
              <td>{log.changeDescription}</td>
            </tr>
          ))
        )}
      </tbody>
    </Table>
  );
};

export default AppointmentChangeLogTable;
