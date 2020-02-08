import React from "react";
import renderer from "react-test-renderer";
import XIcon from "./XIcon";

describe("X Icon", () => {
  test("Component renders as expected", () => {
    const component = renderer.create(<XIcon onClick={() => jest.fn()} />);

    const tree = component.toJSON();
    expect(tree).toMatchSnapshot();
  });
});
