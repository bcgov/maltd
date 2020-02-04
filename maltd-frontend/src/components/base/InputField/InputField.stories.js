/* eslint-disable react/jsx-filename-extension */
import React from "react";
import { action } from "@storybook/addon-actions";
import { storiesOf } from "@storybook/react";
import "bootstrap/dist/css/bootstrap.css";
import InputField from "./InputField";

const inputField = {
  type: "text",
  name: "idir",
  placeholder: "Enter IDIR username to find",
  valid: false,
  invalid: false,
  value: "",
  disabled: false
};

storiesOf("InputField", module)
  .add("default", () => (
    <InputField
      inputField={inputField}
      onChange={action("input value changed")}
    />
  ))
  .add("valid", () => (
    <InputField
      inputField={{ ...inputField, valid: true, value: "IDIR/testuser" }}
      onChange={action("input value changed")}
    />
  ))
  .add("invalid", () => (
    <InputField
      inputField={{ ...inputField, invalid: true, value: "a" }}
      onChange={action("input value changed")}
    />
  ))
  .add("disabled", () => (
    <InputField inputField={{ ...inputField, disabled: true }} />
  ));
