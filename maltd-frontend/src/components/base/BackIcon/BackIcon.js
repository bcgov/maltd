/* eslint-disable react/jsx-filename-extension */
import React from "react";
import { FaAngleLeft } from "react-icons/fa";
import "./BackIcon.css";

export default function BackIcon({ backIcon: { message }, onClick }) {
  return (
    <div id="main" onClick={onClick}>
      <FaAngleLeft className="size-large" />
      <div className="limit-width">
        <h5>Back</h5>
        <p>{message}</p>
      </div>
    </div>
  );
}
