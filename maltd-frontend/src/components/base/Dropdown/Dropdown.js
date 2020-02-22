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
        onDropdownClick(item);
      }}
      key={item.id}
    >
      {item.name}-{item.type}
    </DropdownItem>
  ));

  return (
    <UncontrolledDropdown>
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
    items: PropTypes.array.isRequired
  }).isRequired,
  onDropdownClick: PropTypes.func.isRequired
};
