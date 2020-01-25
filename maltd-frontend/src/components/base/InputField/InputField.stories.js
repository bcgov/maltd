/* eslint-disable react/jsx-filename-extension */
import React from "react";
import { storiesOf } from "@storybook/react";
import "bootstrap/dist/css/bootstrap.css";
import InputField from "./InputField";

const inputField = {
  name: "idir",
  type: "text",
  placeholder: "Enter IDIR username to find",
  valid: false,
  invalid: false,
  val: ""
};

storiesOf("InputField", module)
  .add("default", () => <InputField inputField={inputField} />)
  .add("valid", () => (
    <InputField
      inputField={{ ...inputField, valid: true, val: "IDIR/testuser" }}
    />
  ))
  .add("invalid", () => (
    <InputField inputField={{ ...inputField, invalid: true, val: "a" }} />
  ));
