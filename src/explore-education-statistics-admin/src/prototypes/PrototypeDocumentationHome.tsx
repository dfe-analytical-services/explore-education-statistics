import React from "react";
import Accordion from "../components/Accordion";
import AccordionSection from "../components/AccordionSection";
import Link from "../components/Link";
import PrototypeDownloadDropdown from "./components/PrototypeDownloadDropdown";
import PrototypePage from "./components/PrototypePage";
import Tabs from "../components/Tabs";
import TabsSection from "../components/TabsSection";

const BrowseReleasesPage = () => {
  return (
    <PrototypePage wide breadcrumbs={[{ text: "Documentation" }]}>
      <h1>Documentation</h1>
    </PrototypePage>
  );
};

export default BrowseReleasesPage;
