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

  return (
    <>
      {sectionId === 'setup' && (
        <h2 className="govuk-heading-l">Edit release summary</h2>
      )}
      <form action="/prototypes/publication-create-new-absence-config">
        <FormFieldset id="test" legend="Select release period">
          <FormGroup>
            <label htmlFor="time-period" className="govuk-label">
              Type
            </label>
            <select
              name="time-period"
              id="time-period"
              className="govuk-select"
            >
              <optgroup label="Academic year">
                <option value="AY">Academic year - AY</option>
                <option value="AYQ1">Academic year Q1 - AYQ1</option>
                <option value="AYQ1Q2">Academic year Q1 to Q2 - AYQ1Q2</option>
                <option value="AYQ1Q3">Academic year Q1 to Q3 - AYQ1Q3</option>
                <option value="AYQ1Q4">Academic year Q1 to Q4 - AYQ1Q4</option>
                <option value="AYQ2">Academic year Q2 - AYQ2</option>
                <option value="AYQ2Q3">Academic year Q2 to Q3 - AYQ2Q3</option>
                <option value="AYQ2Q4">Academic year Q2 to Q4 - AYQ2Q4</option>
                <option value="AYQ3">Academic year Q3 - AYQ3</option>
                <option value="AYQ3Q4">Academic year Q3 to Q3 - AYQ3Q4</option>
                <option value="AYQ4">Academic year Q4 - AYQ4</option>
              </optgroup>
              <optgroup label="Calendar year">
                <option value="CY">Calendar year - CY</option>
                <option value="CYQ1">Calendar year Q1 - CYQ1</option>
                <option value="CYQ1Q2">Calendar year Q1 to Q2 - CYQ1Q2</option>
                <option value="CYQ1Q3">Calendar year Q1 to Q3 - CYQ1Q3</option>
                <option value="CYQ1Q4">Calendar year Q1 to Q4 - CYQ1Q4</option>
                <option value="CYQ2">Calendar year Q2 - CYQ2</option>
                <option value="CYQ2Q3">Calendar year Q2 to Q3 - CYQ2Q3</option>
                <option value="CYQ2Q4">Calendar year Q2 to Q4 - CYQ1Q4</option>
                <option value="CYQ3">Calendar year Q3 - CYQ3</option>
                <option value="CYQ3Q4">Calendar year Q3 to Q4 - CYQ3Q4</option>
                <option value="CYQ4">Calendar year Q4 - CYQ4</option>
              </optgroup>
              <optgroup label="Financial year">
                <option value="FY">Financial year - FY</option>
                <option value="FYQ1">Financial year Q1 - FYQ1</option>
                <option value="FYQ1Q2">Financial year Q1 to Q2 - FYQ1Q2</option>
                <option value="FYQ1Q3">Financial year Q1 to Q3 - FYQ1Q3</option>
                <option value="FYQ1Q4">Financial year Q1 to Q4 - FYQ1Q4</option>
                <option value="FYQ2">Financial year Q2 - FYQ2</option>
                <option value="FYQ2Q3">Financial year Q2 to Q3 - FYQ2Q3</option>
                <option value="FYQ2Q4">Financial year Q2 to Q4 - FYQ1Q4</option>
                <option value="FYQ3">Financial year Q3 - FYQ3</option>
                <option value="FYQ3Q4">Financial year Q3 to Q4 - FYQ3Q4</option>
                <option value="FYQ4">Financial year Q4 - FYQ4</option>
              </optgroup>
              <optgroup label="Tax year">
                <option value="TY">Tax year - TY</option>
                <option value="TYQ1">Tax year Q1 - TYQ1</option>
                <option value="TYQ1Q2">Tax year Q1 to Q2 - TYQ1Q2</option>
                <option value="TYQ1Q3">Tax year Q1 to Q3 - TYQ1Q3</option>
                <option value="TYQ1Q4">Tax year Q1 to Q4 - TYQ1Q4</option>
                <option value="TYQ2">Tax year Q2 - TYQ2</option>
                <option value="TYQ2Q3">Tax year Q2 to Q3 - TYQ2Q3</option>
                <option value="TYQ2Q4">Tax year Q2 to Q4 - TYQ1Q4</option>
                <option value="TYQ3">Tax year Q3 - TYQ3</option>
                <option value="TYQ3Q4">Tax year Q3 to Q4 - TYQ3Q4</option>
                <option value="TYQ4">Tax year Q4 - TYQ4</option>
              </optgroup>
              <optgroup label="Term">
                <option value="T1">Autumn term - T1</option>
                <option value="T1T2">Autumn and spring term - T1T2</option>
                <option value="T2">Spring term - T2</option>
                <option value="T2">Summer term - T3</option>
              </optgroup>
              <optgroup label="Month">
                <option value="M1">January - M1</option>
                <option value="M2">February - M2</option>
                <option value="M3">March - M3</option>
                <option value="M4">April - M4</option>
                <option value="M5">May - M5</option>
                <option value="M6">June - M6</option>
                <option value="M7">July - M7</option>
                <option value="M8">August - M8</option>
                <option value="M9">September - M9</option>
                <option value="M10">October - M10</option>
                <option value="M11">November - M11</option>
                <option value="M12">December - M12</option>
              </optgroup>
              <optgroup label="Other">
                <option value="EOM">Up until 31st March - EOM</option>
              </optgroup>
            </select>
          </FormGroup>
          <FormGroup>
            <FormTextInput
              id="release-year"
              name="release-year"
              label="Year"
              value="2018"
              width={4}
            />
          </FormGroup>
          {/* <FormGroup>
            <fieldset className="govuk-fieldset">
              <legend className="govuk-heading-s govuk-!-margin-bottom-3">
                Financial year start
              </legend>
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
            </fieldset>
          </FormGroup> */}
        </FormFieldset>

        <fieldset className="govuk-fieldset govuk-!-margin-top-9">
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
        <fieldset className="govuk-fieldset govuk-!-margin-top-9 govuk-!-margin-bottom-9">
          <legend className="govuk-fieldset__legend govuk-fieldset__legend--m">
            Next release date (optional)
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
                label: 'National Statistics',
                value: 'national-stats',
              },
              {
                id: 'adhoc-stats',
                label: 'Official / ad hoc statistics',
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
                      Create new template
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
                      Copy existing template (2017 / 2018)
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
              Update release summary
            </button>

            <div className="govuk-!-margin-top-6">
              <Link to="/prototypes/publication-create-new-absence-config">
                Cancel
              </Link>
            </div>
          </>
        )}
      </form>
    </>
  );
};

export default PrototypePublicationConfig;
