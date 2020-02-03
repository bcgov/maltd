/* eslint-disable react/jsx-filename-extension */
import React from "react";
import { storiesOf } from "@storybook/react";
import "bootstrap/dist/css/bootstrap.css";
import UserSearch from "./UserSearch";

const userSearch = {
  state: { isLoading: false, userExists: true }
};

const isLoadingUserSearch = {
  state: { isLoading: true, userExists: true }
};

const userNotExist = {
  state: { isLoading: false, userExists: false }
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
  label: "Find"
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
  .add("not exist", () => (
    <UserSearch
      userSearch={userNotExist}
      inputField={{
        ...inputField,
        invalid: true,
        value: "IDIR/validuser"
      }}
      generalButton={{ ...generalButton, color: "danger" }}
    />
  ))
  .add("invalid", () => (
    <UserSearch
      userSearch={userSearch}
      inputField={{ ...inputField, invalid: true, value: "a" }}
      generalButton={{ ...generalButton, color: "danger" }}
    />
  ));
