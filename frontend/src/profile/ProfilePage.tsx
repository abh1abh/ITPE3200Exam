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
import { Link, useNavigate } from "react-router-dom";
import { Button } from "react-bootstrap";

const ProfilePage: React.FC = () => {
  const { hasRole, logout } = useAuth();
  const [profileData, setProfileData] = useState<Client | HealthcareWorker | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const isHealthcareWorker = hasRole("HealthcareWorker");
  const isClient = hasRole("Client");

  const [isDeleting, setIsDeleting] = useState<boolean>(false);
  const [toDelete, setToDelete] = useState<HealthcareWorker | Client | null>(null);

  const navigate = useNavigate();

  // Fetch profile data
  const fetchProfileData = async () => {
    setLoading(true);
    setError(null);

    try {
      if (isHealthcareWorker) {
        // Fetch healthcare worker data if user is a worker
        const worker = await HealthcareWorkerService.fetchWorkerBySelf();
        setProfileData(worker);
      } else if (isClient) {
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
      if (isHealthcareWorker) {
        await HealthcareWorkerService.deleteWorker(userToDelete.id);
      } else if (isClient) {
        await ClientService.deleteClient(userToDelete.id);
      } else {
        throw new Error("Unknown user role");
      }

      // If the deleted user is the logged-in user, log them out
      if (isHealthcareWorker || isClient) {
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
          <UserDetailsCard user={profileData} />
          <Button variant="secondary" className="me-2" onClick={() => navigate(-1)}>
            Back
          </Button>
          <Link
            to={
              `/${isHealthcareWorker ? "healthcareworker" : "client"}/${profileData.id}/update` // Navigate to update page based on user role
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
