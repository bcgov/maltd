/* eslint-disable react/jsx-filename-extension */
import React from "react";
import { storiesOf } from "@storybook/react";
import "bootstrap/dist/css/bootstrap.css";
import NavBar from "./NavBar";

const navBar = {
  isAuthed: false
};

storiesOf("NavBar", module)
  .add("unauthenticated", () => <NavBar navBar={navBar} />)
  .add("authenticated", () => (
    <NavBar navBar={{ ...navBar, isAuthed: true }} />
  ));
