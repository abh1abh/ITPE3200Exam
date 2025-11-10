import React from "react";
import { Button, Table } from "react-bootstrap";
import { HealthcareWorker } from "../types/healthcareWorker";
import { Client } from "../types/client";
import { Link } from "react-router-dom";

interface Props {
  user: HealthcareWorker[] | Client[];
  isAdmin: boolean;
  onDeleteClick: (user: HealthcareWorker | Client) => void;
}

const UserTable: React.FC<Props> = ({ user, isAdmin, onDeleteClick }) => {
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
        {user.length === 0 ? (
          <tr>
            <td className="text-center text-muted">
              No healthcare workers found.
            </td>
          </tr>
        ) : (
          user.map((user, index) => (
            <tr key={user.id}>
              <td>{index + 1}</td>
              {isAdmin && <td>{user.id}</td>}
              <td>{user.name}</td>
              <td>{user.address}</td>
              <td>{user.phone}</td>
              <td>
                {isAdmin ? (
                  <Link
                  to={`/healthcareworker/${user.id}/update`}
                  className="btn btn-sm btn-primary me-2"
                >
                  Update
                </Link>
                ) : (
                  <Link
                  to={`/healthcareworker/${user.id}/view`}
                  className="btn btn-sm btn-primary me-2"
                >Update </Link>
                )}
                <Button variant="danger" size="sm" onClick={() => onDeleteClick(user)}>
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

export default UserTable;