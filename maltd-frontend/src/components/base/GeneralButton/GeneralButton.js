/* eslint-disable react/jsx-filename-extension */
import React from "react";
import { Button } from "reactstrap";
import PropTypes from "prop-types";
import "./GeneralButton.css";

export default function GeneralButton({
  generalButton: {
    type,
    color,
    disabled,
    block,
    active,
    outline,
    label,
    styling
  }
}) {
  return (
    <Button
      className={styling}
      type={type}
      color={color}
      disabled={disabled}
      block={block}
      active={active}
      outline={outline}
    >
      {label}
    </Button>
  );
}

GeneralButton.propTypes = {
  generalButton: PropTypes.shape({
    styling: PropTypes.string.isRequired,
    type: PropTypes.string.isRequired,
    color: PropTypes.string.isRequired,
    disabled: PropTypes.bool.isRequired,
    block: PropTypes.bool.isRequired,
    active: PropTypes.bool.isRequired,
    outline: PropTypes.bool.isRequired,
    label: PropTypes.any.isRequired
  }).isRequired
};
