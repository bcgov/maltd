import React from "react";
import PropTypes from "prop-types";
import { Container, Row, Col } from "reactstrap";
import { Dropdown, Button } from "shared-components";
import ListElement from "../../base/ListElement/ListElement";
import "./UserAccess.css";

export default function UserAccess({
  userAccess: { projects, userName, userEmail },
  isButtonDisabled,
  onXClick,
  onPlusClick,
  onDropdownClick,
  dropdown,
  duplicateErrorMessage
}) {
  const userListElement = {
    title: userName,
    description: userEmail
  };
  const projectExists =
    projects && projects.length > 0 ? "projects" : "noProjects";
  let key = null;

  const projectNames = ["Select Project"];
  dropdown.items.forEach(item => {
    projectNames.push(item.name);
  });

  const onDropdownSelect = item => {
    let selectedItem;
    dropdown.items.forEach(i => {
      if (i.name === item) selectedItem = i;
    });

    onDropdownClick(selectedItem);
  };

  return (
    <Container className="move-down">
      <Row id="user-access-row">
        <Col className="cols">
          <Row className="inner-row">
            <Col className="big-font">USER</Col>
          </Row>
          <ListElement listElement={userListElement} />
        </Col>
        <Col className="top-projects" id={projectExists}>
          <div className="cols">
            <Row>
              <Col className="big-font">CURRENT PROJECTS</Col>
            </Row>
            {projectExists === "projects" &&
              projects.map(value => {
                const listElement = {
                  title: value.name,
                  resources: value.resources,
                  id: value.id
                };

                key = listElement.id;

                return (
                  <div key={key}>
                    <ListElement
                      listElement={listElement}
                      onXClick={onXClick}
                      id={key}
                    />
                  </div>
                );
              })}
            {(!projects || projects.length === 0) && (
              <ListElement listElement={{}} />
            )}
          </div>
        </Col>
      </Row>
      <br />
      <Row className="outer">
        <Col>
          {projects && projects.length > 0 && (
            <div className="drop-plus" key={key}>
              <Dropdown
                items={projectNames}
                onSelect={item => onDropdownSelect(item)}
              />
              {dropdown.selectedDropdownItem && (
                <Button
                  label="Add"
                  disabled={isButtonDisabled}
                  hasLoader={isButtonDisabled}
                  styling="bcgov-normal-blue btn add-project"
                  onClick={onPlusClick}
                />
              )}
            </div>
          )}
          {(!projects || projects.length === 0) && (
            <React.Fragment>
              <div className="drop-plus">
                <Dropdown
                  items={projectNames}
                  onSelect={item => onDropdownSelect(item)}
                />
                <div>
                  {dropdown.selectedDropdownItem && (
                    <Button
                      label="Add"
                      disabled={isButtonDisabled}
                      hasLoader={isButtonDisabled}
                      styling="bcgov-normal-blue btn add-project"
                      onClick={onPlusClick}
                    />
                  )}
                </div>
              </div>
            </React.Fragment>
          )}
        </Col>
        <Col>
          {duplicateErrorMessage && (
            <small className="error-message">{duplicateErrorMessage}</small>
          )}
        </Col>
      </Row>
    </Container>
  );
}

UserAccess.propTypes = {
  userAccess: PropTypes.shape({
    projects: PropTypes.array.isRequired,
    userName: PropTypes.string.isRequired,
    userEmail: PropTypes.string
  }).isRequired,
  isButtonDisabled: PropTypes.bool,
  onXClick: PropTypes.func.isRequired,
  onPlusClick: PropTypes.func.isRequired,
  onDropdownClick: PropTypes.func.isRequired,
  dropdown: PropTypes.shape({
    items: PropTypes.array.isRequired,
    selectedDropdownItem: PropTypes.object
  }).isRequired,
  duplicateErrorMessage: PropTypes.string
};

UserAccess.defaultProps = {
  duplicateErrorMessage: "",
  isButtonDisabled: false
};
