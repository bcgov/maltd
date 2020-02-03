/* eslint-disable react/jsx-filename-extension */
import React, { useState } from "react";
import PropTypes from "prop-types";
import "./MainPage.css";
import UserSearch from "../../composite/UserSearch/UserSearch";
import UserAccess from "../../composite/UserAccess/UserAccess";

export default function MainPage() {
  // declare state variables, using hooks
  const [validInput, setValidInput] = useState(false);
  const [invalidInput, setInvalidInput] = useState(false);
  const [value, setValue] = useState("");
  const [disabledInput, setDisabledInput] = useState(false);
  const [disabledButton, setDisabledButton] = useState(true);
  const [isLoading, setIsLoading] = useState(false);
  const [isUserSearch, setIsUserSearch] = useState(true);
  const [projects, setProjects] = useState([]);
  const [userEmail, setUserEmail] = useState(null);

  function onButtonClick() {
    setIsLoading(true);
    setDisabledButton(true);
    setDisabledInput(true);

    fetch(`https://localhost:5001/api/users/${value}`)
      .then(res => res.json())
      .then(result => {
        if (result.status !== 404) {
          setProjects(result.projects);
          if (result.email) {
            setUserEmail(result.email);
          }

          setIsUserSearch(false);
        } else {
          setIsLoading(false);
          setDisabledButton(false);
          setDisabledInput(false);
        }
      });
  }

  function onInputChange(event) {
    const val = event.target.value;

    if (val.length === 0) {
      setInvalidInput(false);
      setValidInput(false);
    } else if (val.length < 5) {
      setInvalidInput(true);
    } else {
      setInvalidInput(false);
      setValidInput(true);
      setDisabledButton(false);
    }

    setValue(event.target.value);
  }

  const inputField = {
    type: "text",
    name: "idir",
    placeholder: "Enter IDIR username to find",
    valid: validInput,
    invalid: invalidInput,
    value,
    disabled: disabledInput
  };

  const generalButton = {
    type: "submit",
    color: "primary",
    disabled: disabledButton,
    label: "Find"
  };

  const userSearch = {
    state: {
      isLoading,
      userExists: null
    }
  };

  const userAccess = {
    projects,
    userName: value,
    userEmail
  };

  return (
    <div className="container my-3 p-3 rounded shadow">
      <h4>Add or Remove User</h4>
      {isUserSearch && (
        <UserSearch
          userSearch={userSearch}
          inputField={inputField}
          onChange={onInputChange}
          generalButton={generalButton}
          onClick={onButtonClick}
        />
      )}
      {!isUserSearch && <UserAccess userAccess={userAccess} />}
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
