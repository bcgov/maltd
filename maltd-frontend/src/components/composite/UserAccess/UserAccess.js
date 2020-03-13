import React from "react";
import PropTypes from "prop-types";
import { Container, Row, Col } from "reactstrap";
import ListElement from "../../base/ListElement/ListElement";
import PlusIcon from "../../base/PlusIcon/PlusIcon";
import Dropdown from "../../base/Dropdown/Dropdown";
import "./UserAccess.css";

export default function UserAccess({
  userAccess: { projects, userName, userEmail },
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

  return (
    <Container className="move-down">
      <Row xs="1" sm="1" md="2" id="user-access-row">
        <Col className="cols">
          <Row className="inner-row">
            <Col className="big-font">USER</Col>
          </Row>
          <ListElement listElement={userListElement} />
          {duplicateErrorMessage && (
            <small className="error-message">{duplicateErrorMessage}</small>
          )}
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
      <Row xs="1" sm="1" md="2" className="outer">
        <Col />
        <Col>
          {projects && projects.length > 0 && (
            <div className="drop-plus" key={key}>
              <Dropdown dropdown={dropdown} onDropdownClick={onDropdownClick} />
              {dropdown.selectedDropdownItem && (
                <PlusIcon onClick={onPlusClick} />
              )}
            </div>
          )}
          {(!projects || projects.length === 0) && (
            <React.Fragment>
              <div className="drop-plus">
                <Dropdown
                  dropdown={dropdown}
                  onDropdownClick={onDropdownClick}
                />
                {dropdown.selectedDropdownItem && (
                  <PlusIcon onClick={onPlusClick} />
                )}
              </div>
            </React.Fragment>
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
  onXClick: PropTypes.func.isRequired,
  onPlusClick: PropTypes.func.isRequired,
  onDropdownClick: PropTypes.func.isRequired,
  dropdown: PropTypes.shape({
    items: PropTypes.array.isRequired,
    selectedDropdownItem: PropTypes.object
  }).isRequired,
  duplicateErrorMessage: PropTypes.string.isRequired
};
