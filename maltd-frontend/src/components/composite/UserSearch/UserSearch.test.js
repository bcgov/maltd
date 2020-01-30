/* eslint-disable react/jsx-filename-extension */
import React from "react";
import renderer from "react-test-renderer";
import Adapter from "enzyme-adapter-react-16";
import { mount } from "enzyme";
import Enzyme from "enzyme";
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
          state: { isLoading: false }
        }}
        inputField={inputField}
        generalButton={generalButton}
      />
    );

    const tree = component.toJSON();
    expect(tree).toMatchSnapshot();
  });

  test("Component renders a loading message when search results are loading", () => {
    const component = mount(
      <UserSearch
        userSearch={{
          state: { isLoading: true }
        }}
        inputField={inputField}
        generalButton={generalButton}
      />
    );

    expect(component.exists("#loading")).toEqual(true);
  });

  test("Component does not render a loading message when no results are loading", () => {
    const component = mount(
      <UserSearch
        userSearch={{
          state: { isLoading: false }
        }}
        inputField={inputField}
        generalButton={generalButton}
      />
    );

    expect(component.exists("#loading")).toEqual(false);
  });
});
