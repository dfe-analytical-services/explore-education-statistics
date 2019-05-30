import React from 'react';
import Link from '../../../components/Link';

interface Props {
  sectionId?: string;
  task?: string;
}

const PrototypeAdminNavigation = ({ sectionId, task }: Props) => {
  return (
    <>
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
              Add / edit data
            </Link>
          </li>
          <li
            className={
              sectionId === 'addTable'
                ? 'app-navigation--current-page'
                : 'app-navigation--non-selected-page'
            }
          >
            <Link
              to="/prototypes/publication-create-new-absence-table?status=step1"
              className="govuk-link govuk-link--no-visited-state"
            >
              Create data tables
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
              Preview / edit content
            </Link>
          </li>
          {/* <li
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
          </li> */}
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
              Set publish status
            </Link>
          </li>
        </ul>
      </nav>

      {task !== 'editRelease' && (
        <>
          <span className="govuk-tag">New release in progress</span>
          <h1 className="govuk-heading-l">
            Pupil absence statistics and data for schools in England{' '}
            <span className="govuk-caption-l">Academic year 2018 to 2019</span>
          </h1>
        </>
      )}
    </>
  );
};

export default PrototypeAdminNavigation;
