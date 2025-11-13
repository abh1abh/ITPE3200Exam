import React, { useEffect, useState } from "react";
import { useAuth } from "../auth/AuthContext";
import * as ClientService from "../client/clientService";
import * as HealthcareWorkerService from "../healtcareWorker/healthcareWorkerService";
//Shared profileform.
//Display profile information.
//Placed in the 'shared' since all the users (logged in) can use it.
const ProfileForm: React.FC = () => {
    const { user } = useAuth();
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);

    const [data, setdata] = useState({ // Form data state with default values
        email: "",
        name: "",
        number: "",
        address: ""
    });
    console.log(user);
    
    const fetchUserData = async () => { // Fetch profile data based on user role
        setLoading(true);
        setError(null);
        try {
            if(user?.role === "Admin"){
                
            }
            if (user?.role === "HealthcareWorker") { // Fetch healthcare worker data if user is a worker
                const worker = await HealthcareWorkerService.fetchWorkerBySelf(); // Fetch worker data using the service
                data.email = worker.email;                                        // Set form data
                data.name = worker.name;
                data.number = worker.phone;
                data.address = worker.address;
            }
            else if (user?.role === "Client") { // Fetch client data if user is a client
                const client = await ClientService.fetchClientBySelf(); // Fetch client data using the service
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
