/* eslint-disable react/jsx-filename-extension */
import React from "react";
import PropTypes from "prop-types";

export default function ListElement({
  listElement: { title, description },
  id
}) {
  if (title) {
    return (
      <div>
        <strong>{title}</strong>
        {description && <p>{description}</p>}
      </div>
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
    title: PropTypes.string.isRequired,
    description: PropTypes.string
  }).isRequired,
  id: PropTypes.string
};
