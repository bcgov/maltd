import React from "react";
import PropTypes from "prop-types";
import XIcon from "../XIcon/XIcon";
import "./ListElement.css";

export default function ListElement({
  listElement: { title, description, resources, id },
  onXClick
}) {
  let memberOfResources = "";

  if (resources && resources.length > 0) {
    resources.forEach((resource, index) => {
      if (index !== resources.length - 1) {
        memberOfResources += `${resource}, `;
      } else {
        memberOfResources += resource;
      }
    });
  }

  if (title) {
    return (
      <React.Fragment>
        <div className="project-div">
          <div className="project-list-item" id="project-info">
            <strong className="large-size">{title}</strong>
            {description && (
              <p className="project-list-item" id="email-info">
                {description}
              </p>
            )}
            {memberOfResources && (
              <p className="project-list-item" id="email-info">
                {memberOfResources}
              </p>
            )}
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
    resources: PropTypes.array,
    id: PropTypes.string
  }).isRequired,
  onXClick: PropTypes.func
};

ListElement.defaultProps = {
  onXClick: null
};
