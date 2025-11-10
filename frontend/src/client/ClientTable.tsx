import React from "react";
import { Button, Table } from "react-bootstrap";
import { Client } from "../types/client";
import { Link } from "react-router-dom";

interface Props {
  clients: Client[];
  isAdmin: boolean;
  onDeleteClick: (client: Client) => void;
}

const ClientTable: React.FC<Props> = ({ clients, isAdmin, onDeleteClick }) => {
  return (
    <Table striped bordered hover responsive>
      <thead>
        <tr>
          <th>#</th>
          {isAdmin && <th>Client ID</th>}
          <th>Name</th>
          <th>Address</th>
          <th>Phone Number</th>
          <th>Actions</th>
        </tr>
      </thead>
      <tbody>
        {clients.length === 0 ? (
          <tr>
            <td className="text-center text-muted">
              No healthcare workers found.
            </td>
          </tr>
        ) : (
          clients.map((client, index) => (
            <tr key={client.id}>
              <td>{index + 1}</td>
              {isAdmin && <td>{client.id}</td>}
              <td>{client.name}</td>
              <td>{client.address}</td>
              <td>{client.phone}</td>
              <td>
                {isAdmin ? (
                  <Link
                  to={`/client/${client.id}/update`}
                  className="btn btn-sm btn-primary me-2"
                >
                  Update
                </Link>
                ) : (
                  <Link
                  to={`/client/${client.id}/view`}
                  className="btn btn-sm btn-primary me-2"
                >Update </Link>
                )}
                <Button variant="danger" size="sm" onClick={() => onDeleteClick(client)}>
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

export default ClientTable;