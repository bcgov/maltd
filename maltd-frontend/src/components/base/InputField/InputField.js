/* eslint-disable react/jsx-filename-extension */
import React from "react";
import { Input } from "reactstrap";
import PropTypes from "prop-types";
import "./InputField.css";

export default function InputField({
  inputField: { type, name, placeholder, valid, invalid, value }
}) {
  return (
    <Input
      className="input-field"
      type={type}
      name={name}
      placeholder={placeholder}
      valid={valid}
      invalid={invalid}
      defaultValue={value}
    />
  );
}

InputField.propTypes = {
  inputField: PropTypes.shape({
    type: PropTypes.string.isRequired,
    name: PropTypes.string.isRequired,
    placeholder: PropTypes.string.isRequired,
    valid: PropTypes.bool.isRequired,
    invalid: PropTypes.bool.isRequired,
    value: PropTypes.string.isRequired
  }).isRequired
};
