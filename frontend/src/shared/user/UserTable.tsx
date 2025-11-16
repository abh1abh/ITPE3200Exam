import React from "react";
import { Button, Table } from "react-bootstrap";
import { HealthcareWorker } from "../../types/healthcareWorker";
import { Client } from "../../types/client";
import { Link } from "react-router-dom";

interface Props {
  users: HealthcareWorker[] | Client[];
  isHealthcareWorker: boolean;
  isAdmin: boolean;
  onDeleteClick: (user: HealthcareWorker | Client) => void;
}
// User table component. Displays a list of users with actions to view details, update, or delete, based on user type.
const UserTable: React.FC<Props> = ({ users, isHealthcareWorker, onDeleteClick }) => {
  return (
    <Table striped bordered hover responsive>
      <thead>
        <tr>
          <th>ID</th>
          <th>Name</th>
          <th>Address</th>
          <th>Phone Number</th>
          <th>Actions</th>
        </tr>
      </thead>
      <tbody>
        {users.length === 0 ? (
          <tr>
            <td colSpan={6} className="text-center text-muted">
              No {isHealthcareWorker ? "healthcare workers" : "clients"} found.{" "}
              {/* conditional message when no users are found based on role */}
            </td>
          </tr>
        ) : (
          users.map((user) => (
            <tr key={user.id}>
              <td>{user.id}</td>
              <td>{user.name}</td>
              <td>{user.address}</td>
              <td>{user.phone}</td>
              <td>
                <Link // Navigate to details page based on user role
                  to={`/${isHealthcareWorker ? "healthcareworker" : "client"}/${user.id}/details`}
                  className="btn btn-sm btn-primary me-2">
                  Details
                </Link>
                <Link // Navigate to update page based on user role
                  to={`/${isHealthcareWorker ? "healthcareworker" : "client"}/${user.id}/update`}
                  className="btn btn-sm btn-primary me-2">
                  Update
                </Link>
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
