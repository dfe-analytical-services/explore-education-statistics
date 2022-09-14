import { FormFieldsetProps } from '@common/components/form/FormFieldset';
import FormTextSearchInput from '@common/components/form/FormTextSearchInput';
import { FormFieldset } from '@common/components/form/index';
import useMounted from '@common/hooks/useMounted';
import formatPretty from '@common/utils/number/formatPretty';
import React, { useState } from 'react';
import FormCheckboxGroup, {
  BaseFormCheckboxGroup,
  FormCheckboxGroupProps,
} from './FormCheckboxGroup';
import styles from './FormCheckboxSearchGroup.module.scss';

export interface FormCheckboxSearchGroupProps extends FormCheckboxGroupProps {
  maxSearchResults?: number;
  searchHelpText?: string;
  searchLabel?: string;
  searchOnly?: boolean;
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
    maxSearchResults = 500,
    name,
    onFieldsetFocus,
    onFieldsetBlur,
    options = [],
    searchHelpText,
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

  const getResultsMessage = () => {
    const numResults = filteredOptions.length;
    if (!searchOnly && numResults === 0) {
      return <p>No options available.</p>;
    }
    if (searchOnly) {
      if (numResults === 0) {
        return (
          <p>
            {searchHelpText || 'Search above and select at least one option.'}
          </p>
        );
      }
      if (numResults > maxSearchResults) {
        return (
          <p>
            {formatPretty(numResults)} results found. Please refine your search
            to view options.
          </p>
        );
      }
    }
    return null;
  };

  const showResults = !searchOnly || filteredOptions.length <= maxSearchResults;

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
        {showResults && filteredOptions.length > 0 && (
          <BaseFormCheckboxGroup
            {...groupProps}
            id={`${id}-options`}
            value={value}
            name={name}
            options={filteredOptions}
            small
          />
        )}
        {getResultsMessage()}
      </div>
    </FormFieldset>
  );
};

export default FormCheckboxSearchGroup;
