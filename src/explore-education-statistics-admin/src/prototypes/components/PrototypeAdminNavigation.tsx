import React from "react";
import Link from "../../components/Link";

interface Props {
  sectionId?: string;
}

const PrototypeAdminNavigation = ({ sectionId }: Props) => {
  return (
    <>
      <nav className="app-navigation govuk-!-margin-bottom-9">
        <ul className="app-navigation__list govuk-!-margin-bottom-0">
          <li
            className={
              sectionId === "addData"
                ? "app-navigation--current-page"
                : "app-navigation--non-selected-page"
            }
          >
            <a
              href="/prototypes/publication-create-new-absence-data"
              className="govuk-link govuk-link--no-visited-state"
            >
              Add data
            </a>
          </li>
          <li
            className={
              sectionId === "addContent"
                ? "app-navigation--current-page"
                : "app-navigation--non-selected-page"
            }
          >
            <a
              href="/prototypes/publication-create-new-absence"
              className="govuk-link govuk-link--no-visited-state"
            >
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
    </>
  );
};

export default PrototypeAdminNavigation;
