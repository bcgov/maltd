/* eslint-disable react/jsx-filename-extension */
import React from "react";
import renderer from "react-test-renderer";
import SearchBox from "./SearchBox";

describe("Search Box", () => {
  test("Search box component renders as expected", () => {
    const component = renderer.create(
      <SearchBox
        searchBox={{
          type: "submit",
          color: "success",
          disabled: true,
          block: false,
          active: false,
          outline: false
        }}
      />
    );

    const tree = component.toJSON();
    expect(tree).toMatchSnapshot();
  });
});
