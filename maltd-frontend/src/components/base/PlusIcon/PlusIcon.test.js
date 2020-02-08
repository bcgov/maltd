import React from "react";
import renderer from "react-test-renderer";
import PlusIcon from "./PlusIcon";

describe("Plus Icon", () => {
  test("Component renders as expected", () => {
    const component = renderer.create(<PlusIcon onClick={() => jest.fn()} />);

    const tree = component.toJSON();
    expect(tree).toMatchSnapshot();
  });
});
