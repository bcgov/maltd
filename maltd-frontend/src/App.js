/* eslint-disable react/jsx-filename-extension */
import React from "react";
import UserSearch from "./components/composite/UserSearch/UserSearch";
import "bootstrap/dist/css/bootstrap.css";

export default function App() {
  const userSearch = { state: { isLoading: true } };
  return (
    <div>
      <h1>MALTD Frontend</h1>
      <UserSearch userSearch={userSearch} />
    </div>
  );
}
