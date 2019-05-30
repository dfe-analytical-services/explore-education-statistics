import React from 'react';
import Details from '@common/components/Details';
import {
  FormGroup,
  FormFieldset,
  FormTextInput,
  FormSelect,
  FormRadioGroup,
} from '@common/components/form';
import PrototypeAdminNavigation from './components/PrototypeAdminNavigation';
import PrototypePage from './components/PrototypePage';
import Link from '../../components/Link';

const PublicationDataPage = () => {
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
      <PrototypeAdminNavigation sectionId="addTable" />
      <h2 className="govuk-heading-m">1. Select data file</h2>
      {window.location.search === '?status=step1' && (
        <form method="get">
          <fieldset className="govuk-fieldset govuk-!-margin-bottom-6">
            <legend className="govuk-fieldset__legend govuk-fieldset__legend--s govuk-visually-hidden">
              Select file
            </legend>
            <div className="govuk-radios">
              <div className="govuk-radios__item">
                <input
                  type="radio"
                  name="data-type"
                  id="data-type-geographical"
                  value="data-type-geographical"
                  className="govuk-radios__input"
                  defaultChecked
                />
                <label
                  htmlFor="data-type-geographical"
                  className="govuk-label govuk-radios__label"
                >
                  Geographical absence
                </label>
              </div>
              <div className="govuk-radios__item">
                <input
                  type="radio"
                  name="data-type"
                  id="data-type-local-authority"
                  value="data-type-local-authority"
                  className="govuk-radios__input"
                />
                <label
                  htmlFor="data-type-local-authority"
                  className="govuk-label govuk-radios__label"
                >
                  Local authority
                </label>
              </div>
              <div className="govuk-radios__item">
                <input
                  type="radio"
                  name="data-type"
                  id="data-type-revised"
                  value="revised"
                  className="govuk-radios__input"
                />
                <label
                  htmlFor="data-type-revised"
                  className="govuk-label govuk-radios__label"
                >
                  Revised
                </label>
              </div>
            </div>
          </fieldset>
          <Link
            to="publication-create-new-absence-table?status=step2"
            className="govuk-button"
          >
            Next step
          </Link>
        </form>
      )}
      {window.location.search !== '?status=step1' && (
        <dl className="govuk-summary-list govuk-summary-list--no-border">
          <div className="govuk-summary-list__row">
            <dt className="govuk-summary-list__key">Selected data</dt>
            <dd className="govuk-summary-list__value">Geographical absence</dd>
            <dd className="govuk-summary-list__actions">
              <Link to="publication-create-new-absence-table?status=step1">
                Change file
              </Link>
            </dd>
          </div>
        </dl>
      )}
      <hr />
      <h2 className="govuk-heading-m">2. Choose location</h2>
      {window.location.search === '?status=step2' && (
        <>
          <Details summary="National" tag="1 selected">
            <fieldset
              className="govuk-fieldset"
              id="locationFiltersForm-levels-National"
            >
              <legend className="govuk-fieldset__legend govuk-fieldset__legend--m govuk-visually-hidden">
                National
              </legend>
              <div className="govuk-checkboxes">
                <div className="govuk-checkboxes__item">
                  <input
                    className="govuk-checkboxes__input"
                    id="locationFiltersForm-levels-National-e-92000001"
                    name="locations.National"
                    type="checkbox"
                    value="E92000001"
                    defaultChecked
                  />
                  <label
                    className="govuk-label govuk-checkboxes__label"
                    htmlFor="locationFiltersForm-levels-National-e-92000001"
                  >
                    England
                  </label>
                </div>
              </div>
            </fieldset>
          </Details>
          <Details summary="Regional">regional items</Details>
          <Details summary="Local authority">Local authority items</Details>
          <Details summary="Local authority district">
            Local authority district items
          </Details>
          <Link
            to="publication-create-new-absence-table?status=step3"
            className="govuk-button govuk-!-margin-right-3"
          >
            Next step
          </Link>
          <Link
            to="publication-create-new-absence-table?status=step1"
            className="govuk-button govuk-button--secondary"
          >
            Previous step
          </Link>
        </>
      )}

      {['?status=step3', '?status=step4', '?status=step5'].includes(
        window.location.search,
      ) && (
        <dl className="govuk-summary-list govuk-summary-list--no-border">
          <div className="govuk-summary-list__row">
            <dt className="govuk-summary-list__key">Location</dt>
            <dd className="govuk-summary-list__value">National</dd>
            <dd className="govuk-summary-list__actions">
              <Link to="publication-create-new-absence-table?status=step2">
                Change file
              </Link>
            </dd>
          </div>
        </dl>
      )}
      <hr />
      <h2 className="govuk-heading-m">3. Time period</h2>
      <hr />
      <h2 className="govuk-heading-m">4. Filters</h2>
      <hr />
      <h2 className="govuk-heading-m">5. View and save table</h2>
      <hr />
    </PrototypePage>
  );
};

export default PublicationDataPage;
