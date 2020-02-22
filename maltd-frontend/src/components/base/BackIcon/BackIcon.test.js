import React from "react";
import renderer from "react-test-renderer";
import BackIcon from "./BackIcon";

describe("Back Icon", () => {
  test("Component renders as expected", () => {
    const component = renderer.create(
      <BackIcon
        backIcon={{
          message: "Find another user"
        }}
        onClick={() => jest.fn()}
      />
    );

    const tree = component.toJSON();
    expect(tree).toMatchSnapshot();
  });
});
