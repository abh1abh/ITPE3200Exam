import React, { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import * as healthcareWorkerService from "./healthcareWorkerService";
import { HealthcareWorker } from "../types/healthcareWorker";
import UserUpdateForm from "../shared/UserUpdateForm";
import { useAuth } from "../auth/AuthContext";
import Loading from "../shared/Loading";
import { Alert } from "react-bootstrap";
import { UpdateUserDto, User } from "../types/user";

const HealthcareWorkerUpdatePage: React.FC = () => {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const [worker, setWorker] = useState<HealthcareWorker | null>(null);
    const { hasRole, user } = useAuth();
    
    const isAdmin = hasRole("Admin");
    
    const [isSelf, setIsSelf] = useState<boolean>(false);
    const [loading, setLoading] = useState<boolean>(true);
    const [fetchError, setFetchError] = useState<string | null>(null);
    const [submitError, setSubmitError] = useState<string | null>(null);
    
    useEffect(() => {
        const fetchWorker = async () => {
        try {
            const data = await healthcareWorkerService.fetchWorker(Number(id));
                setWorker(data);

            // If not admin, check if this is user's own worker profile
            if (!isAdmin && hasRole("HealthcareWorker")) {
                if (user?.nameid) {
                    // Match logged-in AuthUserId to HealthcareWorker
                    const me = await healthcareWorkerService.fetchWorkerByAuthId(user.nameid);
                    if (me?.id === data.id) {
                        setIsSelf(true);
                    } else {
                        setIsSelf(false);
                    }
                }
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
            console.log("Updated successfully");
            if(isAdmin){
                navigate("/healthcareworkers");
            }
            else{
                navigate("/profile");
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
                <UserUpdateForm
                    user={worker}
                    role="HealthcareWorker"
                    onUserChanged={handleWorkerUpdated}
                    serverError={submitError}
                />
            )}
        </div>
    );
}
export default HealthcareWorkerUpdatePage;