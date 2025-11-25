import React, { useEffect } from "react";
import * as HealthcareWorkerService from "./healthcareWorkerService";
import { useAuth } from "../auth/AuthContext";
import UserDetailsCard from "../shared/user/UserDetailsCard";
import { HealthcareWorker } from "../types/healthcareWorker";
import Loading from "../shared/Loading";
import UserDeleteModal from "../shared/user/UserDeleteModal";
import { useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import Button from "react-bootstrap/esm/Button";

const HealthcareWorkerDetailsPage: React.FC = () => {
  // Get worker ID from URL params
  const { id } = useParams<{ id: string }>();
  const { hasRole } = useAuth();

  // States for profile data, loading, errors, and deletion
  const [profileData, setProfileData] = useState<HealthcareWorker | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [isDeleting, setIsDeleting] = useState(false);
  const [toDelete, setToDelete] = useState<HealthcareWorker | null>(null);
  const navigate = useNavigate();

  // Check if the current user is a healthcare worker
  const isHealthcareWorker = hasRole("HealthcareWorker");

  // Fetch profile data
  const fetchProfileData = async () => {
    setLoading(true);
    setError(null);
    try {
      const worker = await HealthcareWorkerService.fetchWorker(Number(id)); // Fetch worker data using the service
      setProfileData(worker); // Set the fetched data to state
    } catch (error: any) {
      console.error("Error fetching profile data:", error);
      setError("Failed to fetch profile data");
    } finally {
      setLoading(false);
    }
  };
  // On component mount
  useEffect(() => {
    fetchProfileData();
  }, []);

  const confirmDeleteWorker = async (
    user: HealthcareWorker // Confirm deletion of worker
  ) => {
    if (!toDelete?.id) return; // No worker to delete
    setError(null);
    setIsDeleting(true);
    setToDelete(user); // Set the worker to be deleted
    try {
      await HealthcareWorkerService.deleteWorker(toDelete.id); // Call delete service
      setToDelete(null);
      if (isHealthcareWorker) {
        // If the deleted user is viewing their own profile, redirect to home
        navigate("/");
      } else {
        navigate("/healthcareworkers"); // Redirect to workers list after deletion
      }
    } catch (error) {
      console.error("Error deleting Worker: ", error);
      setError("Error deleting Worker. Try again later.");
      setToDelete(null);
    } finally {
      setIsDeleting(false);
    }
  };

  return (
    <div>
      <h2>Healthcare Worker profile</h2>
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
              `/healthcareworker/${profileData.id}/update` // Navigate to update page based on user role
            }
            className="btn btn-primary me-2">
            Update
          </Link>
          <Button variant="danger" onClick={() => confirmDeleteWorker(profileData)}>
            Delete
          </Button>

          {/* Reusable user details card component */}
          {toDelete && (
            <UserDeleteModal // Reusable user delete confirmation modal
              user={toDelete} // User to be deleted
              onCancel={() => setToDelete(null)} // Cancel deletion
              onConfirm={() => confirmDeleteWorker(toDelete as HealthcareWorker)} // Confirm deletion
              isDeleting={isDeleting} // Deletion in progress state
            />
          )}
        </>
      ) : (
        <p>No profile data available.</p>
      )}
    </div>
  );
};

export default HealthcareWorkerDetailsPage;
