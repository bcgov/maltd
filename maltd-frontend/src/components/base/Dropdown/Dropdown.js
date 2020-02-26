import React, { useState } from "react";
import PropTypes from "prop-types";
import {
  UncontrolledDropdown,
  DropdownToggle,
  DropdownMenu,
  DropdownItem
} from "reactstrap";

// import "./Dropdown.css";

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
    <UncontrolledDropdown direction="left">
      <DropdownToggle
        className="dropdown"
        style={{
          backgroundColor: "white",
          color: "black",
          overflow: "hidden",
          whiteSpace: "nowrap",
          textOverflow: "ellipsis",
          paddingRight: "1px",
          marginRight: "1px",
          width: "220px"
        }}
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
