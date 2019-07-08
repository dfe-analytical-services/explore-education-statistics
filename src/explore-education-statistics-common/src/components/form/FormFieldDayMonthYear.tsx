import FormFieldset from '@common/components/form/FormFieldset';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import React from 'react';

interface DayMonthYearProps {
  day: string;
  month: string;
  year: string;
}

interface Props extends DayMonthYearProps {
  fieldsetLegend: string;
  fieldName: string;
}

const FormFieldDayMonthYear = ({ fieldsetLegend, fieldName }: Props) => {
  return (
    <FormFieldset id={`${fieldName}Fieldset`} legend={fieldsetLegend}>
      <FormFieldTextInput<DayMonthYearProps>
        id={`${fieldName}.day`}
        name={`${fieldName}.day`}
        label="Day"
        type="number"
        pattern="[0-9]*"
        width={2}
        formGroupClass="govuk-date-input__item"
      />
      <FormFieldTextInput<DayMonthYearProps>
        id={`${fieldName}.month`}
        name={`${fieldName}.month`}
        type="number"
        pattern="[0-9]*"
        label="Month"
        width={2}
        formGroupClass="govuk-date-input__item"
      />
      <FormFieldTextInput<DayMonthYearProps>
        id={`${fieldName}.year`}
        name={`${fieldName}.year`}
        type="number"
        pattern="[0-9]*"
        label="Year"
        width={4}
        formGroupClass="govuk-date-input__item"
      />
    </FormFieldset>
  );
};

export default FormFieldDayMonthYear;
