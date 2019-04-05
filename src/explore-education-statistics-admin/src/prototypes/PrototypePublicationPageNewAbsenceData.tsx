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
import { PrototypeEditableContent } from "./components/PrototypeEditableContent";
import TabsSection from "../components/TabsSection";

const PublicationDataPage = () => {
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
      <PrototypeAdminNavigation sectionId="addData" />

      <h2 className="govuk-heading-m">Add data to release</h2>

      <div className="govuk-form-group">
        <label className="govuk-label" htmlFor="file-upload-1">
          Upload a file
        </label>
        <input
          className="govuk-file-upload"
          id="file-upload-1"
          name="file-upload-1"
          type="file"
        />
      </div>
    </PrototypePage>
  );
};

export default PublicationDataPage;
