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
    value = [],
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

  let filteredOptions = options;

  if (searchTerm) {
    filteredOptions = options.filter(
      option =>
        option.label.toLowerCase().includes(searchTerm.toLowerCase()) ||
        value.indexOf(option.value) > -1,
    );
  }

  return (
    <FormFieldset
      {...fieldsetProps}
      onBlur={onFieldsetBlur}
      onFocus={onFieldsetFocus}
    >
      <FormTextSearchInput
        id={`${id}-search`}
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

      <div aria-live="assertive" className={styles.optionsContainer}>
        <BaseFormCheckboxGroup
          {...groupProps}
          id={`${id}-checkboxes`}
          value={value}
          name={name}
          options={filteredOptions}
          small
        />
      </div>
    </FormFieldset>
  );
};

export default FormCheckboxSearchGroup;
