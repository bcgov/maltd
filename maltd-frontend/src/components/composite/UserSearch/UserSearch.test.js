/* eslint-disable react/jsx-filename-extension */
import React from "react";
import renderer from "react-test-renderer";
import Adapter from "enzyme-adapter-react-16";
import Enzyme, { mount } from "enzyme";
import { Spinner } from "reactstrap";
import UserSearch from "./UserSearch";

Enzyme.configure({ adapter: new Adapter() });

describe("User Search", () => {
  const inputField = {
    type: "text",
    placeholder: "placeholder",
    name: "myinputfield",
    valid: false,
    invalid: false,
    value: "idir"
  };

  const generalButton = {
    type: "submit",
    color: "success",
    disabled: true,
    block: false,
    active: false,
    outline: false,
    label: "Find",
    styling: "generic-classname"
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

  test("Component renders with a loading spinner when in the loading state", () => {
    const component = mount(
      <UserSearch
        userSearch={{
          state: { isLoading: true }
        }}
        inputField={inputField}
        generalButton={generalButton}
      />
    );

    expect(component.find(Spinner)).toHaveLength(1);
  });
});
