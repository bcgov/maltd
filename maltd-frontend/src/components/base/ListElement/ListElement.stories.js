import React from "react";
import { storiesOf } from "@storybook/react";
import ListElement from "./ListElement";

export const listElement = {
  title: "Test Element"
};

storiesOf("List Element", module).add("default", () => (
  <ListElement listElement={listElement} />
));
