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

  const [searchTerm, setSearchTerm] = useState('');
  const { isMounted } = useMounted();

  let filteredOptions = options;

  if (searchTerm) {
    filteredOptions = options.filter(
      option =>
        new RegExp(searchTerm, 'i').test(option.label) ||
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
          <div className={styles.inputContainer}>
            <FormTextSearchInput
              id={`${id}-search`}
              name={`${name}-search`}
              onChange={event => setSearchTerm(event.target.value)}
              label={searchLabel}
              width={20}
            />

            {selectedCount > 0 && !hideCount && (
              <div className="govuk-!-margin-top-2">
                <span className="govuk-tag">{`${selectedCount} selected`}</span>
              </div>
            )}
          </div>

          <div className={styles.optionsContainer}>
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
