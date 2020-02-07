import React from "react";
import { Collapse, Navbar, NavbarBrand, Nav, NavbarText } from "reactstrap";
import PropTypes from "prop-types";
import "./NavBar.css";
import GeneralButton from "../GeneralButton/GeneralButton";

const generalButton = {
  type: "submit",
  color: "danger",
  disabled: false,
  label: "Logout"
};

export default function NavBar({ navBar: { isAuthed }, onClick }) {
  return (
    <div className="nav-div">
      <Navbar expand="md">
        <NavbarBrand>MALT</NavbarBrand>
        <Collapse isOpen navbar>
          <Nav className="mr-auto" navbar>
            <NavbarText>Account and License Management Tool</NavbarText>
          </Nav>
          {isAuthed && (
            <GeneralButton generalButton={generalButton} onClick={onClick} />
          )}
        </Collapse>
      </Navbar>
    </div>
  );
}

NavBar.propTypes = {
  navBar: PropTypes.shape({
    isAuthed: PropTypes.bool.isRequired
  }).isRequired,
  onClick: PropTypes.func.isRequired
};
