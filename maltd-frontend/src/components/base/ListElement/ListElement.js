import React from "react";
import PropTypes from "prop-types";

export default function ListElement({ listElement: { title, description } }) {
  return (
    <div>
      <strong>{title}</strong>
      <p>{description}</p>
    </div>
  );
}

ListElement.propTypes = {
  listElement: PropTypes.shape({
    title: PropTypes.string.isRequired,
    description: PropTypes.string.isRequired
  })
};
