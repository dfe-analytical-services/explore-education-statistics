import React from 'react';
import Details from '@common/components/Details';
import {
  FormGroup,
  FormFieldset,
  FormTextInput,
  FormSelect,
  // FormRadioGroup,
} from '@common/components/form';
import PrototypeAdminExampleTables from './components/PrototypeAdminExampleTables';
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

      <h2 className="govuk-heading-m">Build tables for release</h2>

      <p className="govuk-body">
        Choose the data from your uploaded files then use filters to create your
        table.
      </p>

      <p className="govuk-body">
        Once you've created a table, you can save it for use in your release.
      </p>

      <hr />

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
        <dl className="govuk-summary-list govuk-!-margin-0 govuk-summary-list--no-border">
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
        <dl className="govuk-summary-list govuk-!-margin-0 govuk-summary-list--no-border">
          <div className="govuk-summary-list__row">
            <dt className="govuk-summary-list__key">Location</dt>
            <dd className="govuk-summary-list__value">National</dd>
            <dd className="govuk-summary-list__actions">
              <Link to="publication-create-new-absence-table?status=step2">
                Change location
              </Link>
            </dd>
          </div>
        </dl>
      )}
      <hr />
      <h2 className="govuk-heading-m">3. Time period</h2>
      {window.location.search === '?status=step3' && (
        <>
          <FormGroup>
            <FormFieldset id="lead" legend="">
              <FormGroup>
                <FormSelect
                  id="start-date"
                  label="Start"
                  name="start-date"
                  options={[
                    { label: '2012', value: '2012' },
                    { label: '2013', value: '2013' },
                    { label: '2014', value: '2014' },
                    { label: '2015', value: '2015' },
                    { label: '2016', value: '2016' },
                    { label: '2017', value: '2017' },
                    { label: '2018', value: '2018' },
                    { label: '2019', value: '2019' },
                  ]}
                />
              </FormGroup>
              <FormGroup>
                <FormSelect
                  id="end-date"
                  label="End"
                  name="end-date"
                  options={[
                    { label: '2012', value: '2012' },
                    { label: '2013', value: '2013' },
                    { label: '2014', value: '2014' },
                    { label: '2015', value: '2015' },
                    { label: '2016', value: '2016' },
                    { label: '2017', value: '2017' },
                    { label: '2018', value: '2018' },
                    { label: '2019', value: '2019' },
                  ]}
                />
              </FormGroup>
            </FormFieldset>
          </FormGroup>
          <Link
            to="publication-create-new-absence-table?status=step4"
            className="govuk-button govuk-!-margin-right-3"
          >
            Next step
          </Link>
          <Link
            to="publication-create-new-absence-table?status=step2"
            className="govuk-button govuk-button--secondary"
          >
            Previous step
          </Link>
        </>
      )}
      {['?status=step4', '?status=step5'].includes(window.location.search) && (
        <>
          <dl className="govuk-summary-list govuk-!-margin-0 govuk-summary-list--no-border">
            <div className="govuk-summary-list__row">
              <dt className="govuk-summary-list__key">Start date</dt>
              <dd className="govuk-summary-list__value">2012 to 2013</dd>
              <dd className="govuk-summary-list__actions" />
            </div>
            <div className="govuk-summary-list__row">
              <dt className="govuk-summary-list__key">End date</dt>
              <dd className="govuk-summary-list__value">2016 to 2017</dd>
              <dd className="govuk-summary-list__actions">
                <Link to="publication-create-new-absence-table?status=step3">
                  Change time period
                </Link>
              </dd>
            </div>
          </dl>
        </>
      )}
      <hr />
      <h2 className="govuk-heading-m" id="tableFilters">
        4. Filters
      </h2>
      {['?status=step4', '?status=step5'].includes(window.location.search) && (
        <>
          <h3 className="govuk-heading-s">
            Categories
            <span className="govuk-hint">
              Select at least one option from each category
            </span>
          </h3>

          <Details
            summary="Characteristic"
            tag="1 selected"
            className="govuk-!-margin-bottom-2"
          >
            <div className="dfe-filter-overflow">
              <img
                src="/static/images/prototype/characteristic-filter.png"
                alt=""
              />
            </div>
          </Details>
          <Details summary="School type" tag="1 selected">
            <div className="dfe-filter-overflow">
              <img src="/static/images/prototype/school-filter.png" alt="" />
            </div>
          </Details>

          <h3 className="govuk-heading-s">
            Indicators
            <span className="govuk-hint">Select at least one indicator</span>
          </h3>
          <Details summary="Indicator" tag="3 selected">
            <div className="dfe-filter-overflow">
              <img
                src="/static/images/prototype/indicator-filters.png"
                alt=""
              />
            </div>
          </Details>
          <Link
            to="publication-create-new-absence-table?status=step5"
            className="govuk-button govuk-!-margin-right-5"
          >
            Update table
          </Link>
          <Link
            to="publication-create-new-absence-table?status=step3"
            className="govuk-button govuk-button--secondary"
          >
            Previous step
          </Link>
        </>
      )}
      <hr />
      <h2 className="govuk-heading-m">5. View and save table</h2>
      {['?status=step5'].includes(window.location.search) && (
        <div className="govuk-width-container">
          <PrototypeAdminExampleTables task="edit" />

          <FormGroup>
            <FormTextInput
              id="save-table"
              name="save-table"
              label="Save as"
              value="Table for absence highlights panel"
              width={20}
            />
          </FormGroup>

          <Link
            to="/prototypes/publication-create-new-absence-table?status=step1"
            className="govuk-button govuk-!-margin-right-3"
          >
            Save and create another table
          </Link>

          <Link
            to="publication-create-new-absence-view-table"
            className="govuk-button govuk-!-margin-right-3"
          >
            Save and continue
          </Link>
        </div>
      )}
      <hr />
      <div className="govuk-grid-row govuk-!-margin-top-9">
        <div className="govuk-grid-column-one-half ">
          <Link to="/prototypes/publication-create-new-absence-data">
            Previous step, add / edit data
          </Link>
        </div>
        <div className="govuk-grid-column-one-half dfe-align--right">
          <Link to="/prototypes/publication-create-new-absence-view-table">
            Next step, view / edit tables
          </Link>
        </div>
      </div>
    </PrototypePage>
  );
};

export default PublicationDataPage;
