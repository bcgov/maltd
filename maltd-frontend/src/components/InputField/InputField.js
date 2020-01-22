import React from 'react';
import { Input } from 'reactstrap';
import PropTypes from 'prop-types';
import './InputField.css';

export default function InputField({ inputField: { name, type, placeholder, valid, invalid, value }, onUpdate }) {
  return (
    <Input
      className="input-field"
      type={type}
      name={name}
      placeholder={placeholder}
      valid={valid}
      invalid={invalid}
      value={value}
      onChange={() => onUpdate()}
    />
  );
}

InputField.propTypes = {
  inputField: PropTypes.shape({
    type: PropTypes.string.isRequired,
    placeholder: PropTypes.string.isRequired,
    name: PropTypes.string.isRequired,
    valid: PropTypes.bool.isRequired,
    value: PropTypes.string.isRequired
  }),
  onUpdate: PropTypes.func,
};
