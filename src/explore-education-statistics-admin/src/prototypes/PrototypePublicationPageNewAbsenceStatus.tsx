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
      <PrototypeAdminNavigation sectionId="status" />

      <div className="govuk-form-group">
        <fieldset className="govuk-fieldset">
          <legend className="govuk-fieldset__legend govuk-fieldset__legend--m">
            Set page status
          </legend>
          <div className="govuk-radios">
            <div className="govuk-radios__item">
              <input
                className="govuk-radios__input"
                type="radio"
                name="status"
                id="edit"
                value="edit"
                defaultChecked
              />
              <label className="govuk-label govuk-radios__label" htmlFor="edit">
                Editing in progress
              </label>
            </div>

            <div className="govuk-radios__item">
              <input
                className="govuk-radios__input"
                type="radio"
                name="status"
                id="readyApproval"
                value="edit"
              />
              <label
                className="govuk-label govuk-radios__label"
                htmlFor="readyApproval"
              >
                Ready for approval
              </label>
            </div>

            <div className="govuk-radios__item">
              <input
                className="govuk-radios__input"
                type="radio"
                name="status"
                id="cancelEdit"
                value="edit"
                data-aria-controls="content-for-approval"
              />
              <label
                className="govuk-label govuk-radios__label"
                htmlFor="cancelEdit"
              >
                Cancel editing
              </label>
            </div>
          </div>
        </fieldset>
        <button className="govuk-button govuk-!-margin-top-3">
          Update page status
        </button>
      </div>
    </PrototypePage>
  );
};

export default PublicationDataPage;
