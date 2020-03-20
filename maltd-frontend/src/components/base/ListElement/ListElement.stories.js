import React from "react";
import { storiesOf } from "@storybook/react";
import ListElement from "./ListElement";

const listElement = {
  title: "MALTD"
};

storiesOf("List Element", module)
  .add("not properly added resources", () => (
    <ListElement
      listElement={{
        ...listElement,
        resources: [
          { type: "Dyn", message: "error exists", status: "not-member" },
          { type: "Share", message: null, status: "member" }
        ]
      }}
    />
  ))
  .add("user", () => (
    <ListElement
      listElement={{
        ...listElement,
        title: "User name",
        description: "user@gov.bc.ca"
      }}
    />
  ))
  .add("properly added resources", () => (
    <ListElement
      listElement={{
        ...listElement,
        resources: [
          { type: "Dyn", status: "member" },
          { type: "Share", status: "member" }
        ]
      }}
    />
  ))
  .add("empty", () => (
    <ListElement listElement={{ ...listElement, title: undefined }} />
  ));
