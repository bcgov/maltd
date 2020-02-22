import React from "react";
import { storiesOf } from "@storybook/react";
import { action } from "@storybook/addon-actions";
import "bootstrap/dist/css/bootstrap.css";
import Dropdown from "./Dropdown";

const dropdown = {
  defaultTitle: "Choose a project",
  items: [
    {
      id: "123",
      name: "System 1",
      type: "Dynamics"
    },
    {
      id: "321",
      name: "System 2",
      type: "Sharepoint"
    }
  ]
};

storiesOf("Dropdown", module).add("default", () => (
  <Dropdown dropdown={dropdown} onDropdownClick={action("dropdown clicked")} />
));
