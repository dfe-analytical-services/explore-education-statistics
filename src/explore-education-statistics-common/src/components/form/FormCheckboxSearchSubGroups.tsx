import ButtonText from '@common/components/ButtonText';
import FormCheckboxGroup, {
  BaseFormCheckboxGroup,
  CheckboxGroupAllChangeEvent,
  CheckboxOption,
  FormCheckboxGroupProps,
} from '@common/components/form/FormCheckboxGroup';
import FormFieldset, {
  FormFieldsetProps,
} from '@common/components/form/FormFieldset';
import useMounted from '@common/hooks/useMounted';
import { OmitStrict } from '@common/types';
import React, {
  MouseEvent,
  MouseEventHandler,
  useCallback,
  useMemo,
  useState,
  memo,
} from 'react';
import styles from './FormCheckboxSearchSubGroups.module.scss';
import FormTextSearchInput from './FormTextSearchInput';

export interface OptionGroup {
  id?: string;
  legend: string;
  options: CheckboxOption[];
}

export interface FormCheckboxSearchSubGroupsProps
  extends OmitStrict<
    FormCheckboxGroupProps,
    'onAllChange' | 'selectAll' | 'selectAllText' | 'options'
  > {
  options: OptionGroup[];
  onAllChange?: (
    event: MouseEvent<HTMLButtonElement>,
    checked: boolean,
    options: OptionGroup[],
  ) => void;
  onSubGroupAllChange?: (
    event: CheckboxGroupAllChangeEvent,
    checked: boolean,
    options: CheckboxOption[],
  ) => void;
  searchLabel?: string;
}

const FormCheckboxSearchSubGroups = ({
  searchLabel = 'Search options',
  onAllChange,
  onSubGroupAllChange,
  ...props
}: FormCheckboxSearchSubGroupsProps) => {
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

  const { isMounted, onMounted } = useMounted();

  const [searchTerm, setSearchTerm] = useState('');

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
      filteredOptions.every(group =>
        group.options.every(option => value.includes(option.value)),
      ),
    [filteredOptions, value],
  );

  const handleAllGroupsChange: MouseEventHandler<HTMLButtonElement> = useCallback(
    event => {
      if (onAllChange) {
        onAllChange(event, isAllChecked, filteredOptions);
      }
    },
    [isAllChecked, onAllChange, filteredOptions],
  );

  return (
    <FormFieldset {...fieldsetProps} useFormId={false}>
      {isMounted && (
        <>
          {totalOptions > 1 && options.length > 1 && (
            <ButtonText
              id={`${id}-all`}
              className="govuk-!-margin-bottom-4"
              underline={false}
              onClick={handleAllGroupsChange}
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
      )}

      <div
        aria-live={onMounted('assertive')}
        className={styles.optionsContainer}
      >
        {options.length > 1 ? (
          <>
            {filteredOptions.map((optionGroup, index) => (
              <FormCheckboxGroup
                {...groupProps}
                key={optionGroup.legend}
                name={name}
                id={
                  optionGroup.id
                    ? `${id}-${optionGroup.id}`
                    : `${id}-options-${index + 1}`
                }
                legend={optionGroup.legend}
                legendSize="s"
                options={optionGroup.options}
                value={value}
                selectAll
                selectAllText={(allChecked, opts) =>
                  `${allChecked ? 'Unselect' : 'Select'} all ${
                    opts.length
                  } subgroup options`
                }
                onAllChange={(event, checked) => {
                  if (onSubGroupAllChange) {
                    onSubGroupAllChange(event, checked, optionGroup.options);
                  }
                }}
                hiddenText={`for ${optionGroup.legend}`}
              />
            ))}
          </>
        ) : (
          <BaseFormCheckboxGroup
            {...groupProps}
            name={name}
            id={options[0].id ? options[0].id : `${id}-1`}
            options={
              filteredOptions.length
                ? filteredOptions[0].options
                : options[0].options
            }
            value={value}
            selectAll
            onAllChange={(event, checked) => {
              if (onSubGroupAllChange) {
                onSubGroupAllChange(event, checked, filteredOptions[0].options);
              }
            }}
          />
        )}
      </div>
    </FormFieldset>
  );
};

export default memo(FormCheckboxSearchSubGroups);
