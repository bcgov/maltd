import React from "react";
import PropTypes from "prop-types";
import { FaAngleLeft } from "react-icons/fa";
import "./BackIcon.css";

export default function BackIcon({ backIcon: { message }, onClick }) {
  return (
    <div
      id="main"
      role="button"
      tabIndex="0"
      onClick={onClick}
      onKeyDown={onClick}
    >
      <FaAngleLeft className="size-large" />
      <div className="limit-width">
        <h5>Back</h5>
        <p>{message}</p>
      </div>
    </div>
  );
}

BackIcon.propTypes = {
  backIcon: PropTypes.shape({
    message: PropTypes.string.isRequired
  }).isRequired,
  onClick: PropTypes.func.isRequired
};
