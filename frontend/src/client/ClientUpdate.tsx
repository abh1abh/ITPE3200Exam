import React, { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import * as clientService from "./clientService";
import { Client } from "../types/client";
import UserUpdateForm from "../shared/UserUpdateForm";
import { useAuth } from "../auth/AuthContext";
import Loading from "../shared/Loading";
import { Alert } from "react-bootstrap";
import { UpdateUserDto } from "../types/user";

const ClientUpdatePage: React.FC = () => {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const [client, setClient] = useState<Client | null>(null);
    const { hasRole } = useAuth();
    
    const isAdmin = hasRole("Admin");
    const isClient = hasRole("Client");
    
    const [loading, setLoading] = useState<boolean>(true);
    const [fetchError, setFetchError] = useState<string | null>(null);
    const [submitError, setSubmitError] = useState<string | null>(null);
    const [success, setSuccess] = useState<string | null>(null);
    
    useEffect(() => {
        const fetchClient = async () => {
        try {
            const data = await clientService.fetchClient(Number(id));
            setClient(data);
        } catch (error) {
            console.error("Error fetching worker:", error);
            setFetchError("Failed to fetch worker data.");
        } finally {
            setLoading(false);
        }
        };
        if (id) fetchClient();
    }, [id]);

    const handleClientUpdated = async (updated: UpdateUserDto) => {
        try {
            await clientService.updateClient(Number(id), updated);
            setSuccess("Update successful!");
            if(isAdmin){
                setTimeout(() => navigate("/clients"), 2000);
            }
            else if(isClient){
                setTimeout(() => navigate("/profile"), 2000);            
            }
        } catch (error) {
            console.error("error update client:", error);
            setSubmitError("Failed to update client.");
        }
    };
    return (
        <div>
            <h2>Update Client</h2>
            {success && <Alert variant="success">{success}</Alert>}
            {loading ? (
                <Loading />
            ) : !client ? (
                <Alert variant="warning" className="mt-3">
                    No client found.
                </Alert>
            ) : fetchError ? (
                <Alert variant="danger" className="mt-3">
                    {fetchError}
                </Alert>
            ) : (
                <UserUpdateForm
                    profileUser={client}
                    onUserChanged={handleClientUpdated}
                    role="Client"
                    serverError={submitError}
                />
            )}
        </div>
    );
}
export default ClientUpdatePage;