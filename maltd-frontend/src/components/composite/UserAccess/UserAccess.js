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
              projects.map(value => {
                const listElement = { title: value.name, id: value.id };
                return (
                  <>
                    <ListElement
                      key={value.name}
                      listElement={listElement}
                      onXClick={onXClick}
                    />
                    <p />
                  </>
                );
              })}
            <div>
              <Dropdown dropdown={dropdown} onDropdownClick={onDropdownClick} />
              <PlusIcon onClick={onPlusClick} />
            </div>
            {(!projects || projects.length === 0) && (
              <>
                <ListElement listElement={{}} onPlusClick={onPlusClick} />
                <div>
                  <Dropdown
                    dropdown={dropdown}
                    onDropdownClick={onDropdownClick}
                  />
                  <PlusIcon onClick={onPlusClick} />
                </div>
              </>
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
  onXClick: PropTypes.func.isRequired
};
