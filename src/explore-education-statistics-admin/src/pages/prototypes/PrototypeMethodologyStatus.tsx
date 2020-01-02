import React from 'react';
import { RouteChildrenProps } from 'react-router';
import Link from '@admin/components/Link';
import PrototypeMethodologyNavigation from './components/PrototypeMethodologyNavigation';
import PrototypePage from './components/PrototypePage';

const UpdateStatusForm = () => {
  return (
    <>
      <form action="/prototypes/publication-create-new-methodology-status">
        <div className="govuk-form-group">
          <fieldset className="govuk-fieldset">
            <legend className="govuk-fieldset__legend govuk-fieldset__legend--m">
              Update methodology status
            </legend>
            <p className="govuk-body">Select and update methodology status.</p>
            <div className="govuk-radios">
              <div className="govuk-radios__item">
                <input
                  className="govuk-radios__input"
                  type="radio"
                  name="status"
                  id="edit"
                  value="readyMethodologyApproval"
                  defaultChecked
                />
                <label
                  className="govuk-label govuk-radios__label"
                  htmlFor="edit"
                >
                  In draft
                </label>
              </div>

              <div className="govuk-radios__item">
                <input
                  className="govuk-radios__input"
                  type="radio"
                  name="status"
                  id="readyMethodologyHigherReview"
                  value="readyMethodologyHigherReview"
                  data-aria-controls="content-for-approval"
                />
                <label
                  className="govuk-label govuk-radios__label"
                  htmlFor="readyHigherReview"
                >
                  Ready for sign-off
                </label>
              </div>
            </div>
          </fieldset>
          <div className="govuk-form-group govuk-!-margin-top-6">
            <label
              htmlFor="release-notes"
              className="govuk-label govuk-label--s"
            >
              Internal release notes
              <span className="govuk-hint">
                This will be shown on the view draft methodology summary to help
                and update team members about this content.
              </span>
            </label>
            <textarea
              id="release-notes"
              className="govuk-textarea govuk-!-width-one-half"
            />
          </div>
          <button className="govuk-button govuk-!-margin-top-3" type="submit">
            Update
          </button>
        </div>
      </form>
    </>
  );
};

const PublicationDataPage = ({ location }: RouteChildrenProps) => {
  return (
    <PrototypePage
      wide
      breadcrumbs={[{ text: 'Create new release', link: '#' }]}
    >
      <PrototypeMethodologyNavigation sectionId="status" />

      {location.search.includes('?status=ready') === false && (
        <UpdateStatusForm />
      )}

      {location.search === '?status=readyMethodologyApproval' && (
        <>
          <div className="govuk-panel govuk-panel--confirmation">
            <h1 className="govuk-panel__title">Release in draft</h1>
            <div className="govuk-panel__body">
              You can return to 'View draft methodology' tab on your{' '}
              <Link
                className="govuk-link dfe-link--white"
                to="/prototypes/admin-dashboard?status=readyMethodologyApproval"
              >
                dashboard
              </Link>{' '}
              to view comments and continue editing.
            </div>
          </div>
        </>
      )}
      {location.search === '?status=readyMethodologyHigherReview' && (
        <>
          <div className="govuk-panel govuk-panel--confirmation">
            <h1 className="govuk-panel__title">
              Methodology ready for final sign-off
            </h1>
            <div className="govuk-panel__body">
              This methodology content has been passed on to the responsible
              statistican. You can still view this methodology under the 'View
              draft methodology' tab on your{' '}
              <Link
                className="dfe-link--white"
                to="/prototypes/admin-dashboard?status=readyMethodologyHigherReview"
              >
                dashboard
              </Link>{' '}
              .
            </div>
          </div>
        </>
      )}

      <hr className="govuk-!-margin-top-9" />
      <div className="govuk-grid-row govuk-!-margin-top-9">
        <div className="govuk-grid-column-one-half ">
          <Link to="/prototypes/methodology-edit">
            <span className="govuk-heading-m govuk-!-margin-bottom-0">
              Previous step
            </span>
            Manage content
          </Link>
        </div>
        {location.search === '?status=readyTeamApproval' && (
          <div className="govuk-grid-column-one-half dfe-align--right">
            <Link to="/prototypes/admin-dashboard?status=readyApproval">
              <span className="govuk-heading-m govuk-!-margin-bottom-0">
                Next step
              </span>
              Go to dashboard
            </Link>
          </div>
        )}
        {location.search === '?status=readyHigherReview' && (
          <div className="govuk-grid-column-one-half dfe-align--right">
            <Link to="/prototypes/admin-dashboard?status=readyHigherReview">
              <span className="govuk-heading-m govuk-!-margin-bottom-0">
                Next step
              </span>
              Go to dashboard
            </Link>
          </div>
        )}
      </div>
    </PrototypePage>
  );
};

export default PublicationDataPage;
