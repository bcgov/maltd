import React from "react";
import PropTypes from "prop-types";
import Icon from "../../../img/plus-icon.PNG";
import "./PlusIcon.css";

export default function PlusIcon({ onClick }) {
  return (
    <div
      role="button"
      tabIndex="0"
      onClick={onClick}
      onKeyDown={onClick}
      className="icon"
    >
      <img src={Icon} alt="plus icon" width="25px" height="25px" />
    </div>
  );
}

PlusIcon.propTypes = {
  onClick: PropTypes.func.isRequired
};
