import React from "react";
import PropTypes from "prop-types";
import XIcon from "../XIcon/XIcon";
import "./ListElement.css";

export default function ListElement({
  listElement: { title, description, resources, id },
  onXClick
}) {
  let memberOfResources = "";
  let notMemberOfResources = "";
  let message;

  if (resources && resources.length > 0) {
    resources.forEach((resource, index) => {
      if (resource.message) {
        notMemberOfResources += `${resource.type}`;
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
                Member: {memberOfResources}
              </p>
            )}
            {notMemberOfResources && (
              <React.Fragment>
                <p className="project-list-item" id="member-resources">
                  Not member: {notMemberOfResources}
                </p>
                <p className="project-list-item" id="member-resources">
                  <b>Error:</b> {message}
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
