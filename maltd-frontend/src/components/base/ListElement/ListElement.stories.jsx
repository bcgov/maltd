import React from "react";
import { storiesOf } from "@storybook/react";
import ListElement from "./ListElement.jsx";

const listElement = {
  title: "MALTD"
};

storiesOf("List Element", module)
  .add("current project", () => <ListElement listElement={listElement} />)
  .add("user", () => (
    <ListElement
      listElement={{
        ...listElement,
        title: "User name",
        description: "user@gov.bc.ca"
      }}
    />
  ))
  .add("empty", () => (
    <ListElement listElement={{ ...listElement, title: undefined }} />
  ));
