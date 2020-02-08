import React from "react";
import { storiesOf } from "@storybook/react";
import { action } from "@storybook/addon-actions";
import "bootstrap/dist/css/bootstrap.css";
import PlusIcon from "./PlusIcon";

storiesOf("PlusIcon", module).add("default", () => (
  <PlusIcon onClick={action("plus icon clicked")} />
));
