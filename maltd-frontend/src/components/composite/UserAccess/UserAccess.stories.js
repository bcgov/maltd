/* eslint-disable react/jsx-filename-extension */
import React from "react";
import { storiesOf } from "@storybook/react";
import "bootstrap/dist/css/bootstrap.css";
import UserAccess from "./UserAccess";
import "./UserAccess.css";

const userAccess = {
  projects: [{ name: "Project1" }, { name: "Project2" }],
  userName: "User Name",
  userEmail: "user@gov.bc.ca"
};

storiesOf("UserAccess", module)
  .add("default", () => <UserAccess userAccess={userAccess} />)
  .add("no projects", () => (
    <UserAccess userAccess={{ ...userAccess, projects: [] }} />
  ));
