import React from "react";
import AuthenticationGuard from "./components/hoc/AuthenticationGuard";

export default function App() {
  return (
    <div>
      <AuthenticationGuard />
    </div>
  );
}
