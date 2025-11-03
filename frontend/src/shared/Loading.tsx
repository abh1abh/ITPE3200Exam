import React from "react";
import { Spinner } from "react-bootstrap";

const Loading: React.FC = () => {
  return (
    <div className="text-center my-4">
      <Spinner animation="border" role="status" />
      <div className="mt-2">Loading…</div>
    </div>
  );
};

export default Loading;
