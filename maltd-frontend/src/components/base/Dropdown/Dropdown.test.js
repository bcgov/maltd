import React from "react";
import renderer from "react-test-renderer";
import Dropdown from "./Dropdown";

describe("Dropdown", () => {
  test("Component renders as expected", () => {
    const component = renderer.create(
      <Dropdown
        dropdown={{
          items: [
            {
              id: "123",
              name: "System 1",
              type: "Dynamics"
            }
          ]
        }}
        onDropdownClick={() => jest.fn()}
      />
    );

    const tree = component.toJSON();
    expect(tree).toMatchSnapshot();
  });
});
