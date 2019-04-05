import React from "react";
import Accordion from "../components/Accordion";
import AccordionSection from "../components/AccordionSection";
import Details from "../components/Details";
import Link from "../components/Link";
import PrototypeAbsenceData from "./components/PrototypeAbsenceData";
import PrototypeDataSample from "./components/PrototypeDataSample";
import PrototypeMap from "./components/PrototypeMap";
import PrototypePage from "./components/PrototypePage";
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

      <nav className="app-navigation">
        <ul className="app-navigation__list govuk-!-margin-bottom-0">
          <li className="app-navigation--current-page">
            <a href="#" className="govuk-link govuk-link--no-visited-state">
              Add data
            </a>
          </li>
          <li>
            <a href="#" className="govuk-link govuk-link--no-visited-state">
              Add / edit content
            </a>
          </li>
          <li>
            <a href="#" className="govuk-link govuk-link--no-visited-state">
              Schedule release date
            </a>
          </li>
          <li>
            <a href="#" className="govuk-link govuk-link--no-visited-state">
              Set status
            </a>
          </li>
        </ul>
      </nav>

      <h2 className="govuk-heading-m">Add data to release</h2>
    </PrototypePage>
  );
};

export default PublicationPage;
