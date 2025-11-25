import React, { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import * as clientService from "./clientService";
import { Client } from "../types/client";
import UserUpdateForm from "../shared/user/UserUpdateForm";
import Loading from "../shared/Loading";
import { Alert } from "react-bootstrap";
import { UpdateUserDto } from "../types/user";

const ClientUpdatePage: React.FC = () => {
  // Get client ID from URL params
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  // States for client data, loading, and errors
  const [client, setClient] = useState<Client | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [fetchError, setFetchError] = useState<string | null>(null);
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  useEffect(() => {
    // On component mount
    const fetchClient = async () => {
      // Fetch client data
      try {
        const data = await clientService.fetchClient(Number(id)); // Fetch client data using the service
        setClient(data); // Set the fetched data to state
      } catch (error) {
        console.error("Error fetching worker:", error);
        setFetchError("Failed to fetch worker data.");
      } finally {
        setLoading(false);
      }
    };
    if (id) fetchClient(); // Fetch client if id is present
  }, [id]);

  const handleClientUpdated = async (updated: UpdateUserDto) => {
    // Handle client update
    try {
      await clientService.updateClient(Number(id), updated); // Call update service
      setSuccess("Update successful!");
      // Redirect back to previous page after a delay
      setTimeout(() => navigate(-1), 1000); // Redirect after 1 second
    } catch (error) {
      console.error("Error update client:", error);
      setSubmitError("Failed to update client.");
    }
  };
  return (
    <div>
      <h2>Update Client</h2>
      {success && <Alert variant="success">{success}</Alert>} {/* Show success message */}
      {loading ? (
        <Loading />
      ) : !client ? ( // No client found, show warning
        <Alert variant="warning" className="mt-3">
          No client found.
        </Alert>
      ) : fetchError ? ( // Fetch error, show error message
        <Alert variant="danger" className="mt-3">
          {fetchError}
        </Alert>
      ) : (
        <UserUpdateForm // Reusable form component for updating user details
          profileUser={client}
          onUserChanged={handleClientUpdated}
          role="Client"
          serverError={submitError}
        />
      )}
    </div>
  );
};
export default ClientUpdatePage;
