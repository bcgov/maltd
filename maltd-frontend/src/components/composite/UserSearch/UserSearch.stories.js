import React from "react";
import { action } from "@storybook/addon-actions";
import { storiesOf } from "@storybook/react";
import "bootstrap/dist/css/bootstrap.css";
import UserSearch from "./UserSearch";

const userSearch = {
  state: { isLoading: false, userExists: null }
};

const isLoadingUserSearch = {
  state: { isLoading: true, userExists: null }
};

const noUserExistsUserSearch = {
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
      onChange={action("input field value changed")}
    />
  ))
  .add("valid", () => (
    <UserSearch
      userSearch={userSearch}
      inputField={{ ...inputField, valid: true, value: "IDIR/validuser" }}
      generalButton={{ ...generalButton, disabled: false }}
      onChange={action("input field value changed")}
      onClick={action("button clicked")}
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
  .add("user does not exist", () => (
    <UserSearch
      userSearch={noUserExistsUserSearch}
      inputField={inputField}
      generalButton={generalButton}
      onChange={action("input field value changed")}
    />
  ))
  .add("invalid", () => (
    <UserSearch
      userSearch={userSearch}
      inputField={{ ...inputField, invalid: true, value: "a" }}
      generalButton={{ ...generalButton, color: "danger" }}
      onChange={action("input field value changed")}
    />
  ));
