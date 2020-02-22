import React from "react";
import renderer from "react-test-renderer";
import GeneralButton from "./GeneralButton.jsx";

describe("General Button", () => {
  test("Component renders as expected", () => {
    const component = renderer.create(
      <GeneralButton
        generalButton={{
          type: "submit",
          color: "primary",
          disabled: true,
          label: "Find"
        }}
        onClick={() => jest.fn()}
      />
    );

    const tree = component.toJSON();
    expect(tree).toMatchSnapshot();
  });
});
