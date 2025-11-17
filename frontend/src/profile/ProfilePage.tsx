import React, { useEffect } from "react";
import * as HealthcareWorkerService from "../healtcareWorker/healthcareWorkerService";
import * as ClientService from "../client/clientService";
import { useAuth } from "../auth/AuthContext";
import UserDetailsCard from "../shared/user/UserDetailsCard";
import { Client } from "../types/client";
import { HealthcareWorker } from "../types/healthcareWorker";
import Loading from "../shared/Loading";
import UserDeleteModal from "../shared/user/UserDeleteModal";
import { useState } from "react";

const ProfilePage: React.FC = () => {
  const { user, hasRole, logout } = useAuth();
  const [profileData, setProfileData] = useState<Client | HealthcareWorker | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const role = user?.role;
  const [isDeleting, setIsDeleting] = useState(false);
  const [toDelete, setToDelete] = useState<HealthcareWorker | Client | null>(null);

  // Fetch profile data
  const fetchProfileData = async () => {
    setLoading(true);
    setError(null);

    try {
      if (hasRole("HealthcareWorker")) {
        // Fetch healthcare worker data if user is a worker
        const worker = await HealthcareWorkerService.fetchWorkerBySelf();
        setProfileData(worker);
      } else if (hasRole("Client")) {
        // Fetch client data if user is a client
        const client = await ClientService.fetchClientBySelf();
        setProfileData(client);
      } else {
        setError("Unknown user role");
      }
    } catch (error: any) {
      console.error("Error fetching profile data:", error);
      setError("Failed to fetch profile data");
    } finally {
      setLoading(false);
    }
  };
  useEffect(() => {
    fetchProfileData();
  }, []);

  // Confirm delete user
  const confirmDelete = async (userToDelete: Client | HealthcareWorker) => {
    // If no user to delete, return
    if (!userToDelete?.id) return;

    // Set loading and clear previous errors
    setError(null);
    setIsDeleting(true);

    // Attempt to delete user based on role
    try {
      if (role === "HealthcareWorker") {
        await HealthcareWorkerService.deleteWorker(userToDelete.id);
      } else if (role === "Client") {
        await ClientService.deleteClient(userToDelete.id);
      } else {
        throw new Error("Unknown user role");
      }

      // If the deleted user is the logged-in user, log them out
      if (hasRole("HealthcareWorker") || hasRole("Client")) {
        logout();
      }

      setToDelete(null);
    } catch (error) {
      console.error("Error deleting user: ", error);
      setError("Error deleting user. Try again later.");
    } finally {
      setIsDeleting(false);
    }
  };

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
              onConfirm={() => confirmDelete(toDelete)}
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

export default ProfilePage;
