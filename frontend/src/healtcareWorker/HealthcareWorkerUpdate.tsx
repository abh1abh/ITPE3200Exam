import React, { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import * as healthcareWorkerService from "./healthcareWorkerService";
import { HealthcareWorker } from "../types/healthcareWorker";
import UserUpdateForm from "../shared/user/UserUpdateForm";
import Loading from "../shared/Loading";
import { Alert } from "react-bootstrap";
import { UpdateUserDto } from "../types/user";

const HealthcareWorkerUpdatePage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [worker, setWorker] = useState<HealthcareWorker | null>(null);

  const [loading, setLoading] = useState<boolean>(true);
  const [fetchError, setFetchError] = useState<string | null>(null);
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  useEffect(() => {
    const fetchWorker = async () => {
      // Fetch worker data
      try {
        const data = await healthcareWorkerService.fetchWorker(Number(id)); // Fetch worker data using the service
        setWorker(data); // Set the fetched data to state
      } catch (error) {
        console.error("Error fetching worker:", error);
        setFetchError("Failed to fetch worker data.");
      } finally {
        setLoading(false);
      }
    };
    if (id) fetchWorker(); //dependency array
  }, [id]); // On component mount

  const handleWorkerUpdated = async (updated: UpdateUserDto) => {
    // Handle worker update
    try {
      await healthcareWorkerService.updateWorker(Number(id), updated); // Call update service
      setSuccess("Update successful!");
      // Redirect back to previous page after a delay
      setTimeout(() => navigate(-1), 1000); // Redirect after 1 second
    } catch (error) {
      console.error("Error update worker:", error);
      setSubmitError("Failed to update worker.");
    }
  };

  // Render the update form
  return (
    <div>
      <h2>Update Healthcare Worker</h2>
      {success && <Alert variant="success">{success}</Alert>}
      {loading ? ( // Show loading indicator
        <Loading />
      ) : !worker ? (
        <Alert
          variant="warning"
          className="mt-3" // Show warning if no worker found
        >
          No healthcare worker found.
        </Alert>
      ) : fetchError ? (
        <Alert
          variant="danger"
          className="mt-3" // Show fetch error message
        >
          {fetchError}
        </Alert>
      ) : (
        <UserUpdateForm // Reusable form component for updating user details
          profileUser={worker}
          role="HealthcareWorker"
          onUserChanged={handleWorkerUpdated}
          serverError={submitError}
        />
      )}
    </div>
  );
};

export default HealthcareWorkerUpdatePage;
