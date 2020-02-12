import React from "react";
import { action } from "@storybook/addon-actions";
import { storiesOf } from "@storybook/react";
import "bootstrap/dist/css/bootstrap.css";
import NavBar from "./NavBar";

storiesOf("NavBar", module).add("default", () => (
  <NavBar onClick={action("logout button clicked")} />
));
