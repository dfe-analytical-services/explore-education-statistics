import { FormFieldsetProps } from '@common/components/form/FormFieldset';
import FormTextSearchInput from '@common/components/form/FormTextSearchInput';
import { FormFieldset } from '@common/components/form/index';
import useMounted from '@common/hooks/useMounted';
import React, { useState } from 'react';
import FormCheckboxGroup, {
  BaseFormCheckboxGroup,
  FormCheckboxGroupProps,
} from './FormCheckboxGroup';
import styles from './FormCheckboxSearchGroup.module.scss';

export interface FormCheckboxSearchGroupProps extends FormCheckboxGroupProps {
  searchLabel?: string;
}

const FormCheckboxSearchGroup = ({
  searchLabel = 'Search options',
  ...props
}: FormCheckboxSearchGroupProps) => {
  const { isMounted } = useMounted();

  const [searchTerm, setSearchTerm] = useState('');

  if (!isMounted) {
    return <FormCheckboxGroup {...props} />;
  }

  const {
    id,
    legend,
    hint,
    legendHidden,
    legendSize,
    error,
    name,
    onFieldsetFocus,
    onFieldsetBlur,
    options = [],
    searchOnly = false,
    value = [],
    ...groupProps
  } = props;

  const minSearchCharacters = searchOnly ? 3 : 0;

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

  let filteredOptions = searchOnly
    ? options.filter(option => value.includes(option.value))
    : options;

  if (searchTerm) {
    // Search for URN in the hint field if search term is a number.
    if (searchOnly && !Number.isNaN(Number(searchTerm))) {
      filteredOptions = options.filter(
        option =>
          option.hint?.toString().includes(searchTerm) ||
          value.includes(option.value),
      );
    } else {
      filteredOptions = options.filter(
        option =>
          option.label
            .toLowerCase()
            .includes(searchTerm.trim().toLowerCase()) ||
          value.includes(option.value),
      );
    }
  }

  return (
    <FormFieldset {...fieldsetProps} useFormId={false}>
      <FormTextSearchInput
        id={`${id}-search`}
        name={`${name}-search`}
        label={searchLabel}
        width={20}
        onChange={event => {
          if (
            event.target.value.length >= minSearchCharacters ||
            !event.target.value.length
          ) {
            setSearchTerm(event.target.value);
          }
        }}
        onKeyPress={event => {
          if (event.key === 'Enter') {
            event.preventDefault();
          }
        }}
      />

      <div className={styles.optionsContainer}>
        {filteredOptions.length > 0 && (
          <span
            aria-live="polite"
            aria-atomic
            className="govuk-visually-hidden"
          >
            {`${filteredOptions.length} option${
              filteredOptions.length > 1 ? 's' : ''
            } found`}
          </span>
        )}
        <BaseFormCheckboxGroup
          {...groupProps}
          id={`${id}-options`}
          value={value}
          name={name}
          options={filteredOptions}
          searchOnly={searchOnly}
          small
        />
      </div>
    </FormFieldset>
  );
};

export default FormCheckboxSearchGroup;
