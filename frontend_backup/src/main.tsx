import "bootstrap/dist/css/bootstrap.min.css";
import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import App from "./App";
import "./index.css";

const root = document.getElementById("root") as HTMLElement;

if (root) {
  createRoot(root).render(
    <StrictMode>
      <App />
    </StrictMode>
  );
}