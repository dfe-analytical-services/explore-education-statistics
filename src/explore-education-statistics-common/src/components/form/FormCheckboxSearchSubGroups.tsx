import FormCheckboxGroup from '@common/components/form/FormCheckboxGroup';
import FormFieldset, {
  FormFieldsetProps,
} from '@common/components/form/FormFieldset';
import useMounted from '@common/hooks/useMounted';
import camelCase from 'lodash/camelCase';
import React, { useState } from 'react';
import styles from './FormCheckboxSearchSubGroups.module.scss';
import FormCheckboxSubGroups, {
  FormCheckboxSubGroupsProps,
} from './FormCheckboxSubGroups';
import FormTextSearchInput from './FormTextSearchInput';

export interface FormCheckboxSearchSubGroupsProps
  extends FormCheckboxSubGroupsProps {
  searchLabel?: string;
}

const FormCheckboxSearchSubGroups = ({
  searchLabel = 'Search options',
  legend,
  legendHidden,
  legendSize,
  hint,
  error,
  ...props
}: FormCheckboxSearchSubGroupsProps) => {
  const { id, name, options, onAllChange, value = [] } = props;
  const fieldsetProps: FormFieldsetProps = {
    id,
    legend,
    legendHidden,
    legendSize,
    hint,
    error,
  };

  const { isMounted } = useMounted();

  const [searchTerm, setSearchTerm] = useState('');
  const lowercaseSearchTerm = searchTerm.toLowerCase();

  let filteredOptions = options;

  if (searchTerm) {
    filteredOptions = options
      .filter(optionGroup =>
        optionGroup.options.some(
          option =>
            value.indexOf(option.value) > -1 ||
            option.label.toLowerCase().includes(lowercaseSearchTerm),
        ),
      )
      .map(optionGroup => ({
        ...optionGroup,
        options: optionGroup.options.filter(
          option =>
            value.indexOf(option.value) > -1 ||
            option.label.toLowerCase().includes(lowercaseSearchTerm),
        ),
      }));
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
            {filteredOptions.map(optionGroup => (
              <FormCheckboxGroup
                {...props}
                id={
                  optionGroup.id
                    ? optionGroup.id
                    : `${id}-${camelCase(optionGroup.legend)}`
                }
                key={optionGroup.legend}
                legend={optionGroup.legend}
                legendSize="s"
                options={optionGroup.options}
                onAllChange={(event, checked) => {
                  if (onAllChange) {
                    onAllChange(event, checked, optionGroup.options);
                  }
                }}
              />
            ))}
          </div>
        </FormFieldset>
      ) : (
        <FormCheckboxSubGroups {...fieldsetProps} {...props} />
      )}
    </>
  );
};

export default FormCheckboxSearchSubGroups;
