/* eslint-disable react/jsx-filename-extension */
import React from "react";
import { storiesOf } from "@storybook/react";
import "bootstrap/dist/css/bootstrap.css";
import InputField from "./InputField";

const inputField = {
  name: "idir",
  type: "text",
  placeholder: "IDIR/username",
  valid: false,
  invalid: false,
  value: ""
};

storiesOf("InputField", module)
  .add("default", () => <InputField inputField={inputField} />)
  .add("valid", () => (
    <InputField
      inputField={{ ...inputField, valid: true, value: "IDIR/testuser" }}
    />
  ))
  .add("invalid", () => (
    <InputField inputField={{ ...inputField, invalid: true, value: "a" }} />
  ));
