/* eslint-disable react/jsx-filename-extension */
import React from "react";
import { action } from "@storybook/addon-actions";
import { storiesOf } from "@storybook/react";
import "bootstrap/dist/css/bootstrap.css";
import BackIcon from "./BackIcon";

const backIcon = {
  message: "Find another user"
};

storiesOf("BackIcon", module).add("default", () => (
  <BackIcon backIcon={backIcon} onClick={action("back button clicked")} />
));
