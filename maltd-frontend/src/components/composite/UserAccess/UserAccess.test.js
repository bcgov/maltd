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
      />
    );

    const rows = component.find("#user-access-row");
    expect(rows.length).toEqual(1);
    expect(
      rows
        .first()
        .find("td")
        .find("#noProjects").length
    ).toEqual(1);
  });

  test("Component renders with existing projects when there are projects for a user", () => {
    const component = shallow(
      <UserAccess
        userAccess={{
          projects: [{ name: "Project1" }, { name: "Project2" }],
          userName,
          userEmail
        }}
      />
    );

    const rows = component.find("#user-access-row");
    expect(rows.length).toEqual(1);
    expect(
      rows
        .first()
        .find("td")
        .find("#projects").length
    ).toEqual(1);
  });
});
