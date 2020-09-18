import React from "react";
import renderer from "react-test-renderer";
import Adapter from "enzyme-adapter-react-16";
import Enzyme, { shallow } from "enzyme";
import UserSearch from "./UserSearch";

Enzyme.configure({ adapter: new Adapter() });

describe("User Search", () => {
  const inputField = {
    type: "text",
    name: "myinputfield",
    placeholder: "placeholder",
    valid: false,
    invalid: false,
    value: "idir",
    disabled: false
  };

  const generalButton = {
    type: "submit",
    color: "primary",
    disabled: true,
    label: "Find"
  };

  test("Component renders as expected", () => {
    const component = renderer.create(
      <UserSearch
        userSearch={{
          state: { isLoading: false, userExists: null }
        }}
        inputField={inputField}
        onClick={() => jest.fn()}
        onChange={() => jest.fn()}
        generalButton={generalButton}
      />
    );

    const tree = component.toJSON();
    expect(tree).toMatchSnapshot();
  });

  test("Component does not render a loading message when no results are loading", () => {
    const component = shallow(
      <UserSearch
        userSearch={{
          state: { isLoading: false, userExists: null }
        }}
        inputField={inputField}
        onClick={() => jest.fn()}
        onChange={() => jest.fn()}
        generalButton={generalButton}
      />
    );

    expect(component.exists("#loading")).toEqual(false);
  });

  test("Component renders an error message when there are no results returned for a user search query", () => {
    const component = shallow(
      <UserSearch
        userSearch={{
          state: { isLoading: false, userExists: false }
        }}
        inputField={inputField}
        onClick={() => jest.fn()}
        onChange={() => jest.fn()}
        generalButton={generalButton}
      />
    );

    expect(component.exists(".error-message")).toEqual(true);
  });
});
