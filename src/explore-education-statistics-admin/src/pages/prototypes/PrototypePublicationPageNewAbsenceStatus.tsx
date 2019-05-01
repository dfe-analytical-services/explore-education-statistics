import React from 'react';
import { RouteChildrenProps } from 'react-router';
import Link from '../../components/Link';
import PrototypeAdminNavigation from './components/PrototypeAdminNavigation';
import PrototypePage from './components/PrototypePage';

const PublicationDataPage = ({ location }: RouteChildrenProps) => {
  return (
    <PrototypePage
      wide
      breadcrumbs={[
        {
          link: '/prototypes/admin-dashboard?status=editNewRelease',
          text: 'Administrator dashboard',
        },
        { text: 'Create new release', link: '#' },
      ]}
    >
      <PrototypeAdminNavigation sectionId="status" />
      {location.search === '?status=readyTeamApproval' && (
        <>
          <div className="govuk-panel govuk-panel--confirmation">
            <h1 className="govuk-panel__title">
              Release ready for team approval
            </h1>
            <div className="govuk-panel__body">
              This release had been marked for moderation by the team.
            </div>
          </div>
          <div className="govuk-!-margin-top-6 govuk-!-margin-bottom-6">
            <Link to="/prototypes/publication-create-new-absence-status">
              Edit status
            </Link>
          </div>
          <Link to="/prototypes/admin-dashboard?status=readyApproval">
            Back to administrator dashboard
          </Link>
        </>
      )}
      {location.search !== '?status=readyTeamApproval' && (
        <form action="/prototypes/publication-create-new-absence-status">
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
                    value="editRelease"
                    defaultChecked
                  />
                  <label
                    className="govuk-label govuk-radios__label"
                    htmlFor="edit"
                  >
                    Editing in progress
                  </label>
                </div>

                <div className="govuk-radios__item">
                  <input
                    className="govuk-radios__input"
                    type="radio"
                    name="status"
                    id="readyTeamApproval"
                    value="readyTeamApproval"
                  />
                  <label
                    className="govuk-label govuk-radios__label"
                    htmlFor="readyApproval"
                  >
                    Level 1: Ready for team approval
                  </label>
                </div>

                <div className="govuk-radios__item">
                  <input
                    className="govuk-radios__input"
                    type="radio"
                    name="status"
                    id="cancelEdit"
                    value="delete"
                    data-aria-controls="content-for-approval"
                  />
                  <label
                    className="govuk-label govuk-radios__label"
                    htmlFor="readyTeamLeadApproval"
                  >
                    Level 2: Ready for team lead appoval
                  </label>
                </div>
              </div>
            </fieldset>
            <div className="govuk-form-group govuk-!-margin-top-6">
              <label
                htmlFor="release-notes"
                className="govuk-label govuk-label--s"
              >
                Release notes
              </label>
              <textarea
                id="release-notes"
                className="govuk-textarea govuk-!-width-one-half"
              />
            </div>
            <button className="govuk-button govuk-!-margin-top-3" type="submit">
              Update page status
            </button>
          </div>
        </form>
      )}
    </PrototypePage>
  );
};

export default PublicationDataPage;
