import React, { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import * as healthcareWorkerService from "./healthcareWorkerService";
import { HealthcareWorker } from "../types/healthcareWorker";
import UpdateHealthcareWorkerForm from "./UpdateHealthcareWorkerForm";
import { useAuth } from "../auth/AuthContext";
import Loading from "../shared/Loading";
import { Alert } from "react-bootstrap";
import { UpdateWorkerDto } from "../types/healthcareWorker";

const HealthcareWorkerUpdatePage: React.FC = () => {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const [worker, setWorker] = useState<HealthcareWorker | null>(null);
    const { hasRole } = useAuth();
    
    const isAdmin = hasRole("Admin");
    
    const [loading, setLoading] = useState<boolean>(true);
    const [fetchError, setFetchError] = useState<string | null>(null);
    const [submitError, setSubmitError] = useState<string | null>(null);
    
    useEffect(() => {
        const fetchWorker = async () => {
        try {
            const data = await healthcareWorkerService.fetchWorker(Number(id));
            setWorker(data);
        } catch (error) {
            console.error("Error fetching worker:", error);
            setFetchError("Failed to fetch worker data.");
        } finally {
            setLoading(false);
        }
        };
        if (id) fetchWorker();
    }, [id]);

    const handleWorkerUpdated = async (updated: UpdateWorkerDto) => {
        try {
            await healthcareWorkerService.updateWorker(Number(id), updated);
            console.log("Updated successfully");
            navigate("/healthcare-workers");
        } catch (error) {
            console.error("error update worker:", error);
            setSubmitError("Failed to update worker.");
        }
    };
    return (
        <div>
            <h2>Update Healthcare Worker</h2>
            {loading ? (
                <Loading />
            ) : !worker ? (
                <Alert variant="warning" className="mt-3">
                    No healthcare worker found.
                </Alert>
            ) : fetchError ? (
                <Alert variant="danger" className="mt-3">
                    {fetchError}
                </Alert>
            ) : (
                <UpdateHealthcareWorkerForm
                    worker={worker}
                    onHealthcareWorkerUpdated={handleWorkerUpdated}
                />
            )}
        </div>
    );
}
export default HealthcareWorkerUpdatePage;