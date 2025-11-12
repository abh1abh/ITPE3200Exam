import React, { useEffect, useState } from "react"; 
import { useAuth } from "../auth/AuthContext";
import * as ClientService from "./clientService";
import { Alert, Badge, Button, Container, Spinner, Table } from "react-bootstrap";
import Loading from "../shared/Loading";
import { Client } from "../types/client";
import UserTable from "../shared/UserTable";
import UserDeleteModal from "../shared/UserDeleteModal";

const ClientPage: React.FC = () => {
    const {hasRole} = useAuth();
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);
    const [clients, setClients] = useState<Client[]>([]);
    const [toDelete, setToDelete] = useState<Client | null>(null);
    const [isDeleting, setIsDeleting] = useState(false);
    const [success, setSuccess] = useState<string | null>(null);
    

    const fetchClientData = async () => {
        setLoading(true);
        setError(null);
        try {
            const response = await ClientService.fetchAllClients();
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
        fetchClientData();
    }, []);
    
    const confirmDelete = async () => {
        if (!toDelete?.id) return;
        setError(null);
        setIsDeleting(true);
        try {
        await ClientService.deleteClient(toDelete.id);
        fetchClientData();
        setSuccess("Client deleted successfully.");
        setToDelete(null);
        } catch (error) {
        console.error("Error deleting Client: ", error);
        setError("Error deleting Client. Try again later.");
        setToDelete(null);
        } finally {
        setIsDeleting(false);
        }
    };

    
    return (
        <div>
            <h2>Clients</h2>
            <Button onClick={fetchClientData} className="btn btn-primary mb-3 me-2" disabled={loading}>
                    {loading ? "Loading..." : "Refresh clients"}
                  </Button>
            {!loading && error && <Alert variant="danger">{error}</Alert>}
            {success && <Alert variant="success">{success}</Alert>}
            
            {!loading && !error && (
                <>
                <UserTable
                    users={clients}
                    isHealthcareWorker={false}
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

export default ClientPage;