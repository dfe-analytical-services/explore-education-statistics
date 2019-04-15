import React from 'react';
import Link from '../../../components/Link';

interface Props {
  sectionId?: string;
}

const PrototypeAdminNavigation = ({ sectionId }: Props) => {
  return (
    <>
      <span className="govuk-tag">New release in progress</span>
      <span className="govuk-caption-l">Academic year 2018 to 2019</span>
      <h1 className="govuk-heading-l">
        Pupil absence statistics and data for schools in England
      </h1>
      <dl className="dfe-meta-content">
        <dt className="govuk-caption-m">Scheduled publish date: </dt>
        <dd>
          <strong>To be set</strong>
        </dd>
      </dl>
      <nav className="app-navigation govuk-!-margin-bottom-9">
        <ul className="app-navigation__list govuk-!-margin-bottom-0">
          <li
            className={
              sectionId === 'setup'
                ? 'app-navigation--current-page'
                : 'app-navigation--non-selected-page'
            }
          >
            <Link
              to="/prototypes/publication-create-new-absence-config"
              className="govuk-link govuk-link--no-visited-state"
            >
              Release setup
            </Link>
          </li>
          <li
            className={
              sectionId === 'addData'
                ? 'app-navigation--current-page'
                : 'app-navigation--non-selected-page'
            }
          >
            <Link
              to="/prototypes/publication-create-new-absence-data"
              className="govuk-link govuk-link--no-visited-state"
            >
              Add data
            </Link>
          </li>
          <li
            className={
              sectionId === 'addContent'
                ? 'app-navigation--current-page'
                : 'app-navigation--non-selected-page'
            }
          >
            <Link
              to="/prototypes/publication-create-new-absence"
              className="govuk-link govuk-link--no-visited-state"
            >
              Add / edit content
            </Link>
          </li>
          <li
            className={
              sectionId === 'schedule'
                ? 'app-navigation--current-page'
                : 'app-navigation--non-selected-page'
            }
          >
            <Link
              to="/prototypes/publication-create-new-absence-schedule"
              className="govuk-link govuk-link--no-visited-state"
            >
              Schedule publish date
            </Link>
          </li>
          <li
            className={
              sectionId === 'status'
                ? 'app-navigation--current-page'
                : 'app-navigation--non-selected-page'
            }
          >
            <Link
              to="/prototypes/publication-create-new-absence-status"
              className="govuk-link govuk-link--no-visited-state"
            >
              Set status
            </Link>
          </li>
        </ul>
      </nav>
    </>
  );
};

export default PrototypeAdminNavigation;
