/* eslint-disable react/jsx-filename-extension */
import React from "react";
import { storiesOf } from "@storybook/react";
import "bootstrap/dist/css/bootstrap.css";
import UserSearch from "./UserSearch";

const userSearch = {
  state: { isLoading: false }
};

const inputField = {
  name: "idir",
  type: "text",
  placeholder: "IDIR/username",
  valid: false,
  invalid: false,
  value: ""
};
