/* eslint-disable react/jsx-filename-extension */
import React from "react";
import UserSearch from "./components/composite/UserSearch/UserSearch";
import "bootstrap/dist/css/bootstrap.css";

export default function App() {
  const userSearch = { state: { isLoading: true } };

  const inputField = {
    type: "text",
    placeholder: "Enter IDIR username to find",
    name: "usernamesearch",
    valid: false,
    invalid: false,
    value: ""
  };

  const generalButton = {
    type: "submit",
    color: "primary",
    disabled: true,
    block: false,
    active: false,
    outline: false,
    label: "Find",
    styling: "general-button align-right"
  };
  return (
    <div>
      <h1>MALTD Frontend</h1>
      <UserSearch
        userSearch={userSearch}
        inputField={inputField}
        generalButton={generalButton}
      />
    </div>
  );
}
