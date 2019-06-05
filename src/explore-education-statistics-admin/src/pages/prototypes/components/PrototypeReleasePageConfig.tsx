import React from 'react';
import {
  FormGroup,
  FormFieldset,
  FormTextInput,
  FormSelect,
  // FormRadioGroup,
  // Form,
} from '@common/components/form';
import Link from '../../../components/Link';

interface Props {
  sectionId?: string;
  action?: string;
}

const PrototypePublicationConfig = ({ sectionId }: Props) => {
  // const [value, setValue] = useState('academic-year');

  return (
    <>
      {sectionId === 'setup' && (
        <h2 className="govuk-heading-m">Edit release setup</h2>
      )}
      <h3 className="govuk-heading-l">
        Pupil absence statistics and data for schools in England
      </h3>
      <form action="/prototypes/publication-create-new-absence-config">
        <FormFieldset id="test" legend="Academic year">
          <FormGroup>
            <FormTextInput
              id="release-period"
              name="release-period"
              label="Year starting"
              value="2018"
              width={4}
            />
            <span className="govuk-hint govuk-!-margin-top-1">
              Academic year 2018 to 2019
            </span>
          </FormGroup>

          <FormGroup>
            <FormSelect
              id="time-period"
              label="Select time period"
              name="time-period"
              options={[
                { label: 'Full academic year', value: 'full-year' },
                { label: 'Autumn term', value: 'autumn-term' },
                { label: 'Spring term', value: 'spring-term' },
                { label: 'Summer term', value: 'summer-term' },
              ]}
            />
          </FormGroup>
        </FormFieldset>

        <FormGroup>
          <FormFieldset id="lead" legend="Statistician">
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
