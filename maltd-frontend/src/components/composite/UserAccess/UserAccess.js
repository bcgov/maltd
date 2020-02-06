/* eslint-disable react/jsx-filename-extension */
import React from "react";
import PropTypes from "prop-types";
import { Table } from "reactstrap";
import ListElement from "../../base/ListElement/ListElement";
import "./UserAccess.css";

export default function UserAccess({
  userAccess: { projects, userName, userEmail }
}) {
  const userListElement = {
    title: userName,
    description: userEmail
  };
  const projectExists =
    projects && projects.length > 0 ? "projects" : "noProjects";

  return (
    <Table borderless>
      <thead>
        <tr className="border-bottom">
          <th>USER</th>
          <th>CURRENT PROJECTS</th>
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
                const listElement = { title: value.name };
                return (
                  <ListElement key={value.name} listElement={listElement} />
                );
              })}
            {(!projects || projects.length === 0) && (
              <ListElement listElement={{}} />
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
  }).isRequired
};
