import React from "react";
import ListElement from "../../base/ListElement/ListElement";
import { Table } from "reactstrap";

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
