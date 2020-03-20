import React from "react";
import PropTypes from "prop-types";
import XIcon from "../XIcon/XIcon";
import "./ListElement.css";

export default function ListElement({
  listElement: { title, description, resources, id },
  onXClick
}) {
  let memberOfResources = "";
  let errorMessage;
  let message;

  console.log("title", title);

  if (resources && resources.length > 0) {
    resources.forEach((resource, index) => {
      console.log("list element resource", resource);
      if (resource.message) {
        errorMessage = `Couldn't add/remove to ${resource.type}`;
        message = resource.message;
      } else if (index !== resources.length - 1) {
        memberOfResources += `${resource.type}, `;
      } else {
        memberOfResources += resource.type;
      }
    });
  }

  if (title && (memberOfResources || description)) {
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
              <p className="project-list-item" id="member-resources">
                {memberOfResources}
              </p>
            )}
            {errorMessage && (
              <React.Fragment>
                <p className="project-list-item" id="member-resources">
                  {errorMessage}
                </p>
                <p className="project-list-item" id="member-resources">
                  {message}
                </p>
              </React.Fragment>
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
      {errorMessage && (
        <React.Fragment>
          <p className="project-list-item" id="member-resources">
            {errorMessage}
          </p>
          <p className="project-list-item" id="member-resources">
            {message}
          </p>
        </React.Fragment>
      )}
      {!errorMessage && <p>No projects</p>}
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
