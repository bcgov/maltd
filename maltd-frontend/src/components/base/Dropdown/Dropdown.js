import React, { useState } from "react";
import PropTypes from "prop-types";
import {
  UncontrolledDropdown,
  DropdownToggle,
  DropdownMenu,
  DropdownItem
} from "reactstrap";

export default function Dropdown({ dropdown: { items }, onDropdownClick }) {
  const [selectedTitle, setSelectedTitle] = useState("Select a project");

  const dropdownItems = items.map(item => (
    <DropdownItem
      onClick={() => {
        setSelectedTitle(`${item.name} - ${item.type}`);
        onDropdownClick(item.id);
      }}
    >
      {item.name} - {item.type}
    </DropdownItem>
  ));

  return (
    <UncontrolledDropdown style={{ paddingTop: "20px" }}>
      <DropdownToggle
        style={{ backgroundColor: "white", color: "black" }}
        caret
      >
        {selectedTitle}
      </DropdownToggle>
      <DropdownMenu>{dropdownItems}</DropdownMenu>
    </UncontrolledDropdown>
  );
}

Dropdown.propTypes = {
  dropdown: PropTypes.shape({
    defaultTitle: PropTypes.string.isRequired,
    items: PropTypes.array.isRequired
  }).isRequired
};
