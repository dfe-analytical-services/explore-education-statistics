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

const PublicationSchedulePage = () => {
  return (
    <PrototypePage
      wide
      breadcrumbs={[
        {
          link: "/prototypes/admin-dashboard?status=editRelease",
          text: "Administrator dashboard"
        },
        { text: "Create new release", link: "#" }
      ]}
    >
      <PrototypeAdminNavigation sectionId="schedule" />

      <form action="/prototypes/publication-create-new-absence-schedule">
        <fieldset className="govuk-fieldset">
          <legend className="govuk-fieldset__legend govuk-fieldset__legend--m">
            Schedule publish date
          </legend>
          <div className="govuk-date-input" id="schedule-publish-date">
            <div className="govuk-date-input__item">
              <div className="govuk-form-group">
                <label
                  htmlFor="schedule-day"
                  className="govuk-label govuk-date-input__label"
                >
                  Day
                </label>
                <input
                  className="govuk-input govuk-date-input__inout govuk-input--width-2"
                  id="schedule-day"
                  name="schedule-day"
                  type="number"
                  pattern="[0-9]*"
                />
              </div>
            </div>
            <div className="govuk-date-input__item">
              <div className="govuk-form-group">
                <label
                  htmlFor="schedule-month"
                  className="govuk-label govuk-date-input__label"
                >
                  Month
                </label>
                <input
                  className="govuk-input govuk-date-input__inout govuk-input--width-2"
                  id="schedule-month"
                  name="schedule-month"
                  type="number"
                  pattern="[0-9]*"
                />
              </div>
            </div>
            <div className="govuk-date-input__item">
              <div className="govuk-form-group">
                <label
                  htmlFor="schedule-year"
                  className="govuk-label govuk-date-input__label"
                >
                  Year
                </label>
                <input
                  className="govuk-input govuk-date-input__inout govuk-input--width-4"
                  id="schedule-year"
                  name="schedule-year"
                  type="number"
                  pattern="[0-9]*"
                />
              </div>
            </div>
          </div>
        </fieldset>
        <fieldset className="govuk-fieldset govuk-!-margin-top-9">
          <legend className="govuk-fieldset__legend govuk-fieldset__legend--m">
            Schedule publish time
          </legend>
          <div className="govuk-date-input" id="schedule-publish-date">
            <div className="govuk-date-input__item">
              <div className="govuk-form-group">
                <label
                  htmlFor="schedule-hours"
                  className="govuk-label govuk-date-input__label"
                >
                  Hour
                </label>
                <input
                  className="govuk-input govuk-date-input__inout govuk-input--width-2"
                  id="schedule-hours"
                  name="schedule-hours"
                  type="number"
                  pattern="[0-9]*"
                  value="09"
                />
              </div>
            </div>
            <div className="govuk-date-input__item">
              <div className="govuk-form-group">
                <label
                  htmlFor="schedule-mins"
                  className="govuk-label govuk-date-input__label"
                >
                  Minutes
                </label>
                <input
                  className="govuk-input govuk-date-input__inout govuk-input--width-2"
                  id="schedule-mins"
                  name="schedule-mins"
                  type="number"
                  pattern="[0-9]*"
                  value="30"
                />
              </div>
            </div>
          </div>
          <div className="govuk-form-group govuk-!-margin-top-6">
            <button type="submit" className="govuk-button">
              Update scheduled publish date and time
            </button>
          </div>
        </fieldset>
      </form>
      <div className="govuk-!-margin-top-6">
        <Link to="/prototypes/publication-create-new-absence-schedule">
          Cancel update
        </Link>
      </div>
    </PrototypePage>
  );
};

export default PublicationSchedulePage;
