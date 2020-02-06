/* eslint-disable react/jsx-filename-extension */
import React from "react";
import PropTypes from "prop-types";
import "./ListElement.css";

export default function ListElement({ listElement: { title, description } }) {
  if (title) {
    return (
      <div>
        <strong className="large-size">{title}</strong>
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
    title: PropTypes.string,
    description: PropTypes.string
  }).isRequired
};
