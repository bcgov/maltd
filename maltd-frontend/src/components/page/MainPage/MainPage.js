/* eslint-disable react/jsx-filename-extension */
import React from "react";
import PropTypes from "prop-types";
import "./MainPage.css";
import UserSearch from "../../composite/UserSearch/UserSearch";

export default function MainPage() {
  const inputField = {
    type: "text",
    name: "idir",
    placeholder: "Enter IDIR username to find",
    valid: false,
    invalid: false,
    value: "",
    disabled: false
  };

  const generalButton = {
    type: "submit",
    color: "primary",
    disabled: true,
    label: "Find"
  };

  const userSearch = {
    state: {
      isLoading: false,
      userExists: null
    }
  };

  return (
    <div className="container my-3 p-3 rounded shadow">
      <h4>Add or Remove User</h4>
      <UserSearch
        userSearch={userSearch}
        inputField={inputField}
        generalButton={generalButton}
      />
    </div>
  );
}

MainPage.propTypes = {
  userSearch: PropTypes.shape({
    state: PropTypes.any.isRequired
  }).isRequired,
  inputField: PropTypes.shape({
    type: PropTypes.string.isRequired,
    name: PropTypes.string.isRequired,
    placeholder: PropTypes.string.isRequired,
    valid: PropTypes.bool.isRequired,
    invalid: PropTypes.bool.isRequired,
    value: PropTypes.string.isRequired,
    disabled: PropTypes.bool.isRequired
  }).isRequired,
  generalButton: PropTypes.shape({
    type: PropTypes.string.isRequired,
    color: PropTypes.string.isRequired,
    disabled: PropTypes.bool.isRequired,
    label: PropTypes.any.isRequired
  }).isRequired
};
