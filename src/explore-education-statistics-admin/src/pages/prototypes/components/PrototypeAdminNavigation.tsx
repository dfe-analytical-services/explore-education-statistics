import React from 'react';
import Link from '@admin/components/Link';

interface Props {
  sectionId?: string;
  task?: string;
}

const PrototypeAdminNavigation = ({ sectionId }: Props) => {
  return (
    <>
      <h1 className="govuk-heading-xl">
        <span className="govuk-caption-xl">Create new release</span>
        Pupil absence statistics and data for schools in England{' '}
      </h1>

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
              Release summary
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
              Manage data
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
              Manage data blocks
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
              Manage content
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
              Update release status
            </Link>
          </li>
        </ul>
      </nav>
    </>
  );
};

export default PrototypeAdminNavigation;
