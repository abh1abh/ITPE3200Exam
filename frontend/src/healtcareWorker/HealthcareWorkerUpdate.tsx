import React, { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import * as healthcareWorkerService from "./healthcareWorkerService";
import { HealthcareWorker } from "../types/healthcareWorker";
import UserUpdateForm from "../shared/UserUpdateForm";
import { useAuth } from "../auth/AuthContext";
import { jwtDecode } from "jwt-decode";
import Loading from "../shared/Loading";
import { Alert } from "react-bootstrap";
import { UpdateUserDto } from "../types/user";

const HealthcareWorkerUpdatePage: React.FC = () => {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const [worker, setWorker] = useState<HealthcareWorker | null>(null);
    const { hasRole, user } = useAuth();
    
    const isAdmin = hasRole("Admin");
    const isWorker = hasRole("HealthcareWorker");
    
    const [isSelf, setIsSelf] = useState<boolean>(false);
    const [loading, setLoading] = useState<boolean>(true);
    const [fetchError, setFetchError] = useState<string | null>(null);
    const [submitError, setSubmitError] = useState<string | null>(null);
    const [success, setSuccess] = useState<string | null>(null);
    
    useEffect(() => {
        const fetchWorker = async () => {
        try {
            // If not admin, check if this is user's own worker profile
            if (!isAdmin && hasRole("HealthcareWorker")) {
                const data = await healthcareWorkerService.fetchWorkerBySelf();
                setWorker(data);
                setIsSelf(true);
            }
            else{
                const data = await healthcareWorkerService.fetchWorker(Number(id));
                setWorker(data);
            }
        } catch (error) {
            console.error("Error fetching worker:", error);
            setFetchError("Failed to fetch worker data.");
        } finally {
            setLoading(false);
        }
        };
        if (id) fetchWorker();
    }, [id, isAdmin, hasRole, user]);

    const canAccessPage = isAdmin || isSelf;

    const handleWorkerUpdated = async (updated: UpdateUserDto) => {
        try {
            await healthcareWorkerService.updateWorker(Number(id), updated);
            setSuccess("Update successful!");
            if(isAdmin){
                setTimeout(() => navigate("/healthcareworkers"), 2000);
            }
            else if(isWorker){
                setTimeout(() => navigate("/profile"), 2000);
            }
        } catch (error) {
            console.error("error update worker:", error);
            setSubmitError("Failed to update worker.");
        }
    };
    if (!canAccessPage) {
        return (
            <Alert variant="danger" className="mt-3">
                You do not have permission to access this page.
            </Alert>
        );
    }
    else{
    return (
        <div>
            <h2>Update Healthcare Worker</h2>
            {success && <Alert variant="success">{success}</Alert>}
            {loading ? (
                <Loading />
            ) : !worker ? (
                <Alert variant="warning" className="mt-3">
                    No healthcare worker found.
                </Alert>
            ) :fetchError ? (
                <Alert variant="danger" className="mt-3">
                    {fetchError}
                </Alert>
            ) : (
                <UserUpdateForm
                    profileUser={worker}
                    role="HealthcareWorker"
                    onUserChanged={handleWorkerUpdated}
                    serverError={submitError}
                />
            )}
        </div>
    );
}
}
export default HealthcareWorkerUpdatePage;