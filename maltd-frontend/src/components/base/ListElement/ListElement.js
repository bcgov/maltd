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
      <React.Fragment>
        <div className="project-div">
          <div className="project-list-item">
            <strong className="large-size">{title}</strong>
            {description && <p className="project-list-item">{description}</p>}
          </div>
          <div>
            {typeof onXClick === "function" && (
              <XIcon id={id} onClick={onXClick} />
            )}
          </div>
        </div>
      </React.Fragment>
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

ListElement.defaultProps = {
  onXClick: null
};
