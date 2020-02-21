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
  hideCount?: boolean;
  searchLabel?: string;
}

const FormCheckboxSearchGroup = ({
  hideCount = false,
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

  const selectedCount = options.reduce(
    (acc, option) => (value.indexOf(option.value) > -1 ? acc + 1 : acc),
    0,
  );

  return (
    <>
      {isMounted ? (
        <FormFieldset {...fieldsetProps}>
          {selectedCount > 0 && !hideCount && (
            <div className="govuk-!-margin-bottom-2">
              <span className="govuk-tag govuk-!-font-size-14">{`${selectedCount} selected`}</span>
            </div>
          )}

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
