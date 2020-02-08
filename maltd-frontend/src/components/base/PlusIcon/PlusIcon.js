import React from "react";
import PropTypes from "prop-types";
import Icon from "../../../img/plus-icon.PNG";

export default function PlusIcon({ onClick }) {
  return (
    <div
      id="main"
      role="button"
      tabIndex="0"
      onClick={onClick}
      onKeyDown={onClick}
    >
      <img src={Icon} alt="plus icon" width="25px" height="25px" />
    </div>
  );
}

PlusIcon.propTypes = {
  onClick: PropTypes.func.isRequired
};
