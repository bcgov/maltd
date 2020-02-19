import React from "react";
import { storiesOf } from "@storybook/react";
import { action } from "@storybook/addon-actions";
import "bootstrap/dist/css/bootstrap.css";
import UserAccess from "./UserAccess";
import "./UserAccess.css";

const userAccess = {
  projects: [
    { name: "Project1", id: "1" },
    { name: "Project2", id: "2" }
  ],
  userName: "User Name",
  userEmail: "user@gov.bc.ca"
};
const onXClick = action("x clicked");
const onPlusClick = action("plus clicked");
const onDropdownClick = action("dropdown clicked");
const dropdown = {
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

storiesOf("UserAccess", module)
  .add("default", () => (
    <UserAccess
      userAccess={userAccess}
      onDropdownClick={onDropdownClick}
      onPlusClick={onPlusClick}
      onXClick={onXClick}
      dropdown={dropdown}
    />
  ))
  .add("no projects", () => (
    <UserAccess
      userAccess={{ ...userAccess, projects: [] }}
      onDropdownClick={onDropdownClick}
      onPlusClick={onPlusClick}
      onXClick={onXClick}
      dropdown={dropdown}
    />
  ));
