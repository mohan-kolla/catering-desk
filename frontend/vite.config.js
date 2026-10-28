import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

// Production builds are served from mohankolla.com/catering-desk/ (GitHub Pages).
export default defineConfig(({ command }) => ({
  plugins: [react()],
  base: command === "build" ? "/catering-desk/" : "/",
}));
