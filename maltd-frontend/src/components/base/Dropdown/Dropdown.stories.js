import React from "react";
import { storiesOf } from "@storybook/react";
import "bootstrap/dist/css/bootstrap.css";
import Dropdown from "./Dropdown";

const dropdown = {
  defaultTitle: "Choose a project",
  items: ["System1", "System2"]
};

storiesOf("Dropdown", module).add("default", () => (
  <Dropdown dropdown={dropdown} />
));
