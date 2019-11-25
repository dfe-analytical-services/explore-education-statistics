import React, { useState } from 'react';
import {
  FormGroup,
  FormFieldset,
  FormTextInput,
  FormRadioGroup,
} from '@common/components/form';
import Link from '@admin/components/Link';

interface Props {
  sectionId?: string;
  action?: string;
  title?: string;
}

const PrototypeMethodologyConfig = ({ sectionId, title }: Props) => {
  return (
    <>
      {sectionId === 'setup' && (
        <h2 className="govuk-heading-l">Edit methodology summary</h2>
      )}

      <form action="/prototypes/publication-create-new-methodology-config">
        <FormFieldset id="test" legend="Basic details" legendHidden>
          <FormGroup>
            <FormTextInput
              id="methodology-title"
              name="methodology-title"
              label="Methodology title"
              value={title}
              percentageWidth="three-quarters"
            />
          </FormGroup>
        </FormFieldset>

        <fieldset className="govuk-fieldset govuk-!-margin-top-9 govuk-!-margin-bottom-9">
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
          <button type="submit" className="govuk-button">
            Create new methodology
          </button>
        )}
        {sectionId === 'setup' && (
          <>
            <button type="submit" className="govuk-button govuk-!-margin-top-6">
              Update methodology summary
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

export default PrototypeMethodologyConfig;
