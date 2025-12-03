import React, { useEffect } from "react";
import * as ClientService from "./clientService";
import UserDetailsCard from "../shared/user/UserDetailsCard";
import { Client } from "../types/client";
import Loading from "../shared/Loading";
import UserDeleteModal from "../shared/user/UserDeleteModal";
import { useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { Button } from "react-bootstrap";
import { useAuth } from "../auth/AuthContext";

const ClientDetailsPage: React.FC = () => {
  // Get client ID from URL params
  const { id } = useParams<{ id: string }>();

  // States for profile data, loading, errors, and deletion
  const [profileData, setProfileData] = useState<Client | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [isDeleting, setIsDeleting] = useState<boolean>(false);
  const [toDelete, setToDelete] = useState<Client | null>(null);

  const navigate = useNavigate();

  // Fetch profile data on component mount
  const fetchProfileData = async () => {
    setLoading(true); // Set loading to true before fetch
    setError(null); // Clear previous errors
    try {
      const clientDto = await ClientService.fetchClient(Number(id)); // Fetch client data using the service
      const client: Client = {
        //map ClientDto to Client
        id: clientDto.id,
        name: clientDto.name,
        email: clientDto.email,
        phone: clientDto.phone,
        address: clientDto.address,
      };
      setProfileData(client); // Set the fetched data to state
    } catch (error: any) {
      console.error("Error fetching profile data:", error);
      setError("Failed to fetch profile data");
    } finally {
      setLoading(false); // Set loading to false after fetch attempt
    }
  };

  // On component mount
  useEffect(() => {
    fetchProfileData();
  }, []);

  // Confirm deletion of client
  const confirmDeleteClient = async (user: Client) => {
    if (!toDelete?.id) return; // No client to delete
    setError(null); // Clear previous errors
    setIsDeleting(true); // Set deleting state
    setToDelete(user as Client); // Set the client to be deleted
    try {
      await ClientService.deleteClient(toDelete.id); // Call delete service
      navigate(-1); // Navigate back after deletion
      setToDelete(null);
    } catch (error) {
      console.error("Error deleting Client: ", error);
      setError("Error deleting Client. Try again later.");
      setToDelete(null);
    } finally {
      setIsDeleting(false); // Reset deleting state
    }
  };

  // Render profile component
  return (
    <div>
      <h2>Client profile</h2>
      {loading ? (
        <Loading />
      ) : error ? (
        <p style={{ color: "red" }}>{error}</p>
      ) : profileData ? (
        <>
          <UserDetailsCard user={profileData} />
          <Button variant="secondary" className="me-2" onClick={() => navigate(-1)}>
            Back
          </Button>
          <Link
            to={
              `/client/${profileData.id}/update` // Navigate to update page based on user role
            }
            className="btn btn-primary me-2">
            Update
          </Link>
          <Button variant="danger" onClick={() => setToDelete(profileData)}>
            Delete
          </Button>

          {toDelete && (
            <UserDeleteModal
              user={toDelete}
              onCancel={() => setToDelete(null)}
              onConfirm={() => confirmDeleteClient(toDelete)}
              isDeleting={isDeleting}
            />
          )}
        </>
      ) : (
        <p>No profile data available.</p>
      )}
    </div>
  );
};

export default ClientDetailsPage;
