import React from 'react';
import RelatedInformation from '@common/components/RelatedInformation';
import Link from '@admin/components/Link';

interface Props {
  sectionId?: string;
  task?: string;
}

const PrototypeMethodologyNavigation = ({ sectionId }: Props) => {
  return (
    <>
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <h1 className="govuk-heading-xl">
            <span className="govuk-caption-l">Create new methodology</span>{' '}
            Example statistics: methodology{' '}
          </h1>
        </div>
        <div className="govuk-grid-column-one-third">
          <RelatedInformation heading="Help and guidance">
            <ul className="govuk-list">
              <li>
                <Link to="#" target="blank">
                  Creating new methodology{' '}
                </Link>
              </li>
            </ul>
          </RelatedInformation>
        </div>
      </div>

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
              to="/prototypes/publication-create-new-methodology-config"
              className="govuk-link govuk-link--no-visited-state"
            >
              Methodology summary
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
              to="/prototypes/methodology-edit"
              className="govuk-link govuk-link--no-visited-state"
            >
              Manage content
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
              to="/prototypes/publication-create-new-methodology-status"
              className="govuk-link govuk-link--no-visited-state"
            >
              Update methodology status
            </Link>
          </li>
        </ul>
      </nav>
    </>
  );
};

export default PrototypeMethodologyNavigation;
