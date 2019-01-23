import React from "react";
import { Route } from "react-router";
import Layout from "./components/Layout";
import Home from "./components/Home";
import Upload from "./pages/Upload";

export default () => (
  <Layout>
    <Route exact path="/" component={Home} />
    <Route path="/upload" component={Upload} />
  </Layout>
);
