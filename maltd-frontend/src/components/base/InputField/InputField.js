/* eslint-disable react/jsx-props-no-spreading */
import React from "react";
import { Input } from "reactstrap";
import PropTypes from "prop-types";
import "./InputField.css";

export default function InputField({
  inputField: { name, type, placeholder, valid, invalid, value, disabled },
  onChange
}) {
  return (
    <Input
      className="input-field my-2"
      type={type}
      disabled={disabled}
      name={name}
      placeholder={placeholder}
      valid={valid}
      invalid={invalid}
      value={value}
      {...{
        ...(onChange && { onChange })
      }}
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
    value: PropTypes.string.isRequired,
    disabled: PropTypes.bool.isRequired
  }).isRequired,
  onChange: PropTypes.func
};

InputField.defaultProps = {
  onChange: () => {}
};
