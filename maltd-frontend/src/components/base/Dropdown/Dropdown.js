import React, { useState } from "react";
import PropTypes from "prop-types";
import {
  UncontrolledDropdown,
  DropdownToggle,
  DropdownMenu,
  DropdownItem
} from "reactstrap";

import Icon from "../../../img/caret-down-solid.svg";

import "./Dropdown.css";

export default function Dropdown({ dropdown: { items }, onDropdownClick }) {
  const [selectedTitle, setSelectedTitle] = useState("Select a project");

  const dropdownItems = items.map(item => (
    <DropdownItem
      onClick={() => {
        setSelectedTitle(`${item.name} - ${item.type}`);
        onDropdownClick(item);
      }}
      key={item.id}
    >
      {item.name}-{item.type}
    </DropdownItem>
  ));

  return (
    <UncontrolledDropdown direction="down">
      <DropdownToggle id="dropdown">
        <div id="title">{selectedTitle}</div>
        <div className="img">
          <img
            className="caret"
            alt="down"
            src={Icon}
            height="15px"
            width="15px"
          />
        </div>
      </DropdownToggle>
      <DropdownMenu>{dropdownItems}</DropdownMenu>
    </UncontrolledDropdown>
  );
}

Dropdown.propTypes = {
  dropdown: PropTypes.shape({
    items: PropTypes.array.isRequired
  }).isRequired,
  onDropdownClick: PropTypes.func.isRequired
};
