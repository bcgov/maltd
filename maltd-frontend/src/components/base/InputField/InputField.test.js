/* eslint-disable react/jsx-filename-extension */
import React from "react";
import renderer from "react-test-renderer";
import InputField from "./InputField";

describe("Input Field", () => {
  test("Component renders as expected", () => {
    const component = renderer.create(
      <InputField
        inputField={{
          type: "text",
          placeholder: "placeholder",
          name: "myinputfield",
          valid: false,
          invalid: false,
          val: "idir"
        }}
      />
    );

    const tree = component.toJSON();
    expect(tree).toMatchSnapshot();
  });
});
