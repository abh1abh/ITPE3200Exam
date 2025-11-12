import React from "react";
import * as ClientService from "./clientService";;
import { useAuth } from "../auth/AuthContext";
import UserDetailsCard from "../shared/UserDetailsCard";
import { Client } from "../types/client";
import Loading from "../shared/Loading";
import UserDeleteModal from "../shared/UserDeleteModal";
import { useState } from "react";
import { useParams } from "react-router-dom";

const ClientDetailsPage: React.FC = () => {
    const { id } = useParams<{ id: string }>();
    const { hasRole } = useAuth();
    const [profileData, setProfileData] = React.useState<Client | null>(null);
    const [loading, setLoading] = React.useState<boolean>(true);
    const [error, setError] = React.useState<string | null>(null);
    const [isDeleting, setIsDeleting] = useState(false);
    const [toDelete, setToDelete] = useState<Client | null>(null);
    

    const fetchProfileData = async () => {
        setLoading(true);
        setError(null);
        try {
            const clientDto = await ClientService.fetchClient(Number(id));
            const worker: Client = {
                id: clientDto.id,
                name: clientDto.name,
                email: clientDto.email,
                phone: clientDto.phone,
                address: clientDto.address
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

    const confirmDeleteClient = async (user: Client) => 
    {
        if (!toDelete?.id) return;
        setError(null);
        setIsDeleting(true);
        setToDelete(user as Client);
        try {
            await ClientService.deleteClient(toDelete.id);
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
                    {toDelete && (
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

export default ClientDetailsPage;