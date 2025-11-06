import React, { useState } from "react";
import { Appointment, AppointmentView } from "../types/appointment";
import { Client } from "../types/client";
import { useNavigate } from "react-router-dom";

interface UpdateAppointmentFormProps {
  initialData: AppointmentView;
  onAppointmentChanged: (updated: Appointment) => void;
  serverError?: string | null;
}

const UpdateAppointmentForm: React.FC<UpdateAppointmentFormProps> = ({
  initialData,
  onAppointmentChanged,
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
