import React from "react";
import PropTypes from "prop-types";
import { Table } from "reactstrap";
import ListElement from "../../base/ListElement/ListElement";
import PlusIcon from "../../base/PlusIcon/PlusIcon";
import Dropdown from "../../base/Dropdown/Dropdown";
import "./UserAccess.css";

export default function UserAccess({
  userAccess: { projects, userName, userEmail },
  onXClick,
  onPlusClick,
  onDropdownClick,
  dropdown
}) {
  const userListElement = {
    title: userName,
    description: userEmail
  };
  const projectExists =
    projects && projects.length > 0 ? "projects" : "noProjects";
  let key = null;

  return (
    <Table borderless className="move-down">
      <thead>
        <tr className="border-bottom">
          <th>
            <strong className="big-font">USER</strong>
          </th>
          <th>
            <strong className="big-font">CURRENT PROJECTS</strong>
          </th>
        </tr>
      </thead>
      <tbody>
        <tr id="user-access-row">
          <td>
            <ListElement listElement={userListElement} />
          </td>
          <td id={projectExists}>
            {projects &&
              projects.length > 0 &&
              projects.map(value => {
                const listElement = {
                  title: value.name,
                  description: value.type,
                  id: value.id
                };
                key = listElement.id;
                return (
                  <div key={key}>
                    <ListElement
                      listElement={listElement}
                      onXClick={onXClick}
                    />
                    <p />
                  </div>
                );
              })}
            {projects && projects.length > 0 && (
              <div key={key}>
                <Dropdown
                  dropdown={dropdown}
                  onDropdownClick={onDropdownClick}
                />
                <div className="icon-alignment">
                  <PlusIcon onClick={onPlusClick} />
                </div>
              </div>
            )}
            {(!projects || projects.length === 0) && (
              <React.Fragment>
                <ListElement listElement={{}} />
                <div>
                  <Dropdown
                    dropdown={dropdown}
                    onDropdownClick={onDropdownClick}
                  />
                  <div className="icon-alignment">
                    <PlusIcon onClick={onPlusClick} />
                  </div>
                </div>
              </React.Fragment>
            )}
          </td>
        </tr>
      </tbody>
    </Table>
  );
}

UserAccess.propTypes = {
  userAccess: PropTypes.shape({
    projects: PropTypes.array.isRequired,
    userName: PropTypes.string.isRequired,
    userEmail: PropTypes.string.isRequired
  }).isRequired,
  onXClick: PropTypes.func.isRequired,
  onPlusClick: PropTypes.func.isRequired,
  onDropdownClick: PropTypes.func.isRequired,
  dropdown: PropTypes.shape({
    items: PropTypes.array.isRequired
  }).isRequired
};
