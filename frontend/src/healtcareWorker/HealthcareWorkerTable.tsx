import React from "react";
import { Button, Table } from "react-bootstrap";
import { HealthcareWorker } from "../types/healthcareWorker";
import { Link } from "react-router-dom";

interface Props {
  workers: HealthcareWorker[];
  isAdmin: boolean;
  onDeleteClick: (worker: HealthcareWorker) => void;
}

const HealthcareWorkerTable: React.FC<Props> = ({ workers, isAdmin, onDeleteClick }) => {
  return (
    <Table striped bordered hover responsive>
      <thead>
        <tr>
          <th>#</th>
          {isAdmin && <th>Healthcare Worker ID</th>}
          <th>Name</th>
          <th>Address</th>
          <th>Phone Number</th>
          <th>Actions</th>
        </tr>
      </thead>
      <tbody>
        {workers.length === 0 ? (
          <tr>
            <td className="text-center text-muted">
              No healthcare workers found.
            </td>
          </tr>
        ) : (
          workers.map((worker, index) => (
            <tr key={worker.healthcareWorkerId}>
              <td>{index + 1}</td>
              {isAdmin && <td>{worker.healthcareWorkerId}</td>}
              <td>{worker.name}</td>
              <td>{worker.address}</td>
              <td>{worker.phone}</td>
              <td>
                {isAdmin ? (
                  <Link
                  to={`/healthcareworker/${worker.healthcareWorkerId}/update`}
                  className="btn btn-sm btn-primary me-2"
                >
                  Update
                </Link>
                ) : (
                  <Link
                  to={`/healthcareworker/${worker.healthcareWorkerId}/view`}
                  className="btn btn-sm btn-primary me-2"
                >Update </Link>
                )}
                <Button variant="danger" size="sm" onClick={() => onDeleteClick(worker)}>
                  Delete
                </Button>
              </td>
            </tr>
          ))
        )}
      </tbody>
    </Table>
  );
};

export default HealthcareWorkerTable;