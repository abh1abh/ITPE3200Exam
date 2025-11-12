import React from "react";
import * as HealthcareWorkerService from "./healthcareWorkerService";;
import { useAuth } from "../auth/AuthContext";
import UserDetailsCard from "../shared/UserDetailsCard";
import { HealthcareWorker } from "../types/healthcareWorker";
import Loading from "../shared/Loading";
import UserDeleteModal from "../shared/UserDeleteModal";
import { useState } from "react";
import { Navigate, useNavigate, useParams } from "react-router-dom";

const HealthcareWorkerDetailsPage: React.FC = () => {
    const { id } = useParams<{ id: string }>();
    const { user, hasRole } = useAuth();
    const [profileData, setProfileData] = React.useState<HealthcareWorker | null>(null);
    const [loading, setLoading] = React.useState<boolean>(true);
    const [error, setError] = React.useState<string | null>(null);
    const [isDeleting, setIsDeleting] = useState(false);
    const [toDelete, setToDelete] = useState<HealthcareWorker | null>(null);
    const navigate = useNavigate();

    const isHealthcareWorker = hasRole("HealthcareWorker");

    const fetchProfileData = async () => {
        setLoading(true);
        setError(null);
        try {
            const workerDto = await HealthcareWorkerService.fetchWorker(Number(id));
            const worker: HealthcareWorker = {
                id: workerDto.id,
                name: workerDto.name,
                email: workerDto.email,
                phone: workerDto.phone,
                address: workerDto.address
            };
            setProfileData(worker);
        } catch (error: any) {
            console.error("Error fetching profile data:", error);
            setError("Failed to fetch profile data");
        } finally {
            setLoading(false);
        }
    }
    React.useEffect(() => {
        fetchProfileData();
    }, []);

    const confirmDeleteWorker = async (user: HealthcareWorker) => 
    {
        if (!toDelete?.id) return;
        setError(null);
        setIsDeleting(true);
        setToDelete(user as HealthcareWorker);
        try {
            await HealthcareWorkerService.deleteWorker(toDelete.id);
            setToDelete(null);
            if (isHealthcareWorker){
                navigate('/');
            }
            else{
                navigate('/healthcareworkers');
            }
        } catch (error) {
            console.error("Error deleting Worker: ", error);
            setError("Error deleting Worker. Try again later.");
            setToDelete(null);
        } finally {
            setIsDeleting(false);
        }
        };
    
    return(
        <div>
            <h2>Profile</h2>
            {loading ? (
                <Loading />
            ) : error ? (
                <p style={{ color: "red" }}>{error}</p>
            ) : profileData ? (
                <>
                    <UserDetailsCard user={profileData} onDeleteClick={setToDelete} isHealthcareWorker />
                    {toDelete && (
                        <UserDeleteModal
                            user={toDelete}
                            onCancel={() => setToDelete(null)}
                            onConfirm={() => confirmDeleteWorker(toDelete as HealthcareWorker)}
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

export default HealthcareWorkerDetailsPage;