/* eslint-disable react/jsx-filename-extension */
import React from "react";
import PropTypes from "prop-types";
import { Table } from "reactstrap";
import ListElement from "../../base/ListElement/ListElement";

export default function UserAccess({
  userAccess: { projects, userName, userEmail }
}) {
  const userListElement = {
    title: userName,
    description: userEmail
  };

  return (
    <Table borderless>
      <thead>
        <tr className="border-bottom">
          <th>USER</th>
          <th>CURRENT PROJECTS</th>
        </tr>
      </thead>
      <tbody>
        <tr>
          <td>
            <ListElement listElement={userListElement} />
          </td>
          <td>
            {projects.map(value => {
              const listElement = { title: value.name };
              return <ListElement key={value.name} listElement={listElement} />;
            })}
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
