/* eslint-disable react/jsx-filename-extension */
import React from "react";
import { storiesOf } from "@storybook/react";
import "bootstrap/dist/css/bootstrap.css";
import SearchBox from "./SearchBox";

const searchBox = {
  type: "submit",
  color: "success",
  disabled: true,
  block: false,
  active: false,
  outline: false
};

storiesOf("SearchBox", module)
  .add("default", () => <SearchBox searchBox={searchBox} />)
  .add("valid", () => (
    <SearchBox searchBox={{ ...searchBox, disabled: false }} />
  ))
  .add("invalid", () => (
    <SearchBox searchBox={{ ...searchBox, color: "danger" }} />
  ));
