import { defineConfig, loadEnv } from "vite";
import react from "@vitejs/plugin-react";

// https://vite.dev/config/
export default defineConfig(({ command, mode }) => {
  const env = loadEnv(mode, process.cwd(), "");

  return {
    plugins: [react()],
    server: {
      // Use the VITE_PORT variable, converting it to a number.
      // Fallback to 5173 if the variable is not set.
      port: Number(env.VITE_PORT) || 5173,
    },
  };
});
