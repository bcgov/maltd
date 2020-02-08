import React from "react";
import PropTypes from "prop-types";
import Icon from "../../../img/x-icon.PNG";

export default function XIcon({ onClick }) {
  return (
    <div
      id="main"
      role="button"
      tabIndex="0"
      onClick={onClick}
      onKeyDown={onClick}
    >
      <img src={Icon} alt="x icon" width="25px" height="25px" />
    </div>
  );
}

XIcon.propTypes = {
  onClick: PropTypes.func.isRequired
};
