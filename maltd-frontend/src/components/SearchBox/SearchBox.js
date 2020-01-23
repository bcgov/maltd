/* eslint-disable react/jsx-filename-extension */
import React from "react";
import { Button } from "reactstrap";
import PropTypes from "prop-types";
import "./SearchBox.css";

export default function SearchBox({
  searchBox: { type, color, disabled, block, active, outline }
}) {
  return (
    <Button
      className="search-box"
      type={type}
      color={color}
      disabled={disabled}
      block={block}
      active={active}
      outline={outline}
    >
      Search
    </Button>
  );
}

SearchBox.propTypes = {
  searchBox: PropTypes.shape({
    type: PropTypes.string.isRequired,
    color: PropTypes.string.isRequired,
    disabled: PropTypes.bool.isRequired,
    block: PropTypes.bool.isRequired,
    active: PropTypes.bool.isRequired,
    outline: PropTypes.bool.isRequired
  }).isRequired
};
