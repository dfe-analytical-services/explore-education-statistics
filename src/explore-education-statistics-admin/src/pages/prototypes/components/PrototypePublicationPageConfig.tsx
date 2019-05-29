import React, { useState } from 'react';
import {
  FormGroup,
  FormFieldset,
  FormTextInput,
  FormSelect,
  FormRadioGroup,
} from '@common/components/form';
import Link from '../../../components/Link';

interface Props {
  sectionId?: string;
  action?: string;
}

const PrototypePublicationConfig = ({ sectionId }: Props) => {
  const [value, setValue] = useState('academic-year');

  const formAction = `publication-${sectionId}`;

  return (
    <>
      {sectionId === 'confirmPublication' && (
        <h2 className="govuk-heading-m">Confirm publication details</h2>
      )}
      {sectionId === 'editPublication' && (
        <h2 className="govuk-heading-m">Edit publication details</h2>
      )}

      <form method="get">
        {sectionId !== 'confirmPublication' && (
          <>
            <div className="govuk-form-group">
              <label htmlFor="title" className="govuk-label govuk-label--s">
                Publication title
              </label>
              <input
                className="govuk-input"
                id="title"
                name="title"
                type="text"
                defaultValue="Pupil absence statistics and data for schools in England: summer term"
              />
            </div>

            <FormGroup>
              <FormRadioGroup
                legend="Release type"
                id="release-type"
                name="release-type"
                value={value}
                onChange={event => {
                  setValue(event.target.value);
                }}
                options={[
                  {
                    id: 'release-type-academic',
                    label: 'Academic Year',
                    value: 'academic-year',
                    conditional: (
                      <FormFieldset id="test" legend="">
                        <legend className="govuk-fieldset__legend govuk-fieldset__legend--s">
                          Release period
                        </legend>

                        <FormGroup>
                          <FormTextInput
                            id="release-period"
                            name="release-period"
                            label="Year starting"
                            value="2018"
                            width={4}
                          />
                        </FormGroup>

                        <span className="govuk-hint">
                          Academic year 2018/19
                        </span>

                        <FormGroup>
                          <FormSelect
                            id="time-period"
                            label="Select time period"
                            name="time-period"
                            options={[
                              {
                                label: 'Full academic year',
                                value: 'full-year',
                              },
                              { label: 'Autumn term', value: 'autumn-term' },
                              { label: 'Spring term', value: 'spring-term' },
                              { label: 'Summer term', value: 'summer-term' },
                            ]}
                          />
                        </FormGroup>

                        <FormGroup>
                          <legend className="govuk-fieldset__legend govuk-fieldset__legend--s">
                            Terms per year
                          </legend>

                          <FormRadioGroup
                            legend=""
                            name="terms-per-year"
                            id="terms-per-year"
                            value="terms-6"
                            options={[
                              {
                                id: 'terms-6',
                                label: '6 terms',
                                value: 'terms-6',
                              },
                              {
                                id: 'terms-5',
                                label: '5 terms',
                                value: 'terms-5',
                              },
                            ]}
                          />
                        </FormGroup>
                      </FormFieldset>
                    ),
                  },
                  {
                    id: 'release-type-calendar',
                    label: 'Calendar year',
                    value: 'calendar-year',
                    conditional: (
                      <FormGroup>
                        <FormFieldset
                          id="calendar year"
                          legend="Release period"
                        >
                          <div className="govuk-form-group">
                            <label
                              htmlFor="calendar-year"
                              className="govuk-label"
                            >
                              Year
                            </label>
                            <input
                              className="govuk-input govuk-input--width-4"
                              id="calendar-year"
                              name="calendar-year"
                              type="text"
                              defaultValue="2019"
                              pattern="[0-9]*"
                            />
                          </div>
                        </FormFieldset>
                      </FormGroup>
                    ),
                  },
                  {
                    id: 'release-type-financial',
                    label: 'Financial year',
                    value: 'financial-year',
                    conditional: (
                      <FormGroup>
                        <FormFieldset
                          id="financial year"
                          legend="Financial year start date"
                        >
                          <div className="govuk-form-group">
                            <fieldset className="govuk-fieldset">
                              <div
                                className="govuk-date-input"
                                id="financial-year-start-date"
                              >
                                <div className="govuk-date-input__item">
                                  <div className="govuk-form-group">
                                    <label
                                      htmlFor="financial-year-day"
                                      className="govuk-label govuk-date-input__label"
                                    >
                                      Day
                                    </label>
                                    <input
                                      type="number"
                                      pattern="[0-9]*"
                                      name="financial-year-day"
                                      id="financial-year-day"
                                      className="govuk-input govuk-date-input__input govuk-input--width-2"
                                      defaultValue="05"
                                    />
                                  </div>
                                </div>
                                <div className="govuk-date-input__item">
                                  <div className="govuk-form-group">
                                    <label
                                      htmlFor="financial-year-month"
                                      className="govuk-label govuk-date-input__label"
                                    >
                                      Month
                                    </label>
                                    <input
                                      type="number"
                                      pattern="[0-9]*"
                                      name="financial-year-month"
                                      id="financial-year-month"
                                      className="govuk-input govuk-date-input__input govuk-input--width-2"
                                      defaultValue="04"
                                    />
                                  </div>
                                </div>
                                <div className="govuk-date-input__item">
                                  <div className="govuk-form-group">
                                    <label
                                      htmlFor="financial-year-year"
                                      className="govuk-label govuk-date-input__label"
                                    >
                                      Year
                                    </label>
                                    <input
                                      type="number"
                                      pattern="[0-9]*"
                                      name="financial-year-year"
                                      id="financial-year-year"
                                      className="govuk-input govuk-date-input__input govuk-input--width-4"
                                      defaultValue="2019"
                                    />
                                  </div>
                                </div>
                              </div>
                            </fieldset>
                          </div>
                          <span className="govuk-hint">
                            Financial year 2018/19
                          </span>
                          <div className="govuk-form-group">
                            <label
                              htmlFor="time-financial-period"
                              className="govuk-label"
                            >
                              Select time period
                            </label>
                            <select
                              name="time-financial-period"
                              id="time--financial-period"
                              className="govuk-select"
                            >
                              <option value="full-financial-year">
                                Full financial year
                              </option>
                              <option value="q1">Q1</option>
                              <option value="q2">Q2</option>
                              <option value="q3">Q3</option>
                              <option value="q4">Q4</option>
                              <option value="q1-q2">Q1 to Q2</option>
                              <option value="q1-q3">Q1 to Q3</option>
                              <option value="q2-q3">Q2 to Q3</option>
                              <option value="q2-q3">Q2 to Q4</option>
                            </select>
                          </div>
                        </FormFieldset>
                      </FormGroup>
                    ),
                  },
                  {
                    id: 'release-type-month',
                    label: 'Month',
                    value: 'month',
                    conditional: (
                      <FormGroup>
                        <FormFieldset id="month" legend="">
                          <div className="govuk-form-group">
                            <fieldset className="govuk-fieldset">
                              <legend className="govuk-fieldset__legend govuk-fieldset__legend--s">
                                Monthly release date
                              </legend>
                              <div
                                className="govuk-date-input"
                                id="monthly-release-start-date"
                              >
                                <div className="govuk-date-input__item">
                                  <div className="govuk-form-group">
                                    <label
                                      htmlFor="monthly-release-month"
                                      className="govuk-label govuk-date-input__label"
                                    >
                                      Month
                                    </label>
                                    <input
                                      type="number"
                                      pattern="[0-9]*"
                                      name="monthly-release-month"
                                      id="monthly-release-month"
                                      className="govuk-input govuk-date-input__input govuk-input--width-2"
                                      defaultValue="04"
                                    />
                                  </div>
                                </div>
                                <div className="govuk-date-input__item">
                                  <div className="govuk-form-group">
                                    <label
                                      htmlFor="monthly-release-year"
                                      className="govuk-label govuk-date-input__label"
                                    >
                                      Year
                                    </label>
                                    <input
                                      type="number"
                                      pattern="[0-9]*"
                                      name="monthly-release-year"
                                      id="monthly-release-year"
                                      className="govuk-input govuk-date-input__input govuk-input--width-4"
                                      defaultValue="2019"
                                    />
                                  </div>
                                </div>
                              </div>
                            </fieldset>
                          </div>
                        </FormFieldset>
                      </FormGroup>
                    ),
                  },
                ]}
              />
            </FormGroup>
            <FormGroup>
              <FormFieldset id="lead-statisician" legend="Lead statistician">
                <FormSelect
                  id="select-lead-statisician"
                  label="Select lead statistician"
                  name="select-lead-statisician"
                  options={[
                    { label: 'Mark Pearson', value: 'mark-pearson' },
                    { label: 'Alex Miller', value: 'alex-miller' },
                  ]}
                />
              </FormFieldset>
            </FormGroup>
            <fieldset className="govuk-fieldset">
              <legend className="govuk-fieldset__legend govuk-fieldset__legend--m">
                Next scheduled release publication date
              </legend>
              <div className="govuk-date-input" id="schedule-publish-date">
                <div className="govuk-date-input__item">
                  <div className="govuk-form-group">
                    <label
                      htmlFor="schedule-day"
                      className="govuk-label govuk-date-input__label"
                    >
                      Day
                    </label>
                    <input
                      className="govuk-input govuk-date-input__inout govuk-input--width-2"
                      id="schedule-day"
                      name="schedule-day"
                      type="number"
                      pattern="[0-9]*"
                    />
                  </div>
                </div>
                <div className="govuk-date-input__item">
                  <div className="govuk-form-group">
                    <label
                      htmlFor="schedule-month"
                      className="govuk-label govuk-date-input__label"
                    >
                      Month
                    </label>
                    <input
                      className="govuk-input govuk-date-input__inout govuk-input--width-2"
                      id="schedule-month"
                      name="schedule-month"
                      type="number"
                      pattern="[0-9]*"
                    />
                  </div>
                </div>
                <div className="govuk-date-input__item">
                  <div className="govuk-form-group">
                    <label
                      htmlFor="schedule-year"
                      className="govuk-label govuk-date-input__label"
                    >
                      Year
                    </label>
                    <input
                      className="govuk-input govuk-date-input__inout govuk-input--width-4"
                      id="schedule-year"
                      name="schedule-year"
                      type="number"
                      pattern="[0-9]*"
                    />
                  </div>
                </div>
              </div>
            </fieldset>
          </>
        )}
        {sectionId === 'confirmPublication' && (
          <>
            <dl className="govuk-summary-list">
              <div className="govuk-summary-list__row">
                <dt className="govuk-summary-list__key">Publication title</dt>
                <dd className="govuk-summary-list__value">
                  Pupil absence statistics and data for schools in England:
                  summer term
                </dd>
              </div>
              <div className="govuk-summary-list__row">
                <dt className="govuk-summary-list__key">Release type</dt>
                <dd className="govuk-summary-list__value">Academic year</dd>
              </div>
              <div className="govuk-summary-list__row">
                <dt className="govuk-summary-list__key">Next release period</dt>
                <dd className="govuk-summary-list__value">2018 to 2019</dd>
              </div>
              <div className="govuk-summary-list__row">
                <dt className="govuk-summary-list__key">Lead statistician</dt>
                <dd className="govuk-summary-list__value">Alex Miller</dd>
              </div>
              <div className="govuk-summary-list__row">
                <dt className="govuk-summary-list__key">
                  Next scheduled publication date
                </dt>
                <dd className="govuk-summary-list__value">20 September 2019</dd>
              </div>
            </dl>
            <Link to="/prototypes/publication-edit-new">
              Edit publication details
            </Link>
          </>
        )}
        {sectionId === 'newPublication' && (
          <div className="govuk-!-margin-top-6">
            <input type="hidden" name="status" value="confirmPublication" />

            <button
              type="submit"
              className="govuk-button"
              formAction="publication-confirm-new"
            >
              Create new publication
            </button>

            <div className="govuk-!-margin-top-6">
              <Link to="/prototypes/admin-dashboard">Cancel update</Link>
            </div>
          </div>
        )}
        {sectionId === 'editPublication' && (
          <div className="govuk-!-margin-top-6">
            <input type="hidden" name="status" value="confirmPublication" />
            <button
              type="submit"
              className="govuk-button"
              formAction="publication-confirm-new"
            >
              Edit publication
            </button>

            <div className="govuk-!-margin-top-6">
              <Link to="/prototypes/admin-dashboard">Cancel update</Link>
            </div>
          </div>
        )}
        {sectionId === 'confirmPublication' && (
          <div className="govuk-!-margin-top-6">
            <input type="hidden" name="status" value="newPublication" />
            <button
              type="submit"
              className="govuk-button"
              formAction="admin-dashboard"
            >
              Confirm and create new publication
            </button>

            <div className="govuk-!-margin-top-6">
              <Link to="/prototypes/admin-dashboard">Cancel publication</Link>
            </div>
          </div>
        )}
      </form>
    </>
  );
};

export default PrototypePublicationConfig;
