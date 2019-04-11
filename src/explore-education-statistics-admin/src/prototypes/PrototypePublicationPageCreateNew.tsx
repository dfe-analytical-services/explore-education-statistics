import React from "react";
import Accordion from "../components/Accordion";
import AccordionSection from "../components/AccordionSection";
import Details from "../components/Details";
import Link from "../components/Link";
import PrototypeAbsenceData from "./components/PrototypeAbsenceData";
import PrototypeDataSample from "./components/PrototypeDataSample";
import PrototypeMap from "./components/PrototypeMap";
import PrototypePage from "./components/PrototypePage";
import PrototypePublicationConfig from "./components/PrototypePublicationPageConfig";
import { PrototypeEditableContent } from "./components/PrototypeEditableContent";
import TabsSection from "../components/TabsSection";

const PublicationPage = () => {
  let mapRef: PrototypeMap | null = null;

  return (
    <PrototypePage
      wide
      breadcrumbs={[
        {
          link: "/prototypes/admin-dashboard",
          text: "Administrator dashboard"
        },
        { text: "Create new release", link: "#" }
      ]}
    >
      <h1 className="govuk-heading-xl">Create new release</h1>
      <PrototypePublicationConfig />
    </PrototypePage>
  );
};

export default PublicationPage;
