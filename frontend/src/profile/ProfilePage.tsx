import React from "react";
import * as HealthcareWorkerService from "../healtcareWorker/healthcareWorkerService";
import * as ClientService from "../client/clientService";
import { useAuth } from "../auth/AuthContext";
import UserDetailsCard from "../shared/UserDetailsCard";
import { Client } from "../types/client";
import { HealthcareWorker } from "../types/healthcareWorker";
import Loading from "../shared/Loading";
import UserDeleteModal from "../shared/UserDeleteModal";
import { useState } from "react";

const ProfilePage: React.FC = () => {
    const { user, hasRole, logout } = useAuth();
    const [profileData, setProfileData] = React.useState<Client | HealthcareWorker | null>(null);
    const [loading, setLoading] = React.useState<boolean>(true);
    const [error, setError] = React.useState<string | null>(null);
    const role = user?.role;
    const [isDeleting, setIsDeleting] = useState(false);
    const [toDelete, setToDelete] = useState<HealthcareWorker | null>(null);
    

    const fetchProfileData = async () => {
        setLoading(true);
        setError(null);
        try {
            if (user?.role === "HealthcareWorker") {
                const workerId = user.nameid;
                const worker = await HealthcareWorkerService.fetchWorkerByAuthId(workerId);
                setProfileData(worker);
            } else if (user?.role === "Client") {
                const clientId = user.nameid;
                const client = await ClientService.fetchClientByAuthId(clientId);
                setProfileData(client);
            } else {
                setError("Unknown user role");
            }
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
        if(hasRole("HealthcareWorker")){
            logout();}
        setToDelete(null);
        } catch (error) {
        console.error("Error deleting Worker: ", error);
        setError("Error deleting Worker. Try again later.");
        setToDelete(null);
        } finally {
        setIsDeleting(false);
        }
        };
    
    const confirmDeleteClient = async (user: Client) => {
            if (!toDelete?.id) return;
            setError(null);
            setIsDeleting(true);
            setToDelete(user as Client);
            try {
            await ClientService.deleteClient(toDelete.id);
            if(hasRole("Client")){
                logout();}
            setToDelete(null);
            } catch (error) {
            console.error("Error deleting Client: ", error);
            setError("Error deleting Client. Try again later.");
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
                    <UserDetailsCard user={profileData} onDeleteClick={setToDelete} />
                    {toDelete && role === "HealthcareWorker" && (
                        <UserDeleteModal
                            user={toDelete}
                            onCancel={() => setToDelete(null)}
                            onConfirm={() => confirmDeleteWorker(toDelete as HealthcareWorker)}
                            isDeleting={isDeleting}
                        />
                    )}
                    {toDelete && role === "Client" && (
                        <UserDeleteModal
                            user={toDelete}
                            onCancel={() => setToDelete(null)}
                            onConfirm={() => confirmDeleteClient(toDelete as Client)}
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

export default ProfilePage;