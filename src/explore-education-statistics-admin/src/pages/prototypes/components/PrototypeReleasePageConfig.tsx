import React, { useState } from 'react';
import {
  FormGroup,
  FormFieldset,
  FormTextInput,
  FormSelect,
  FormRadioGroup,
  Form,
} from '@common/components/form';
import Link from '../../../components/Link';

interface Props {
  sectionId?: string;
  action?: string;
}

const PrototypePublicationConfig = ({ sectionId }: Props) => {
  const [value, setValue] = useState('academic-year');

  return (
    <>
      {sectionId === 'setup' && (
        <h2 className="govuk-heading-m">Edit release setup</h2>
      )}
      <h3 className="govuk-heading-l">
        Pupil absence statistics and data for schools in England
      </h3>
      <form action="/prototypes/publication-create-new-absence-config">
        <FormGroup>
          <FormRadioGroup
            legend="Time period coverage "
            id="time-period"
            name="time-period"
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
                    <FormGroup>
                      <FormTextInput
                        id="release-period"
                        name="release-period"
                        label="Year starting"
                        value="2018"
                        width={4}
                      />
                    </FormGroup>

                    <span className="govuk-hint">Academic year 2018/19</span>

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
                    <FormFieldset id="calendar year" legend="">
                      <div className="govuk-form-group">
                        <label htmlFor="calendar-year" className="govuk-label">
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
                      <span className="govuk-hint">Financial year 2018/19</span>
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

        <fieldset className="govuk-fieldset">
          <legend className="govuk-fieldset__legend govuk-fieldset__legend--m">
            Schedule publish date
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
                  value="20"
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
                  value="09"
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
                  value="2019"
                />
              </div>
            </div>
          </div>
        </fieldset>
        <fieldset className="govuk-fieldset govuk-!-margin-top-6 govuk-!-margin-bottom-6">
          <legend className="govuk-fieldset__legend govuk-fieldset__legend--m">
            Next release expected
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
                  value="20"
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
                  value="09"
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
                  value="2020"
                />
              </div>
            </div>
          </div>
        </fieldset>

        <FormGroup>
          <FormRadioGroup
            legend="Data type"
            id="data-type"
            name="data-type"
            value={value}
            onChange={event => {
              setValue(event.target.value);
            }}
            options={[
              {
                id: 'data-final',
                label: 'Final',
                value: 'data-final',
              },
              {
                id: 'data-provisional',
                label: 'Provisional',
                value: 'data-provisional',
              },
              {
                id: 'data-revised',
                label: 'Revised',
                value: 'data-revised',
              },
            ]}
          />
        </FormGroup>

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
                id: 'national-stats',
                label: 'National statistics',
                value: 'national-stats',
              },
              {
                id: 'adhoc-stats',
                label: 'Official / adhoc statistics',
                value: 'adhoc-stats',
              },
            ]}
          />
        </FormGroup>

        {!sectionId && (
          <>
            <div className="govuk-form-group govuk-!-margin-top-6">
              <fieldset className="govuk-fieldset">
                <legend className="govuk-fieldset__legend govuk-fieldset__legend--m">
                  Setup
                </legend>
                <div className="govuk-radios">
                  <div className="govuk-radios__item">
                    <input
                      className="govuk-radios__input"
                      type="radio"
                      name="release-setup"
                      id="release-setup-blank"
                      value="create-blank-template"
                    />
                    <label
                      className="govuk-label govuk-radios__label"
                      htmlFor="release-setup-blank"
                    >
                      Create from blank template
                    </label>
                  </div>
                  {/* <div className="govuk-radios__item">
                    <input
                      className="govuk-radios__input"
                      type="radio"
                      name="release-setup"
                      id="release-setup-copy-structure"
                      value="create-copy-structure"
                    />
                    <label
                      className="govuk-label govuk-radios__label"
                      htmlFor="release-setup-copy-structure"
                    >
                      Copy structure of current release (2017 / 2018)
                    </label>
        </div> */}
                  <div className="govuk-radios__item">
                    <input
                      className="govuk-radios__input"
                      type="radio"
                      name="release-setup"
                      id="release-setup-copy-data-structure"
                      value="create-copy-data-structure"
                      defaultChecked
                    />
                    <label
                      className="govuk-label govuk-radios__label"
                      htmlFor="release-setup-copy-data-structure"
                    >
                      Copy structure of current release (2017 / 2018)
                    </label>
                  </div>
                </div>
              </fieldset>
            </div>

            <button type="submit" className="govuk-button">
              Create new release
            </button>
          </>
        )}
        {sectionId === 'setup' && (
          <>
            <button type="submit" className="govuk-button govuk-!-margin-top-6">
              Update release setup
            </button>

            <div className="govuk-!-margin-top-6">
              <Link to="/prototypes/publication-create-new-absence-config">
                Cancel update
              </Link>
            </div>
          </>
        )}
      </form>
    </>
  );
};

export default PrototypePublicationConfig;
