import ButtonText from '@common/components/ButtonText';
import FormCheckboxGroup, {
  BaseFormCheckboxGroup,
  CheckboxGroupAllChangeEvent,
  CheckboxOption,
  FormCheckboxGroupProps,
  getDefaultSelectAllText,
} from '@common/components/form/FormCheckboxGroup';
import FormFieldset, {
  FormFieldsetProps,
} from '@common/components/form/FormFieldset';
import styles from '@common/components/form/FormCheckboxSearchSubGroups.module.scss';
import FormTextSearchInput from '@common/components/form//FormTextSearchInput';
import VisuallyHidden from '@common/components/VisuallyHidden';
import useMounted from '@common/hooks/useMounted';
import { Dictionary, OmitStrict } from '@common/types';
import React, {
  MouseEvent,
  MouseEventHandler,
  useCallback,
  useMemo,
  useState,
  memo,
} from 'react';

export interface OptionGroup {
  id?: string;
  legend: string;
  options: CheckboxOption[];
}

export interface FormCheckboxSearchSubGroupsProps extends OmitStrict<
  FormCheckboxGroupProps,
  'onAllChange' | 'selectAll' | 'selectAllText' | 'options'
> {
  groupLabel?: string;
  /**
   * Option values to keep visible when searching, keyed by option value.
   * An option is shown if its own label or any linked option's label
   * matches the search term.
   */
  linkedOptionValues?: Dictionary<string[]>;
  options: OptionGroup[];
  searchLabel?: string;
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
}

const FormCheckboxSearchSubGroups = ({
  linkedOptionValues = {},
  searchLabel,
  onAllChange,
  onSubGroupAllChange,
  ...props
}: FormCheckboxSearchSubGroupsProps) => {
  const {
    error,
    groupLabel,
    hint,
    id,
    legend,
    legendHidden,
    legendSize,
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

  const { isMounted } = useMounted();

  const [searchTerm, setSearchTerm] = useState('');

  let filteredOptions = options;

  if (searchTerm) {
    const lowercaseSearchTerm = searchTerm.toLowerCase();

    const matchingValues = new Set(
      options
        .flatMap(optionGroup => optionGroup.options)
        .filter(option =>
          option.label.toLowerCase().includes(lowercaseSearchTerm),
        )
        .map(option => option.value),
    );

    const isVisible = (option: CheckboxOption) =>
      value.indexOf(option.value) > -1 ||
      matchingValues.has(option.value) ||
      (linkedOptionValues[option.value] ?? []).some(linkedValue =>
        matchingValues.has(linkedValue),
      );

    filteredOptions = options
      .filter(optionGroup => optionGroup.options.some(isVisible))
      .map(optionGroup => ({
        ...optionGroup,
        options: optionGroup.options.filter(isVisible),
      }));
  }

  const totalOptions = useMemo(
    () => options.reduce((acc, group) => acc + group.options.length, 0),
    [options],
  );

  // Disabled options can't be selected on their own, so are left out of select all
  const selectableFilteredOptions = useMemo(
    () =>
      filteredOptions.map(group => ({
        ...group,
        options: group.options.filter(option => !option.disabled),
      })),
    [filteredOptions],
  );

  const totalFilteredOptions = useMemo(
    () =>
      selectableFilteredOptions.reduce(
        (acc, group) => acc + group.options.length,
        0,
      ),
    [selectableFilteredOptions],
  );

  const isAllChecked = useMemo(
    () =>
      totalFilteredOptions > 0 &&
      selectableFilteredOptions.every(group =>
        group.options.every(option => value.includes(option.value)),
      ),
    [selectableFilteredOptions, totalFilteredOptions, value],
  );

  const handleAllGroupsChange: MouseEventHandler<HTMLButtonElement> =
    useCallback(
      event => {
        onAllChange?.(event, isAllChecked, selectableFilteredOptions);
      },
      [isAllChecked, onAllChange, selectableFilteredOptions],
    );

  const renderCheckboxes = () => {
    return (
      <>
        {options.length > 1 ? (
          <>
            {filteredOptions.map((optionGroup, index) => (
              <FormCheckboxGroup
                {...groupProps}
                key={optionGroup.legend}
                name={name}
                groupLabel={groupLabel}
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
                selectAllText={(allChecked, opts) => (
                  <>
                    {`${allChecked ? 'Unselect' : 'Select'} all ${
                      opts.length
                    } subgroup options`}
                    <VisuallyHidden>{` for ${optionGroup.legend}`}</VisuallyHidden>
                  </>
                )}
                onAllChange={(event, checked, selectableOptions) => {
                  onSubGroupAllChange?.(event, checked, selectableOptions);
                }}
              />
            ))}
          </>
        ) : (
          <BaseFormCheckboxGroup
            {...groupProps}
            name={name}
            id={options[0].id ? options[0].id : `${id}-1`}
            groupLabel={groupLabel}
            options={
              filteredOptions.length
                ? filteredOptions[0].options
                : options[0].options
            }
            value={value}
            selectAll
            onAllChange={(event, checked, selectableOptions) => {
              onSubGroupAllChange?.(event, checked, selectableOptions);
            }}
          />
        )}
      </>
    );
  };

  if (!isMounted) {
    return renderCheckboxes();
  }

  return (
    <FormFieldset {...fieldsetProps} useFormId={false}>
      {totalOptions > 1 && options.length > 1 && (
        <ButtonText
          id={`${id}-all`}
          className="govuk-!-margin-bottom-4"
          underline={false}
          onClick={handleAllGroupsChange}
        >
          {getDefaultSelectAllText(isAllChecked, totalFilteredOptions)}
          {groupLabel && (
            <VisuallyHidden>{` for ${groupLabel}`}</VisuallyHidden>
          )}
        </ButtonText>
      )}

      {totalOptions > 1 && (
        <FormTextSearchInput
          id={`${id}-search`}
          name={`${name}-search`}
          label={
            searchLabel || (
              <>
                Search options
                {groupLabel && (
                  <VisuallyHidden>{` for ${groupLabel}`}</VisuallyHidden>
                )}
              </>
            )
          }
          width={20}
          onChange={event => setSearchTerm(event.target.value)}
          onKeyPress={event => {
            if (event.key === 'Enter') {
              event.preventDefault();
            }
          }}
        />
      )}

      <div aria-live="assertive" className={styles.optionsContainer}>
        {renderCheckboxes()}
      </div>
    </FormFieldset>
  );
};

export default memo(FormCheckboxSearchSubGroups);
