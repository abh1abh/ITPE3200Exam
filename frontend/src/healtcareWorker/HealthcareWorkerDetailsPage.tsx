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

    const fetchProfileData = async () => { // Fetch profile data
        setLoading(true);
        setError(null);
        try {
            const workerDto = await HealthcareWorkerService.fetchWorker(Number(id)); // Fetch worker data using the service
            const worker: HealthcareWorker = { //map WorkerDto to Worker
                id: workerDto.id,
                name: workerDto.name,
                email: workerDto.email,
                phone: workerDto.phone,
                address: workerDto.address
            };
            setProfileData(worker); // Set the fetched data to state
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

    const confirmDeleteWorker = async (user: HealthcareWorker) =>  // Confirm deletion of worker
    {
        if (!toDelete?.id) return; // No worker to delete
        setError(null);
        setIsDeleting(true);
        setToDelete(user as HealthcareWorker); // Set the worker to be deleted
        try {
            await HealthcareWorkerService.deleteWorker(toDelete.id); // Call delete service
            setToDelete(null);
            if (isHealthcareWorker){ // If the deleted user is viewing their own profile, redirect to home
                navigate('/');
            }
            else{
                navigate('/healthcareworkers'); // Redirect to workers list after deletion
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