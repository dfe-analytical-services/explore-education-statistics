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

      <h2 className="govuk-heading-m">Current data for this release</h2>
      <dl className="govuk-summary-list">
        <div className="govuk-summary-list__row">
          <dt className="govuk-summary-list__key">Data upload 1</dt>
          <dd className="govuk-summary-list__value">
            <a href="#">absence_geoglevels.csv</a>
          </dd>
          <dd className="govuk-summary-list__actions">
            <a href="#">Remove</a> | <a href="#">Replace file</a>
          </dd>
        </div>
        <div className="govuk-summary-list__row">
          <dt className="govuk-summary-list__key">Data upload 2</dt>
          <dd className="govuk-summary-list__value">
            <a href="#">absence_lacharacteristics.csv</a>
          </dd>
          <dd className="govuk-summary-list__actions">
            <a href="#">Remove</a> | <a href="#">Replace file</a>
          </dd>
        </div>
        <div className="govuk-summary-list__row">
          <dt className="govuk-summary-list__key">Data upload 3</dt>
          <dd className="govuk-summary-list__value">
            <a href="#">absence_natcharacteristics.csv</a>
          </dd>
          <dd className="govuk-summary-list__actions">
            <a href="#">Remove</a> | <a href="#">Replace file</a>
          </dd>
        </div>
      </dl>

      <h2 className="govuk-heading-m govuk-!-margin-top-9">
        Add new data to release
      </h2>

      <div className="govuk-form-group">
        <label className="govuk-label" htmlFor="file-upload-1">
          Upload a file test
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
