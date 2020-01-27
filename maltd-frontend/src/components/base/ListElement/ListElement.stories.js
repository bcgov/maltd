/* eslint-disable react/jsx-filename-extension */
import React from "react";
import { storiesOf } from "@storybook/react";
import ListElement from "./ListElement";

const listElement = {
  title: "MALTD",
  description: "Dynamics Account Management Tool"
};

storiesOf("List Element", module)
  .add("current projects", () => <ListElement listElement={listElement} />)
  .add("users", () => (
    <ListElement
      listElement={{
        ...listElement,
        title: "Pat Simonson",
        description: "psimon@gov.bc.ca"
      }}
    />
  ));
