import React from "react";
import PropTypes from "prop-types";
import XIcon from "../XIcon/XIcon";
import "./ListElement.css";

export default function ListElement({
  listElement: { title, description, id },
  onXClick
}) {
  if (title) {
    return (
      <>
        <div>
          <strong className="large-size">{title}</strong>
          {onXClick && <XIcon id={id} onClick={onXClick} />}
        </div>
        {description && <p>{description}</p>}
      </>
    );
  }

  return (
    <div>
      <p>No projects</p>
    </div>
  );
}

ListElement.propTypes = {
  listElement: PropTypes.shape({
    title: PropTypes.string,
    description: PropTypes.string,
    id: PropTypes.string
  }).isRequired,
  onXClick: PropTypes.func
};
