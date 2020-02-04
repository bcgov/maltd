/* eslint-disable react/jsx-filename-extension */
import React, { useState } from "react";
import "./MainPage.css";
import UserSearch from "../../composite/UserSearch/UserSearch";
import NavBar from "../../base/NavBar/NavBar";
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
  const [color, setColor] = useState("primary");
  const [userExists, setUserExists] = useState(null);

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
    color,
    disabled: disabledButton,
    label: "Find"
  };

  const userSearch = {
    state: {
      isLoading,
      userExists
    }
  };

  const userAccess = {
    projects,
    userName: value,
    userEmail
  };

  const navBar = {
    isAuthed: true
  };

  function clearErrorMessage() {
    setTimeout(() => {
      setUserExists(null);
    }, 2000);
  }

  function clearForm() {
    setUserExists(false);
    clearErrorMessage();
    setIsLoading(false);
    setDisabledButton(true);
    setDisabledInput(false);
    setInvalidInput(false);
    setValidInput(false);
    setValue("");
  }

  function onLogoutClick() {}

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
          clearForm();
        }
      })
      .catch(() => {
        clearForm();
      });
  }

  function onInputChange(event) {
    const val = event.target.value;

    if (val.length === 0) {
      setInvalidInput(false);
      setValidInput(false);
      setDisabledButton(true);
      setColor("primary");
    } else if (val.length < 5) {
      setInvalidInput(true);
      setColor("danger");
    } else {
      setInvalidInput(false);
      setValidInput(true);
      setDisabledButton(false);
      setColor("primary");
    }

    setValue(event.target.value);
  }

  return (
    <>
      <NavBar navBar={navBar} onClick={onLogoutClick} />
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
    </>
  );
}
