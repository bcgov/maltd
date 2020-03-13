import React from "react";
import renderer from "react-test-renderer";
import Adapter from "enzyme-adapter-react-16";
import Enzyme, { shallow } from "enzyme";
import UserAccess from "./UserAccess";

Enzyme.configure({ adapter: new Adapter() });

describe("User Access", () => {
  const userName = "username";
  const userEmail = "useremail@gov.bc.ca";

  test("Component renders as expected", () => {
    const component = renderer.create(
      <UserAccess
        userAccess={{
          projects: [],
          userName,
          userEmail
        }}
        onXClick={() => jest.fn()}
        onPlusClick={() => jest.fn()}
        onDropdownClick={() => jest.fn()}
        dropdown={{
          items: [{ id: "123", name: "name", type: "type" }]
        }}
      />
    );

    const tree = component.toJSON();
    expect(tree).toMatchSnapshot();
  });

  test("Component renders with no projects text when there are no projects for a user", () => {
    const component = shallow(
      <UserAccess
        userAccess={{
          projects: [],
          userName,
          userEmail
        }}
        onXClick={() => jest.fn()}
        onPlusClick={() => jest.fn()}
        onDropdownClick={() => jest.fn()}
        dropdown={{
          items: [{ id: "123", name: "name", type: "type" }]
        }}
      />
    );

    const rows = component.find("#user-access-row");
    expect(rows.length).toEqual(1);
    expect(
      rows
        .first()
        .find("Col")
        .find("#noProjects").length
    ).toEqual(1);
  });

  test("Component renders with existing projects when there are projects for a user where they are a member of the resource", () => {
    const component = shallow(
      <UserAccess
        userAccess={{
          projects: [
            {
              name: "Project1",
              id: "123",
              resources: [{ type: "Dyn", status: "member" }]
            },
            {
              name: "Project2",
              id: "1234",
              resources: [{ type: "Dyn", status: "member" }]
            }
          ],
          userName,
          userEmail
        }}
        onXClick={() => jest.fn()}
        onPlusClick={() => jest.fn()}
        onDropdownClick={() => jest.fn()}
        dropdown={{
          items: [{ id: "123", name: "name", type: "type" }]
        }}
      />
    );

    const rows = component.find("#user-access-row");
    expect(rows.length).toEqual(1);
    expect(
      rows
        .first()
        .find("Col")
        .find("#projects").length
    ).toEqual(1);
  });

  test("Component renders with existing projects when there are projects for a user where they are not a member of the resource", () => {
    const component = shallow(
      <UserAccess
        userAccess={{
          projects: [
            {
              name: "Project1",
              id: "123",
              resources: [{ type: "Dyn", status: "non-member" }]
            },
            {
              name: "Project2",
              id: "1234",
              resources: [{ type: "Dyn", status: "non-member" }]
            }
          ],
          userName,
          userEmail
        }}
        onXClick={() => jest.fn()}
        onPlusClick={() => jest.fn()}
        onDropdownClick={() => jest.fn()}
        dropdown={{
          items: [{ id: "123", name: "name", type: "type" }]
        }}
      />
    );

    const rows = component.find("#user-access-row");
    expect(rows.length).toEqual(1);
    expect(
      rows
        .first()
        .find("Col")
        .find("#projects").length
    ).toEqual(1);
  });

  test("Component renders with an error message when a duplicate project is attempted to be added", () => {
    const component = shallow(
      <UserAccess
        userAccess={{
          projects: [],
          userName,
          userEmail
        }}
        onXClick={() => jest.fn()}
        onPlusClick={() => jest.fn()}
        onDropdownClick={() => jest.fn()}
        dropdown={{
          items: [{ id: "123", name: "name", type: "type" }]
        }}
        duplicateErrorMessage="error message"
      />
    );

    const rows = component.find(".error-message");
    expect(rows.length).toEqual(1);
    expect(rows.text()).toEqual("error message");
  });
});
