import React from "react";
import PropTypes from "prop-types";
import { MdDeleteForever } from "react-icons/md";
import "./ListElement.css";

export default function ListElement({
  listElement: { title, description, resources, id },
  onXClick
}) {
  let memberOfResources = "";
  let notMemberOfResources = "";
  let message;

  if (resources && resources.length > 0) {
    resources.forEach(resource => {
      if (resource.message) {
        notMemberOfResources += resource.type;
        message = resource.message;
      } else if (resource.status !== "not-member") {
        memberOfResources += `${resource.type} `;
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
              <MdDeleteForever
                className="pointer"
                size={32}
                onClick={() => onXClick(id)}
              />
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
