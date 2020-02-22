import React from "react";
import { action } from "@storybook/addon-actions";
import { storiesOf } from "@storybook/react";
import "bootstrap/dist/css/bootstrap.css";
import BackIcon from "./BackIcon.jsx";

const backIcon = {
  message: "Find another user"
};

storiesOf("BackIcon", module).add("default", () => (
  <BackIcon backIcon={backIcon} onClick={action("back button clicked")} />
));
