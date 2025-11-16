import React from "react";
import * as ClientService from "./clientService";
import { useAuth } from "../auth/AuthContext";
import UserDetailsCard from "../shared/user/UserDetailsCard";
import { Client } from "../types/client";
import Loading from "../shared/Loading";
import UserDeleteModal from "../shared/user/UserDeleteModal";
import { useState } from "react";
import { useParams } from "react-router-dom";

const ClientDetailsPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const { hasRole } = useAuth();
  const [profileData, setProfileData] = React.useState<Client | null>(null);
  const [loading, setLoading] = React.useState<boolean>(true);
  const [error, setError] = React.useState<string | null>(null);
  const [isDeleting, setIsDeleting] = useState(false);
  const [toDelete, setToDelete] = useState<Client | null>(null);

  // Fetch profile data on component mount
  const fetchProfileData = async () => {
    setLoading(true); // Set loading to true before fetch
    setError(null); // Clear previous errors
    try {
      const clientDto = await ClientService.fetchClient(Number(id)); // Fetch client data using the service
      const worker: Client = {
        //map ClientDto to Client
        id: clientDto.id,
        name: clientDto.name,
        email: clientDto.email,
        phone: clientDto.phone,
        address: clientDto.address,
      };
      setProfileData(worker); // Set the fetched data to state
    } catch (error: any) {
      console.error("Error fetching profile data:", error);
      setError("Failed to fetch profile data");
    } finally {
      setLoading(false); // Set loading to false after fetch attempt
    }
  };
  React.useEffect(() => {
    // On component mount
    fetchProfileData();
  }, []);

  const confirmDeleteClient = async (
    user: Client // Confirm deletion of client
  ) => {
    if (!toDelete?.id) return; // No client to delete
    setError(null); // Clear previous errors
    setIsDeleting(true); // Set deleting state
    setToDelete(user as Client); // Set the client to be deleted
    try {
      await ClientService.deleteClient(toDelete.id); // Call delete service
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
      <h2>Profile</h2>
      {loading ? (
        <Loading />
      ) : error ? (
        <p style={{ color: "red" }}>{error}</p>
      ) : profileData ? (
        <>
          <UserDetailsCard user={profileData} onDeleteClick={setToDelete} />
          {toDelete && (
            <UserDeleteModal
              user={toDelete}
              onCancel={() => setToDelete(null)}
              onConfirm={() => confirmDeleteClient(toDelete as Client)}
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
