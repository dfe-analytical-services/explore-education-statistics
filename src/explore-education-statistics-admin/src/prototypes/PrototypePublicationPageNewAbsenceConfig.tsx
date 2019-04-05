import React from "react";
import Accordion from "../components/Accordion";
import AccordionSection from "../components/AccordionSection";
import Details from "../components/Details";
import Link from "../components/Link";
import PrototypeAbsenceData from "./components/PrototypeAbsenceData";
import PrototypeAdminNavigation from "./components/PrototypeAdminNavigation";
import PrototypeDataSample from "./components/PrototypeDataSample";
import PrototypeMap from "./components/PrototypeMap";
import PrototypePage from "./components/PrototypePage";
import PrototypePublicationConfig from "./components/PrototypePublicationPageConfig";
import { PrototypeEditableContent } from "./components/PrototypeEditableContent";
import TabsSection from "../components/TabsSection";

const PublicationPage = () => {
  let sectionId = "setup";

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
      <PrototypeAdminNavigation sectionId={sectionId} />
      <PrototypePublicationConfig sectionId={sectionId} />
    </PrototypePage>
  );
};

export default PublicationPage;
