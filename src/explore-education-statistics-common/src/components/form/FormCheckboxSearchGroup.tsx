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
  legend,
  legendSize,
  legendHidden,
  hint,
  error,
  ...props
}: FormCheckboxSearchGroupProps) => {
  const { id, name, options, value = [] } = props;
  const fieldsetProps: FormFieldsetProps = {
    id,
    legend,
    legendSize,
    legendHidden,
    hint,
    error,
  };

  const { isMounted } = useMounted();

  const [searchTerm, setSearchTerm] = useState('');
  const lowercaseSearchTerm = searchTerm.toLowerCase();

  let filteredOptions = options;

  if (searchTerm) {
    filteredOptions = options.filter(
      option =>
        option.label.toLowerCase().includes(lowercaseSearchTerm) ||
        value.indexOf(option.value) > -1,
    );
  }

  return (
    <>
      {isMounted ? (
        <FormFieldset {...fieldsetProps}>
          <FormTextSearchInput
            id={`${id}-search`}
            name={`${name}-search`}
            onChange={event => setSearchTerm(event.target.value)}
            onKeyPress={event => {
              if (event.key === 'Enter') {
                event.preventDefault();
              }
            }}
            label={searchLabel}
            width={20}
          />

          <div aria-live="assertive" className={styles.optionsContainer}>
            <BaseFormCheckboxGroup {...props} options={filteredOptions} small />
          </div>
        </FormFieldset>
      ) : (
        <FormCheckboxGroup {...props} {...fieldsetProps} />
      )}
    </>
  );
};

export default FormCheckboxSearchGroup;
