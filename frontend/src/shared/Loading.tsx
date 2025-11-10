import React from "react";
import { Spinner } from "react-bootstrap";
//Shared loading.
//Placed in the 'shared' so that all pages can use it.
//Display a message while waiting response.
const Loading: React.FC = () => {
  return (
    <div className="text-center my-4">
      <Spinner animation="border" role="status" />
      <div className="mt-2">Loading…</div>
    </div>
  );
};

export default Loading;
