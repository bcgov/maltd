import React from "react";
import renderer from "react-test-renderer";
import InputField from "./InputField.jsx";

describe("Input Field", () => {
  test("Component renders as expected", () => {
    const component = renderer.create(
      <InputField
        inputField={{
          type: "text",
          name: "myinputfield",
          placeholder: "placeholder",
          valid: false,
          invalid: false,
          value: "idir",
          disabled: false
        }}
        onChange={() => jest.fn()}
      />
    );

    const tree = component.toJSON();
    expect(tree).toMatchSnapshot();
  });
});
