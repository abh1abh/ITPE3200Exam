import React, { useEffect, useState } from "react"; 
import { AvailableSlot } from "../types/availableSlot";
import { useAuth } from "../auth/AuthContext";
import * as HealthcareWorkerService from "../healtcareworker/HealthcareWorkerService";
import { Alert, Badge, Button, Container, Spinner, Table } from "react-bootstrap";
import Loading from "../shared/Loading";
import { HealthcareWorker } from "../types/healthcareWorker";
import HealthcareWorkerTable from "./HealthcareWorkerTable";

const HealthcareWorkerPage: React.FC = () => {
    const {hasRole} = useAuth();
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);
    const [workers, setClients] = useState<HealthcareWorker[]>([]);
    const [toDelete, setToDelete] = useState<HealthcareWorker | null>(null);

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
    const updateWorker = () => {

    };
    const deleteWorker = () => {

    };

    
    return (
        <div>
            <Button onClick={fetchWorkerData} className="btn btn-primary mb-3 me-2" disabled={loading}>
                    {loading ? "Loading..." : "Refresh workers"}
                  </Button>
            <h2>Healthcare Workers</h2>
            {!loading && error && <Alert variant="danger">{error}</Alert>}
            {!loading && !error && (
                <HealthcareWorkerTable
                    workers={workers}
                    isAdmin={hasRole("Admin")}
                    onDeleteClick={(setToDelete)} />
            )}
        </div>
    );

};

export default HealthcareWorkerPage;