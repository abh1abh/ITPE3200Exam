import React, { useEffect, useState } from "react";
import { useAuth } from "../auth/AuthContext";
import * as HealthcareWorkerService from "../healtcareWorker/healthcareWorkerService";
import { Alert, Button } from "react-bootstrap";
import { HealthcareWorker } from "../types/healthcareWorker";
import UserTable from "../shared/user/UserTable";
import UserDeleteModal from "../shared/user/UserDeleteModal";

const HealthcareWorkerPage: React.FC = () => {
  const { hasRole } = useAuth();
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [workers, setClients] = useState<HealthcareWorker[]>([]);
  const [toDelete, setToDelete] = useState<HealthcareWorker | null>(null);
  const [isDeleting, setIsDeleting] = useState(false);
  const [success, setSuccess] = useState<string | null>(null);

  const fetchWorkerData = async () => {
    // Fetch profile data
    setLoading(true);
    setError(null);
    try {
      const response = await HealthcareWorkerService.fetchAllWorkers(); // Fetch worker data using the service
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
    fetchWorkerData();
  }, []);

  const confirmDelete = async () => {
    // Confirm deletion of worker
    if (!toDelete?.id) return;
    setError(null);
    setIsDeleting(true); // Deletion in progress
    try {
      await HealthcareWorkerService.deleteWorker(toDelete.id); // Call delete service
      fetchWorkerData(); // Refresh worker list after deletion
      setSuccess("Worker deleted successfully.");
      setToDelete(null);
    } catch (error) {
      console.error("Error deleting Worker: ", error);
      setError("Error deleting Worker. Try again later.");
      setToDelete(null);
    } finally {
      setIsDeleting(false);
    }
  };

  return (
    // Render the table component
    <div>
      <h2>Healthcare Workers</h2>
      <Button onClick={fetchWorkerData} className="btn btn-primary mb-3 me-2" disabled={loading}>
        {loading ? "Loading..." : "Refresh workers"}
      </Button>
      {!loading && error && <Alert variant="danger">{error}</Alert>}
      {success && <Alert variant="success">{success}</Alert>}
      {!loading && !error && (
        <>
          <UserTable // Reusable user table component
            users={workers}
            isHealthcareWorker={true}
            isAdmin={hasRole("Admin")}
            onDeleteClick={setToDelete}
          />
          {toDelete && (
            <UserDeleteModal // Reusable user delete confirmation modal
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

export default HealthcareWorkerPage;
