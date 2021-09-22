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
    id,
    hint,
    legend,
    legendHidden,
    legendSize = 'm',
    error,
    name,
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
        selectedOption.indexOf(option.value) > -1
      );
    });
  }

  return (
    <FormFieldset {...fieldsetProps} useFormId={false}>
      {options.length > 1 && (
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
        {filteredOptions.length > 0 ? (
          <BaseFormRadioGroup
            {...groupProps}
            name={name}
            id={id}
            options={filteredOptions}
            onChange={(event, option) => {
              setSelectedOption(event.target.value);
              if (props.onChange) {
                props.onChange(event, option);
              }
            }}
          />
        ) : (
          <p>No results found</p>
        )}
      </div>
    </FormFieldset>
  );
};

export default FormRadioSearchGroup;
