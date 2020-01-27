/* eslint-disable react/jsx-filename-extension */
import React from "react";
import renderer from "react-test-renderer";
import ListElement from "./ListElement";

describe("List Element", () => {
  test("Component renders as expected", () => {
    const component = renderer.create(
      <ListElement
        listElement={{
          title: "title",
          description: "description"
        }}
      />
    );

    const tree = component.toJSON();
    expect(tree).toMatchSnapshot();
  });
});
