import React, { useEffect, useState } from "react";
import { useAuth } from "../auth/AuthContext";
import * as ClientService from "./clientService";
import { Alert, Button } from "react-bootstrap";
import { Client } from "../types/client";
import UserTable from "../shared/user/UserTable";
import UserDeleteModal from "../shared/user/UserDeleteModal";

const ClientPage: React.FC = () => {
  const { hasRole } = useAuth();

  // States for profile data, loading, errors, and deletion
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [clients, setClients] = useState<Client[]>([]);
  const [toDelete, setToDelete] = useState<Client | null>(null);
  const [isDeleting, setIsDeleting] = useState(false);
  const [success, setSuccess] = useState<string | null>(null);

  // Fetch profile data on component mount
  const fetchClientData = async () => {
    setLoading(true); // Set loading to true before fetch
    setError(null); // Clear previous errors
    try {
      const response = await ClientService.fetchAllClients(); // Fetch client data using the service
      setClients(response); // Set the fetched data to state
    } catch (error: any) {
      console.error("Error fetching profile data:", error);
      setError("Failed to fetch profile data");
    } finally {
      setLoading(false);
      console.log("Finished loading");
    }
  };
  useEffect(() => {
    fetchClientData(); // On component mount
  }, []);

  // Confirm deletion of client
  const confirmDelete = async () => {
    if (!toDelete?.id) return; // return if no client ready to delete
    setError(null);
    setIsDeleting(true);
    try {
      await ClientService.deleteClient(toDelete.id); // Call delete service
      fetchClientData(); // Refresh client list after deletion
      setSuccess("Client deleted successfully.");
      setToDelete(null); // Clear the toDelete state
    } catch (error) {
      console.error("Error deleting Client: ", error);
      setError("Error deleting Client. Try again later.");
      setToDelete(null);
    } finally {
      setIsDeleting(false);
    }
  };

  return (
    <div>
      <h2>Clients</h2>
      <Button onClick={fetchClientData} className="btn btn-primary mb-3 me-2" disabled={loading}>
        {loading ? "Loading..." : "Refresh clients"}
      </Button>
      {!loading && error && <Alert variant="danger">{error}</Alert>}
      {success && <Alert variant="success">{success}</Alert>}

      {!loading && !error && (
        <>
          <UserTable // Reusable user table component
            users={clients}
            isHealthcareWorker={false}
            isAdmin={hasRole("Admin")}
            onDeleteClick={setToDelete}
          />
          {toDelete && ( // Show delete modal if a client is selected for deletion
            <UserDeleteModal
              user={toDelete}
              onCancel={() => setToDelete(null)}
              onConfirm={confirmDelete}
              isDeleting={isDeleting}
            />
          )}
        </>
      )}
    </div>
  );
};

export default ClientPage;
