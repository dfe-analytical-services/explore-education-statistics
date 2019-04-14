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
          link: '/prototypes/admin-dashboard',
          text: 'Administrator dashboard',
        },
        { text: 'Create new release', link: '#' },
      ]}
    >
      <PrototypeAdminNavigation sectionId="status" />
      {location.search == '?status=readyApproval' && (
        <>
          <div className="govuk-panel govuk-panel--confirmation">
            <h1 className="govuk-panel__title">Release ready for approval</h1>
            <div className="govuk-panel__body">
              This release had been sent for moderation. You will receive a
              notification in your dashboard once a decision has been made.
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
      {location.search != '?status=readyApproval' && (
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
                    id="readyApproval"
                    value="readyApproval"
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
                    value="delete"
                    data-aria-controls="content-for-approval"
                  />
                  <label
                    className="govuk-label govuk-radios__label"
                    htmlFor="cancelEdit"
                  >
                    Cancel and remove this release
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
            <button className="govuk-button govuk-!-margin-top-3">
              Update page status
            </button>
          </div>
        </form>
      )}
    </PrototypePage>
  );
};

export default PublicationDataPage;
