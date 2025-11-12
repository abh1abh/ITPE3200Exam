import React, { useEffect, useMemo, useState } from "react";
import * as HealthcareWorkerService from "../healtcareWorker/healthcareWorkerService";
import { HealthcareWorker } from "../types/healthcareWorker";
import { Client } from "../types/client";
import { useNavigate } from "react-router-dom";
import * as ClientService from "../client/clientService";
import { useAuth } from "../auth/AuthContext";

const ProfileForm: React.FC = () => {
    const { user } = useAuth();
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);

    const [data, setdata] = useState({
        email: "",
        name: "",
        number: "",
        address: ""
    });
    console.log(user);
    
    const fetchUserData = async () => {
        setLoading(true);
        setError(null);
        try {
            if(user?.role === "Admin"){
                
            }
            if (user?.role === "HealthcareWorker") {
                const worker = await HealthcareWorkerService.fetchWorkerBySelf();
                data.email = worker.email;
                data.name = worker.name;
                data.number = worker.phone;
                data.address = worker.address;
            }
            else if (user?.role === "Client") {
                const client = await ClientService.fetchClientBySelf();
                data.email = client.email;
                data.name = client.name;
                data.number = client.phone;
                data.address = client.address;
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
    useEffect(() => {
        fetchUserData();
    }, []);

    return (
            <div>
                <h2>Profile</h2>
                {loading && <p>Loading...</p>}
                {error && <p style={{ color: "red" }}>{error}</p>}
                <div
                style={{
                  flex: "0 0 50%",
                  maxWidth: "600px",
                }}
              >
                {!loading && !error && (
                  <div
                    style={{
                      textAlign: "left",
                    }}
                  >
                    <div style={{ marginBottom: "12px" }}>
                      <strong>Email:</strong>
                      <div>{data.email}</div>
                    </div>
                    <div style={{ marginBottom: "12px" }}>
                      <strong>Name:</strong>
                      <div>{data.name}</div>
                    </div>
                    <div style={{ marginBottom: "12px" }}>
                      <strong>Phone Number:</strong>
                      <div>{data.number}</div>
                    </div>
                    <div style={{ marginBottom: "12px" }}>
                      <strong>Address:</strong>
                      <div>{data.address}</div>
                    </div>
                  </div>
                )}
              </div>
            </div>
          );
};
export default ProfileForm;
