import React, { useState } from "react";
import { Appointment } from "../types/appointment";
import { Client } from "../types/client";
import { useNavigate } from "react-router-dom";

interface UpdateAppointmentFormProps {
  initialData: Appointment;
  onAppointmentChanged: (updated: Appointment) => void;
  // Optional helpers to show names in the read-only section:
  clientsById?: Record<number, Client>; // or provide clientName directly
  workerName?: string; // if you don't have a lookup, pass a string
  clientName?: string; // same as above
  serverError?: string | null;
}

const UpdateAppointmentForm: React.FC<UpdateAppointmentFormProps> = ({
  initialData,
  onAppointmentChanged,
  clientsById,
  workerName,
  clientName,
  serverError = null,
}) => {
  const navigate = useNavigate();
  const onCancel = () => navigate(-1);

  const [notes, setNotes] = useState<string>(initialData.notes ?? "");
  const [newTaskDesc, setNewTaskDesc] = useState<string>("");
  const [newTaskCompleted, setNewTaskCompleted] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  return <div>UpdateAppointmentForm</div>;
};

export default UpdateAppointmentForm;
