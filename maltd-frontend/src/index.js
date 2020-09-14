import React from "react";
import ReactDOM from "react-dom";
import axios from "axios";
import "./index.css";
import App from "./App";
import * as serviceWorker from "./serviceWorker";
import "bootstrap/dist/css/bootstrap.css";

if (window.REACT_APP_MALTD_API) {
  axios.defaults.baseURL = window.REACT_APP_MALTD_API;
} else if (process.env.REACT_APP_MALTD_API) {
  axios.defaults.baseURL = process.env.REACT_APP_MALTD_API;
}

if (window.REACT_APP_MALTD_API_TIMEOUT) {
  axios.defaults.timeout = window.REACT_APP_MALTD_API_TIMEOUT;
} else if (process.env.REACT_APP_MALTD_API_TIMEOUT) {
  axios.defaults.timeout = process.env.REACT_APP_MALTD_API_TIMEOUT;
} else {
  axios.defaults.timeout = 2 * 60 * 1000; // default to 2 minutes
}

ReactDOM.render(<App />, document.getElementById("root"));

// If you want your app to work offline and load faster, you can change
// unregister() to register() below. Note this comes with some pitfalls.
// Learn more about service workers: https://bit.ly/CRA-PWA
serviceWorker.unregister();
