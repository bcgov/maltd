/* eslint-disable react/jsx-filename-extension */
import React from "react";
import ListElement from "./components/base/ListElement/ListElement";

export default function App() {
  const listElement = {
    title: "test title",
    description: "test description"
  };

  return (
    <div>
      <h1>MALTD Frontend</h1>
      <ListElement listElement={listElement} />
      <ListElement listElement={listElement} />
    </div>
  );
}
