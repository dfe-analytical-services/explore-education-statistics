import React from 'react';
import { RouteChildrenProps } from 'react-router';
import Link from '@admin/components/Link';
import PrototypeAdminNavigation from './components/PrototypeAdminNavigation';
import PrototypePage from './components/PrototypePage';

const UpdateStatusForm = () => {
  return (
    <>
      <form action="/prototypes/publication-create-new-absence-status">
        <div className="govuk-form-group">
          <fieldset className="govuk-fieldset">
            <legend className="govuk-fieldset__legend govuk-fieldset__legend--m">
              Update release status
            </legend>
            <p className="govuk-body">Select and update release status.</p>
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
                  In draft
                </label>
              </div>

              <div className="govuk-radios__item">
                <input
                  className="govuk-radios__input"
                  type="radio"
                  name="status"
                  id="readyHigherReview"
                  value="readyHigherReview"
                  data-aria-controls="content-for-approval"
                />
                <label
                  className="govuk-label govuk-radios__label"
                  htmlFor="readyHigherReview"
                >
                  Ready for higher review
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
      breadcrumbs={[
        {
          link: '/prototypes/admin-dashboard?status=editNewRelease',
          text: 'Administrator dashboard',
        },
        { text: 'Create new release', link: '#' },
      ]}
    >
      <PrototypeAdminNavigation sectionId="status" />

      {location.search.includes('?status=ready') === false && (
        <UpdateStatusForm />
      )}

      {location.search === '?status=readyTeamApproval' && (
        <>
          <div className="govuk-panel govuk-panel--confirmation">
            <h1 className="govuk-panel__title">Release ready for review</h1>
            <div className="govuk-panel__body">
              Check the 'Comments for you to resolve' tab on your{' '}
              <Link
                className="govuk-link dfe-link--white"
                to="/prototypes/admin-dashboard?status=readyApproval"
              >
                dashboard
              </Link>{' '}
              for feedback
            </div>
          </div>
        </>
      )}
      {location.search === '?status=readyHigherReview' && (
        <>
          <div className="govuk-panel govuk-panel--confirmation">
            <h1 className="govuk-panel__title">
              Release ready for final sign-off
            </h1>
            <div className="govuk-panel__body">
              Check the 'Comments for you to resolve' tab on your{' '}
              <Link
                className="dfe-link--white"
                to="/prototypes/admin-dashboard?status=readyHigherReview"
              >
                dashboard
              </Link>{' '}
              for feedback
            </div>
          </div>
        </>
      )}

      <hr className="govuk-!-margin-top-9" />
      <div className="govuk-grid-row govuk-!-margin-top-9">
        <div className="govuk-grid-column-one-half ">
          <Link to="/prototypes/publication-create-new-absence">
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
