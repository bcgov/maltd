/* eslint-disable react/jsx-filename-extension */
import React from "react";
import { storiesOf } from "@storybook/react";
import "bootstrap/dist/css/bootstrap.css";
import UserSearch from "./UserSearch";

const userSearch = {
  state: { isLoading: false }
};

const isLoadingUserSearch = {
  state: { isLoading: true }
};

const inputField = {
  name: "idir",
  type: "text",
  placeholder: "Enter IDIR username to find",
  valid: false,
  invalid: false,
  value: "",
  disabled: false
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

storiesOf("UserSearch", module)
  .add("default", () => (
    <UserSearch
      inputField={inputField}
      generalButton={generalButton}
      userSearch={userSearch}
    />
  ))
  .add("valid", () => (
    <UserSearch
      userSearch={userSearch}
      inputField={{ ...inputField, valid: true, value: "IDIR/validuser" }}
      generalButton={{ ...generalButton, disabled: false }}
    />
  ))
  .add("loading", () => (
    <UserSearch
      userSearch={isLoadingUserSearch}
      inputField={{
        ...inputField,
        valid: true,
        value: "IDIR/validuser",
        disabled: true
      }}
      generalButton={generalButton}
    />
  ))
  .add("invalid", () => (
    <UserSearch
      userSearch={userSearch}
      inputField={{ ...inputField, invalid: true, value: "a" }}
      generalButton={generalButton}
    />
  ));
