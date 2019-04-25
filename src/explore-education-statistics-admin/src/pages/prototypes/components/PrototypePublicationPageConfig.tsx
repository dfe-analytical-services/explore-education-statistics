import React from 'react';
import Link from '../../../components/Link';

interface Props {
  sectionId?: string;
  action?: string;
}

const PrototypePublicationConfig = ({ sectionId }: Props) => {
  return (
    <>
      {sectionId === 'setup' && (
        <h2 className="govuk-heading-m">Edit release setup</h2>
      )}
      <form action="/prototypes/publication-create-new-absence-config">
        <div className="govuk-form-group">
          <label htmlFor="title" className="govuk-label govuk-label--s">
            Publication title
          </label>
          <input
            className="govuk-input"
            id="title"
            name="title"
            type="text"
            value="Pupil absence statistics and data for schools in England"
          />
        </div>
        <div className="govuk-form-group">
          <fieldset className="govuk-fieldset">
            <legend className="govuk-fieldset__legend govuk-fieldset__legend--s">
              Release type
            </legend>
            <div className="govuk-radios govuk-radios--conditional">
              <div className="govuk-radios__item">
                <input
                  className="govuk-radios__input"
                  type="radio"
                  name="release-type"
                  id="release-type-academic"
                  value="academic-year"
                  defaultChecked
                />
                <label
                  className="govuk-label govuk-radios__label"
                  htmlFor="release-type-academic"
                  data-aria-controls="conditional-academic-year"
                >
                  Academic year
                </label>
              </div>
              <div
                className="govuk-radios__conditional govuk-radios__conditional--visible"
                id="conditional-academic-year"
              >
                <div className="govuk-form-group">
                  <fieldset className="govuk-fieldset">
                    <legend className="govuk-fieldset__legend govuk-fieldset__legend--s">
                      Release period
                    </legend>
                    <div className="govuk-form-group">
                      <label htmlFor="release-period" className="govuk-label">
                        Year starting
                      </label>
                      <input
                        className="govuk-input govuk-input--width-4"
                        id="release-period"
                        name="release-period"
                        type="text"
                        value="2018"
                      />
                    </div>
                    <span className="govuk-hint">Academic year 2018/19</span>
                    <div className="govuk-form-group">
                      <label htmlFor="time-period" className="govuk-label">
                        Select time period
                      </label>
                      <select
                        name="time-period"
                        id="time-period"
                        className="govuk-select"
                      >
                        <option value="full-year">Full academic year</option>
                        <option value="autumn-term">Autumn term</option>
                        <option value="spring-term">Spring term</option>
                        <option value="summer-term">Summer term</option>
                        <option value="autumn-spring-term">
                          Autumn and spring term
                        </option>
                        <option value="autumn-spring-term">
                          Spring and summer term
                        </option>
                      </select>
                    </div>
                    <div className="govuk-form-group">
                      <fieldset className="govuk-fieldset">
                        <legend className="govuk-fieldset__legend govuk-fieldset__legend--s">
                          Terms per year
                        </legend>
                      </fieldset>
                      <div className="govuk-radios">
                        <div className="govuk-radios__item">
                          <input
                            type="radio"
                            name="terms-per-year"
                            id="terms-6"
                            value="6"
                            className="govuk-radios__input"
                            defaultChecked
                          />
                          <label
                            htmlFor="terms-6"
                            className="govuk-label govuk-radios__label"
                          >
                            6 terms
                          </label>
                        </div>
                        <div className="govuk-radios__item">
                          <input
                            type="radio"
                            name="terms-per-year"
                            id="terms-5"
                            value="5"
                            className="govuk-radios__input"
                          />
                          <label
                            htmlFor="terms-6"
                            className="govuk-label govuk-radios__label"
                          >
                            5 terms
                          </label>
                        </div>
                      </div>
                    </div>
                  </fieldset>
                </div>
              </div>
              <div className="govuk-radios__item">
                <input
                  className="govuk-radios__input"
                  type="radio"
                  name="release-type"
                  id="release-type-calendar-year"
                  value="calendar-year"
                />
                <label
                  className="govuk-label govuk-radios__label"
                  htmlFor="release-type-calendar-year"
                >
                  Calendar year
                </label>
              </div>
              <div
                className="govuk-radios__conditional govuk-radios__conditional--visible"
                id="conditional-calendar-year"
              >
                <div className="govuk-form-group">
                  <fieldset className="govuk-fieldset">
                    <legend className="govuk-fieldset__legend govuk-fieldset__legend--s">
                      Release period
                    </legend>
                    <div className="govuk-form-group">
                      <label htmlFor="calendar-year" className="govuk-label">
                        Year
                      </label>
                      <input
                        className="govuk-input govuk-input--width-4"
                        id="calendar-year"
                        name="calendar-year"
                        type="text"
                        value="2019"
                        pattern="[0-9]*"
                      />
                    </div>
                  </fieldset>
                </div>
              </div>
              <div className="govuk-radios__item">
                <input
                  className="govuk-radios__input"
                  type="radio"
                  name="release-type"
                  id="release-type-financial"
                  value="financial year"
                />
                <label
                  className="govuk-label govuk-radios__label"
                  htmlFor="release-type-financial"
                >
                  Finacial year
                </label>
              </div>
              <div
                className="govuk-radios__conditional govuk-radios__conditional--visible"
                id="conditional-financial-year"
              >
                <div className="govuk-form-group">
                  <fieldset className="govuk-fieldset">
                    <div className="govuk-form-group">
                      <fieldset className="govuk-fieldset">
                        <legend className="govuk-fieldset__legend govuk-fieldset__legend--s">
                          Financial year start date
                        </legend>
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
                                value="05"
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
                                value="04"
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
                                value="2019"
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
                  </fieldset>
                </div>
              </div>
              <div className="govuk-radios__item">
                <input
                  className="govuk-radios__input"
                  type="radio"
                  name="release-type"
                  id="release-month"
                  value="month"
                />
                <label
                  className="govuk-label govuk-radios__label"
                  htmlFor="release-type-month"
                >
                  Month
                </label>
              </div>
              <div
                className="govuk-radios__conditional govuk-radios__conditional--visible"
                id="conditional-month"
              >
                <div className="govuk-form-group">
                  <fieldset className="govuk-fieldset">
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
                                value="04"
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
                                value="2019"
                              />
                            </div>
                          </div>
                        </div>
                      </fieldset>
                    </div>
                  </fieldset>
                </div>
              </div>
            </div>
          </fieldset>
        </div>

        {!sectionId && (
          <>
            <div className="govuk-form-group">
              <fieldset className="govuk-fieldset">
                <legend className="govuk-fieldset__legend govuk-fieldset__legend--s">
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
                      Copy structure, content and data of current release (2017
                      / 2018)
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
            <button type="submit" className="govuk-button">
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
