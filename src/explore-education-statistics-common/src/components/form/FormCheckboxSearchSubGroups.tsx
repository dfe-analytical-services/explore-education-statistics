import FormCheckboxGroup from '@common/components/form/FormCheckboxGroup';
import FormFieldset, {
  FormFieldsetProps,
} from '@common/components/form/FormFieldset';
import useMounted from '@common/hooks/useMounted';
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
  ...props
}: FormCheckboxSearchSubGroupsProps) => {
  const { isMounted } = useMounted();

  const [searchTerm, setSearchTerm] = useState('');

  if (!isMounted) {
    return <FormCheckboxSubGroups {...props} />;
  }

  const {
    id,
    legend,
    hint,
    legendHidden,
    legendSize,
    error,
    name,
    onAllChange,
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
    const lowercaseSearchTerm = searchTerm.toLowerCase();

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
    <FormFieldset {...fieldsetProps}>
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
        {filteredOptions.map((optionGroup, index) => (
          <FormCheckboxGroup
            {...groupProps}
            key={optionGroup.legend}
            name={name}
            id={optionGroup.id ? optionGroup.id : `${id}-${index + 1}`}
            legend={optionGroup.legend}
            legendSize="s"
            options={optionGroup.options}
            value={value}
            onAllChange={(event, checked) => {
              if (onAllChange) {
                onAllChange(event, checked, optionGroup.options);
              }
            }}
          />
        ))}
      </div>
    </FormFieldset>
  );
};

export default FormCheckboxSearchSubGroups;
