import React, { useEffect, useState } from "react"; 
import { AvailableSlot } from "../types/availableSlot";
import { useAuth } from "../auth/AuthContext";
import * as HealthcareWorkerService from "../healtcareWorker/healthcareWorkerService";
import { Alert, Badge, Button, Container, Spinner, Table } from "react-bootstrap";
import Loading from "../shared/Loading";
import { HealthcareWorker } from "../types/healthcareWorker";
import UserTable from "../shared/UserTable";
import UserDeleteModal from "../shared/UserDeleteModal";

const HealthcareWorkerPage: React.FC = () => {
    const {hasRole} = useAuth();
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);
    const [workers, setClients] = useState<HealthcareWorker[]>([]);
    const [toDelete, setToDelete] = useState<HealthcareWorker | null>(null);
    const [isDeleting, setIsDeleting] = useState(false);
    const [success, setSuccess] = useState<string | null>(null);

    const fetchWorkerData = async () => {
        setLoading(true);
        setError(null);
        try {
            const response = await HealthcareWorkerService.fetchAllWorkers();
            setClients(response);
        } catch (error: any) {
            console.error("Error fetching profile data:", error);
            setError("Failed to fetch profile data");
        } finally {
            setLoading(false);
            console.log("Finished loading");
        }
    }
    useEffect(() => {
        fetchWorkerData();
    }, []);
    
    const confirmDelete = async () => {
        if (!toDelete?.id) return;
        setError(null);
        setIsDeleting(true);
        try {
        await HealthcareWorkerService.deleteWorker(toDelete.id);
        fetchWorkerData();
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
        <div>
            <Button onClick={fetchWorkerData} className="btn btn-primary mb-3 me-2" disabled={loading}>
                    {loading ? "Loading..." : "Refresh workers"}
                  </Button>
            <h2>Healthcare Workers</h2>
            {!loading && error && <Alert variant="danger">{error}</Alert>}
            {success && <Alert variant="success">{success}</Alert>}
            {!loading && !error && (
                <>
                <UserTable
                    user={workers}
                    isAdmin={hasRole("Admin")}
                    onDeleteClick={(setToDelete)} />
                {toDelete && (
                    <UserDeleteModal 
                        user = {toDelete}
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