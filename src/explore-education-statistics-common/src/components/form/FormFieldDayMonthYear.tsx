import FormFieldset from '@common/components/form/FormFieldset';
import FormGroup from '@common/components/form/FormGroup';
import React from 'react';

interface Props {
  fieldsetId: string;
  fieldsetLegend: string;
  fieldIdPrefix: string;
  day: string;
  month: string;
  year: string;
}

const FormFieldDayMonthYear = ({
  fieldsetId,
  fieldsetLegend,
  fieldIdPrefix,
  day,
  month,
  year,
}: Props) => {
  return (
    <FormFieldset id={fieldsetId} legend={fieldsetLegend}>
      <FormGroup>
        <div className="govuk-date-input__item">
          <div className="govuk-form-group">
            <label
              htmlFor="financial-year-day"
              className="govuk-label govuk-date-input__label"
            >
              Day
            </label>
            <input
              id={`${fieldIdPrefix}Day`}
              type="number"
              pattern="[0-9]*"
              name="financial-year-day"
              className="govuk-input govuk-date-input__input govuk-input--width-2"
              value={day}
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
              id={`${fieldIdPrefix}Month`}
              type="number"
              pattern="[0-9]*"
              name="financial-year-month"
              className="govuk-input govuk-date-input__input govuk-input--width-2"
              value={month}
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
              id={`${fieldIdPrefix}Year`}
              type="number"
              pattern="[0-9]*"
              name="financial-year-year"
              className="govuk-input govuk-date-input__input govuk-input--width-4"
              value={year}
            />
          </div>
        </div>
      </FormGroup>
    </FormFieldset>
  );
};

export default FormFieldDayMonthYear;
