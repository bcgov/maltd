/* eslint-disable react/jsx-filename-extension */
import React from "react";
import { Button } from "reactstrap";
import PropTypes from "prop-types";
import "./GeneralButton.css";

export default function GeneralButton({
  generalButton: { type, color, disabled, label }
}) {
  return (
    <Button
      className="general-button"
      type={type}
      color={color}
      disabled={disabled}
    >
      {label}
    </Button>
  );
}

GeneralButton.propTypes = {
  generalButton: PropTypes.shape({
    type: PropTypes.string.isRequired,
    color: PropTypes.string.isRequired,
    disabled: PropTypes.bool.isRequired,
    label: PropTypes.string.isRequired
  }).isRequired
};
