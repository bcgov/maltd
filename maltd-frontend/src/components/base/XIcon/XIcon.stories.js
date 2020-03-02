import React from "react";
import { storiesOf } from "@storybook/react";
import { action } from "@storybook/addon-actions";
import "bootstrap/dist/css/bootstrap.css";
import XIcon from "./XIcon";

storiesOf("XIcon", module).add("default", () => (
  <XIcon id="12" onClick={action("x icon clicked")} />
));
