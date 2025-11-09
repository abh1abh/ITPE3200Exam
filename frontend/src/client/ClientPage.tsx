import React, { useEffect, useState } from "react";
import { AvailableSlot } from "../types/availableSlot";
import { useAuth } from "../auth/AuthContext";
import * as ClientService from "../client/ClientService";
import { Alert, Badge, Button, Container, Spinner, Table } from "react-bootstrap";
import Loading from "../shared/Loading";
import { Client } from "../types/client";

const ClientPage: React.FC = () => {
    const { user } = useAuth();
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);
    const [clients, setClients] = useState<Client[]>([]);


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

    
    return (
        <div>
            <Button onClick={fetchClientData} className="btn btn-primary mb-3 me-2" disabled={loading}>
                    {loading ? "Loading..." : "Refresh Clients"}
                  </Button>
            <h2>Clients</h2>
            {loading ? (
                <Loading />
            ) : error ? (
                <Alert variant="danger">{error}</Alert>
            ) : (
                <Table striped bordered hover>
                    <thead>
                        <tr>
                            <th>ID</th>
                            <th>Name</th>
                            <th>Email</th>
                            <th>Phone</th>
                            <th>Address</th>
                        </tr>
                    </thead>
                    <tbody>
                        {clients.map((client) => (
                            <tr key={client.clientId}>
                                <td>{client.clientId}</td>
                                <td>{client.name}</td>
                                <td>{client.email}</td>
                                <td>{client.phone}</td>
                                <td>{client.address}</td>
                            </tr>
                        ))}
                    </tbody>
                </Table>
            )}
        </div>
    );

};

export default ClientPage;