import { FormFieldsetProps } from '@common/components/form/FormFieldset';
import FormTextSearchInput from '@common/components/form/FormTextSearchInput';
import { FormFieldset } from '@common/components/form/index';
import useMounted from '@common/hooks/useMounted';
import FormRadioGroup, {
  BaseFormRadioGroup,
  FormRadioGroupProps,
} from '@common/components/form/FormRadioGroup';
import React, { useState } from 'react';

export interface FormRadioSearchGroupProps extends FormRadioGroupProps {
  alwaysShowOptions?: string[];
  searchLabel?: string;
}

const FormRadioSearchGroup = ({
  searchLabel = 'Search',
  ...props
}: FormRadioSearchGroupProps) => {
  const { isMounted } = useMounted();

  const [searchTerm, setSearchTerm] = useState('');
  const [selectedOption, setSelectedOption] = useState('');

  const {
    alwaysShowOptions = [],
    id,
    hint,
    legend,
    legendHidden,
    legendSize = 'm',
    error,
    name,
    onChange,
    onFieldsetFocus,
    onFieldsetBlur,
    options = [],
    ...groupProps
  } = props;

  const fieldsetProps: FormFieldsetProps = {
    id,
    legend,
    legendHidden,
    legendSize,
    hint,
    error,
    onFocus: onFieldsetFocus,
    onBlur: onFieldsetBlur,
  };

  if (!isMounted) {
    return (
      <FormRadioGroup
        {...props}
        name={name}
        legend={legend}
        id={id}
        options={options}
      />
    );
  }

  let filteredOptions = options;

  if (searchTerm) {
    filteredOptions = options.filter(option => {
      return (
        option.label.toLowerCase().includes(searchTerm.toLowerCase()) ||
        selectedOption.indexOf(option.value) > -1 ||
        alwaysShowOptions.includes(option.value)
      );
    });
  }
  const noResults = alwaysShowOptions.length
    ? filteredOptions.length === alwaysShowOptions.length
    : !filteredOptions.length;

  const showSearch = alwaysShowOptions.length
    ? options.length > 1 + alwaysShowOptions.length
    : options.length > 1;

  return (
    <FormFieldset {...fieldsetProps} useFormId={false}>
      {showSearch && (
        <FormTextSearchInput
          id={`${id}-search`}
          className="govuk-!-margin-bottom-4"
          name={`${name}-search`}
          label={searchLabel}
          width={20}
          onChange={event => setSearchTerm(event.target.value)}
          onKeyPress={event => {
            if (event.key === 'Enter') {
              event.preventDefault();
            }
          }}
        />
      )}

      <div aria-live="assertive">
        {(alwaysShowOptions.length > 0 || !noResults) && (
          <BaseFormRadioGroup
            {...groupProps}
            name={name}
            id={id}
            options={filteredOptions}
            onChange={(event, option) => {
              setSelectedOption(event.target.value);
              onChange?.(event, option);
            }}
          />
        )}

        {noResults && <p>No results found</p>}
      </div>
    </FormFieldset>
  );
};

export default FormRadioSearchGroup;
