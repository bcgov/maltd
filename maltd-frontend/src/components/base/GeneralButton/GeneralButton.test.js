/* eslint-disable react/jsx-filename-extension */
import React from "react";
import renderer from "react-test-renderer";
import GeneralButton from "./GeneralButton";

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
      />
    );

    const tree = component.toJSON();
    expect(tree).toMatchSnapshot();
  });
});
