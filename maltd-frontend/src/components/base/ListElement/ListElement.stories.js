import React from "react";
import { storiesOf } from "@storybook/react";
import ListElement from "./ListElement";

const listElement = {
  title: "MALTD"
};

storiesOf("List Element", module)
  .add("no resources", () => <ListElement listElement={listElement} />)
  .add("user", () => (
    <ListElement
      listElement={{
        ...listElement,
        title: "User name",
        description: "user@gov.bc.ca"
      }}
    />
  ))
  .add("with resources", () => (
    <ListElement listElement={{ ...listElement, resources: ["Dyn, Share"] }} />
  ))
  .add("empty", () => (
    <ListElement listElement={{ ...listElement, title: undefined }} />
  ));
