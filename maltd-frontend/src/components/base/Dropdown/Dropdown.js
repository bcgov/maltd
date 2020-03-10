import React, { useState } from "react";
import PropTypes from "prop-types";
import {
  UncontrolledDropdown,
  DropdownToggle,
  DropdownMenu,
  DropdownItem
} from "reactstrap";

import { FaCaretDown } from "react-icons/fa";

import "./Dropdown.css";

export default function Dropdown({ dropdown: { items }, onDropdownClick }) {
  const [selectedTitle, setSelectedTitle] = useState("Select a project");

  const dropdownItems = items.map(item => (
    <DropdownItem
      onClick={() => {
        setSelectedTitle(item.name);
        onDropdownClick(item);
      }}
      key={item.id}
    >
      {item.name}
    </DropdownItem>
  ));

  return (
    <UncontrolledDropdown direction="down">
      <DropdownToggle id="dropdown">
        <div id="title">{selectedTitle}</div>
        <div className="img">
          <FaCaretDown />
        </div>
      </DropdownToggle>
      <DropdownMenu data-cy="drop-down-menu">{dropdownItems}</DropdownMenu>
    </UncontrolledDropdown>
  );
}

Dropdown.propTypes = {
  dropdown: PropTypes.shape({
    items: PropTypes.array.isRequired
  }).isRequired,
  onDropdownClick: PropTypes.func.isRequired
};
