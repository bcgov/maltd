import React from "react";
import { action } from "@storybook/addon-actions";
import { storiesOf } from "@storybook/react";
import "bootstrap/dist/css/bootstrap.css";
import NavBar from "./NavBar";

const navBar = {
  isAuthed: false
};

storiesOf("NavBar", module)
  .add("unauthenticated", () => (
    <NavBar navBar={navBar} onClick={action("logout button clicked")} />
  ))
  .add("authenticated", () => (
    <NavBar
      navBar={{ ...navBar, isAuthed: true }}
      onClick={action("logout button clicked")}
    />
  ));
