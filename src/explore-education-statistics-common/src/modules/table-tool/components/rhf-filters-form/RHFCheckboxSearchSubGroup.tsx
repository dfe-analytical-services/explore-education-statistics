import ButtonText from '@common/components/ButtonText';
import { FormFieldset } from '@common/components/form';
import { CheckboxOption } from '@common/components/form/FormCheckboxGroup';
import { FormFieldsetProps } from '@common/components/form/FormFieldset';
import FormTextSearchInput from '@common/components/form/FormTextSearchInput';
import useMounted from '@common/hooks/useMounted';
import RHFCheckboxGroup from '@common/modules/table-tool/components/rhf-filters-form/RHFCheckboxGroup';
import styles from '@common/components/form/FormCheckboxSearchGroup.module.scss';
import difference from 'lodash/difference';
import React, { useMemo, useState, ReactNode } from 'react';
import { UseFormRegister, Path, Control, useWatch } from 'react-hook-form';

interface OptionGroup {
  legend: string;
  options: CheckboxOption[];
}

type RHFCheckboxSearchSubGroupProps<TFormValues> = {
  control: Control<TFormValues>;
  disabled?: boolean;
  error?: string;
  hint?: string | ReactNode;
  id: string;
  legend: string | ReactNode;
  legendHidden?: boolean;
  legendSize?: 'xl' | 'l' | 'm' | 's';
  name: Path<TFormValues>;
  register: UseFormRegister<TFormValues>;
  options: OptionGroup[];
  searchLabel?: string;
  onToggleSelectAll: (nextValues: string[]) => void;
};

const RHFCheckboxSearchSubGroup = <
  TFormValues extends Record<string, unknown>
>({
  control,
  disabled = false,
  error,
  hint,
  id,
  legend,
  legendHidden = false,
  legendSize = 'l',
  name,
  register,
  options,
  searchLabel = 'Search options',
  onToggleSelectAll,
}: RHFCheckboxSearchSubGroupProps<TFormValues>) => {
  const { onMounted } = useMounted();

  const [searchTerm, setSearchTerm] = useState('');

  const fieldsetProps: FormFieldsetProps = {
    hint,
    id,
    legend,
    legendHidden,
    legendSize,
  };

  // Converting here so can manipulate as normal array. Not sure why the return type doesn't allow that.
  const selectedValues = useWatch({ control, name }) as string[];

  let filteredOptions = options;

  if (searchTerm) {
    const lowercaseSearchTerm = searchTerm.toLowerCase();

    filteredOptions = options
      .filter(optionGroup =>
        optionGroup.options.some(
          option =>
            selectedValues.indexOf(option.value) > -1 ||
            option.label.toLowerCase().includes(lowercaseSearchTerm),
        ),
      )
      .map(optionGroup => ({
        ...optionGroup,
        options: optionGroup.options.filter(
          option =>
            selectedValues.indexOf(option.value) > -1 ||
            option.label.toLowerCase().includes(lowercaseSearchTerm),
        ),
      }));
  }

  const totalOptions = useMemo(
    () => options.reduce((acc, group) => acc + group.options.length, 0),
    [options],
  );

  const totalFilteredOptions = useMemo(
    () => filteredOptions.reduce((acc, group) => acc + group.options.length, 0),
    [filteredOptions],
  );

  const isAllChecked = useMemo(
    () =>
      filteredOptions.every(g =>
        g.options.every(
          option => selectedValues && selectedValues.includes(option.value),
        ),
      ),
    [filteredOptions, selectedValues],
  );

  return (
    <FormFieldset {...fieldsetProps} error={error}>
      <>
        {totalOptions > 1 && options.length > 1 && (
          <ButtonText
            id={`${id}-all`}
            className="govuk-!-margin-bottom-4"
            underline={false}
            onClick={() => {
              const allOptionValues = filteredOptions
                .flatMap(g => g.options)
                .map(option => option.value);
              const restValues = difference(selectedValues, allOptionValues);
              const nextValues = isAllChecked
                ? restValues
                : [...restValues, ...allOptionValues];
              onToggleSelectAll(nextValues);
            }}
          >
            {`${
              isAllChecked ? 'Unselect' : 'Select'
            } all ${totalFilteredOptions} options`}
          </ButtonText>
        )}

        {totalOptions > 1 && (
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
        )}
      </>

      <div
        aria-live={onMounted('assertive')}
        className={styles.optionsContainer}
      >
        <>
          {filteredOptions.map(optionGroup => (
            <RHFCheckboxGroup
              id={`${id}-group`}
              legend={optionGroup.legend}
              values={selectedValues}
              options={optionGroup.options}
              key={optionGroup.legend}
              onToggleSelectAll={onToggleSelectAll}
            >
              {optionGroup.options.map(option => {
                const optionId = `${id}-option-${option.value}`;
                return (
                  <div className="govuk-checkboxes__item" key={option.value}>
                    <input
                      className="govuk-checkboxes__input"
                      disabled={disabled}
                      type="checkbox"
                      id={optionId}
                      value={option.value}
                      // eslint-disable-next-line react/jsx-props-no-spreading
                      {...register(name)}
                    />
                    <label
                      className="govuk-label govuk-checkboxes__label"
                      htmlFor={optionId}
                    >
                      {option.label}
                    </label>
                  </div>
                );
              })}
            </RHFCheckboxGroup>
          ))}
        </>
      </div>
    </FormFieldset>
  );
};

export default RHFCheckboxSearchSubGroup;
