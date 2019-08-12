import Details from '@common/components/Details';
import {
  FormFieldset,
  FormGroup,
  FormSelect,
  FormTextInput,
} from '@common/components/form';
import React from 'react';
import Link from '../../components/Link';
import PrototypeAdminExampleTables from './components/PrototypeAdminExampleTables';
import PrototypeAdminNavigation from './components/PrototypeAdminNavigation';
import PrototypePage from './components/PrototypePage';

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

      <div className="govuk-tabs">
        <ul className="govuk-tabs__list">
          <li className="govuk-tabs__list-item">
            <Link
              to="/prototypes/publication-create-new-absence-table"
              className="govuk-tabs__tab govuk-tabs__tab--selected"
            >
              Create data blocks
            </Link>
          </li>
          <li className="govuk-tabs__list-item">
            <Link
              to="/prototypes/publication-create-new-absence-view-table"
              className="govuk-tabs__tab"
            >
              View data blocks
            </Link>
          </li>
        </ul>
        <div className="govuk-tabs__panel">
          <h2 className="govuk-heading-">
            Create a data block for this release
          </h2>

          <p className="govuk-body">
            Choose the data you want to use from your uploaded files and then
            use filters to create your data block.
          </p>

          <h2 className="govuk-heading-m">1. Choose data</h2>
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
                <dd className="govuk-summary-list__value">
                  Geographical absence
                </dd>
                <dd className="govuk-summary-list__actions">
                  <Link to="publication-create-new-absence-table?status=step1">
                    Change
                  </Link>
                </dd>
              </div>
            </dl>
          )}
          <hr />
          <h2 className="govuk-heading-m">2. Choose locations</h2>
          {window.location.search === '?status=step2' && (
            <>
              <p>Select at least one</p>
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
                    Change
                  </Link>
                </dd>
              </div>
            </dl>
          )}
          <hr />
          <h2 className="govuk-heading-m">3. Choose time period</h2>
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
          {['?status=step4', '?status=step5'].includes(
            window.location.search,
          ) && (
            <>
              <dl className="govuk-summary-list govuk-!-margin-0 govuk-summary-list--no-border">
                <div className="govuk-summary-list__row">
                  <dt className="govuk-summary-list__key">Start</dt>
                  <dd className="govuk-summary-list__value">2012 to 2013</dd>
                  <dd className="govuk-summary-list__actions" />
                </div>
                <div className="govuk-summary-list__row">
                  <dt className="govuk-summary-list__key">End</dt>
                  <dd className="govuk-summary-list__value">2016 to 2017</dd>
                  <dd className="govuk-summary-list__actions">
                    <Link to="publication-create-new-absence-table?status=step3">
                      Change
                    </Link>
                  </dd>
                </div>
              </dl>
            </>
          )}
          <hr />
          <h2 className="govuk-heading-m" id="tableFilters">
            4. Choose filters
          </h2>
          {['?status=step4', '?status=step5'].includes(
            window.location.search,
          ) && (
            <>
              <h3 className="govuk-heading-s">
                Categories
                <span className="govuk-hint">
                  Select at least one from each category
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
                  <img
                    src="/static/images/prototype/school-filter.png"
                    alt=""
                  />
                </div>
              </Details>

              <h3 className="govuk-heading-s">
                Indicators
                <span className="govuk-hint">
                  Select at least one indicator
                </span>
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
                Create data block
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
          <h2 className="govuk-heading-m">5. View and save data block</h2>
          {['?status=step5'].includes(window.location.search) && (
            <>
              <PrototypeAdminExampleTables task="edit" />
              <Link
                to="/prototypes/publication-create-new-absence-table?status=step5#add-chart"
                className="govuk-button govuk-button--secondary"
              >
                Create chart
              </Link>
              <FormFieldset id="table-config" legend="Data block details">
                <FormGroup>
                  <FormTextInput
                    id="table-title"
                    name="table-title"
                    label="Data block title"
                    defaultValue="'Absence by characteristic' from 'Pupil absence' in England between 2012/13 and 2016/17"
                  />
                </FormGroup>
                <FormGroup>
                  <FormTextInput
                    id="source"
                    name="source"
                    label="Source"
                    defaultValue="DfE prototype example statistics"
                    width={20}
                  />
                </FormGroup>
                <FormGroup>
                  <label htmlFor="footnotes" className="govuk-label">
                    Footnotes
                  </label>
                  <textarea
                    name="footnotes"
                    id="footnotes"
                    className="govuk-textarea"
                  />
                </FormGroup>
              </FormFieldset>

              <hr />

              <FormGroup>
                <p className="govuk-body">
                  Name and save your data block before viewing it under the
                  'View data blocks' tab at the top of this page.
                </p>

                <FormTextInput
                  id="save-table"
                  name="save-table"
                  label="Name data block"
                  value="Absence highlights panel"
                  width={20}
                />
              </FormGroup>

              <Link
                to="/prototypes/publication-create-new-absence-table?status=step1"
                className="govuk-button govuk-!-margin-right-3"
              >
                Save
              </Link>
              <div className="govuk-!-margin-top-6">
                <Link to="/prototypes/publication-create-new-absence-view-table">
                  View saved data blocks
                </Link>
              </div>
            </>
          )}
        </div>
      </div>
      <div className="govuk-grid-row govuk-!-margin-top-9">
        <div className="govuk-grid-column-one-half ">
          <Link to="/prototypes/publication-create-new-absence-data">
            <span className="govuk-heading-m govuk-!-margin-bottom-0">
              Previous step
            </span>
            Manage data
          </Link>
        </div>
        <div className="govuk-grid-column-one-half dfe-align--right">
          <Link to="/prototypes/publication-create-new-absence">
            <span className="govuk-heading-m govuk-!-margin-bottom-0">
              Next step
            </span>
            Manage content
          </Link>
        </div>
      </div>
    </PrototypePage>
  );
};

export default PublicationDataPage;
