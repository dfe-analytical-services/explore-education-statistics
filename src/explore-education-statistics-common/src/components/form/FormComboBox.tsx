import useClickAway from '@common/hooks/useClickAway';
import useToggle from '@common/hooks/useToggle';
import { Dictionary } from '@common/types';
import classNames from 'classnames';
import React, {
  ChangeEventHandler,
  InputHTMLAttributes,
  KeyboardEvent,
  ReactNode,
  useEffect,
  useRef,
  useState,
} from 'react';
import inputStyles from '@common/components/form/FormTextSearchInput.module.scss';
import styles from './FormComboBox.module.scss';

interface ComboBoxRenderProps {
  value: string;
  selectedOption: number;
}

interface Props {
  afterInput?: ReactNode | ((props: ComboBoxRenderProps) => ReactNode);
  classes?: Partial<Record<'inputLabel', string>>;
  id: string;
  inputLabel: ReactNode;
  inputProps?: InputHTMLAttributes<HTMLInputElement>;
  inputValue?: string;
  initialOption?: number;
  listBoxLabel?: ReactNode | ((props: ComboBoxRenderProps) => ReactNode);
  listBoxLabelId?: string;
  options?: ReactNode[] | ((props: ComboBoxRenderProps) => ReactNode[]);
  onInputChange: ChangeEventHandler<HTMLInputElement>;
  onSelect(selectedOption: number): void;
}

const FormComboBox = ({
  afterInput,
  classes = {},
  id,
  inputLabel,
  inputProps,
  inputValue = '',
  initialOption = -1,
  options,
  listBoxLabel,
  listBoxLabelId,
  onInputChange,
  onSelect,
}: Props) => {
  const containerRef = useRef<HTMLDivElement>(null);
  const listBoxRef = useRef<HTMLUListElement>(null);
  const inputRef = useRef<HTMLInputElement>(null);
  const optionRefs = useRef<Dictionary<HTMLLIElement>>({});

  const [value, setValue] = useState<string>('');
  const [selectedOption, setSelectedOption] = useState<number>(initialOption);
  const [showOptions, toggleShowOptions] = useToggle(false);

  useEffect(() => {
    setValue(inputValue);
  }, [inputValue]);

  useClickAway(containerRef, () => {
    toggleShowOptions(false);
  });

  const renderedOptions =
    typeof options === 'function'
      ? options({ selectedOption, value })
      : options;

  const adjustListBoxScroll = (
    event: KeyboardEvent<HTMLElement>,
    nextSelectedOption: number,
  ) => {
    if (!renderedOptions || !listBoxRef.current || !optionRefs.current) {
      return;
    }

    const optionEl = optionRefs.current[nextSelectedOption];

    if (!optionEl) {
      return;
    }

    switch (event.key) {
      case 'ArrowUp':
        if (nextSelectedOption === renderedOptions.length - 1) {
          listBoxRef.current.scrollTop = listBoxRef.current.scrollHeight;
        } else {
          listBoxRef.current.scrollTop -= optionEl.offsetHeight;
        }
        break;
      case 'ArrowDown':
        if (nextSelectedOption === 0) {
          listBoxRef.current.scrollTop = 0;
        } else {
          listBoxRef.current.scrollTop += optionEl.offsetHeight;
        }
        break;
      default:
    }
  };

  const selectNextOption = (event: KeyboardEvent<HTMLElement>) => {
    event.persist();
    event.preventDefault();

    if (!renderedOptions || !renderedOptions.length) {
      return -1;
    }

    let nextSelectedOption = selectedOption;

    switch (event.key) {
      case 'ArrowUp':
        if (selectedOption <= 0) {
          nextSelectedOption = renderedOptions.length - 1;
        } else {
          nextSelectedOption = selectedOption - 1;
        }
        break;
      case 'ArrowDown':
        if (selectedOption >= renderedOptions.length - 1) {
          nextSelectedOption = 0;
        } else {
          nextSelectedOption = selectedOption + 1;
        }
        break;
      default:
        return selectedOption;
    }

    setSelectedOption(nextSelectedOption);
    adjustListBoxScroll(event, nextSelectedOption);

    return nextSelectedOption;
  };

  const handleContainerInteraction = () => {
    if (showOptions) {
      return;
    }

    // Re-show options if they have been hidden
    // e.g. when the user clicks away from the combobox
    if (renderedOptions) {
      toggleShowOptions(true);
    }
  };

  return (
    <div
      className={styles.container}
      role="none"
      onClick={handleContainerInteraction}
      onKeyDown={handleContainerInteraction}
      ref={containerRef}
    >
      <label
        className={classNames('govuk-label', classes.inputLabel)}
        htmlFor={`${id}-input`}
      >
        {inputLabel}
      </label>

      <input
        type="text"
        // eslint-disable-next-line react/jsx-props-no-spreading
        {...inputProps}
        role="combobox"
        className={classNames(inputStyles.searchInput, 'govuk-input')}
        id={`${id}-input`}
        value={value}
        ref={inputRef}
        aria-autocomplete="list"
        aria-haspopup="listbox"
        aria-controls={`${id}-options`}
        aria-expanded={renderedOptions ? renderedOptions.length > 0 : false}
        aria-activedescendant={
          selectedOption > -1 ? `${id}-option-${selectedOption}` : undefined
        }
        onChange={event => {
          event.persist();

          setValue(event.target.value);
          optionRefs.current = {};

          onInputChange(event);
          toggleShowOptions(true);
        }}
        onKeyDown={event => {
          switch (event.key) {
            case 'ArrowUp':
            case 'ArrowDown':
              selectNextOption(event);
              break;
            case 'Escape':
              setValue('');
              toggleShowOptions(false);
              break;
            case 'Tab':
              toggleShowOptions(false);
              break;
            default:
              break;
          }
        }}
      />

      {typeof afterInput === 'function'
        ? afterInput({ selectedOption, value })
        : afterInput}

      {showOptions && renderedOptions && (
        <div className={styles.optionsContainer} role="alert">
          {typeof listBoxLabel === 'function'
            ? listBoxLabel({ selectedOption, value })
            : listBoxLabel}
          <ul
            aria-labelledby={listBoxLabelId}
            className={classNames(styles.options, {
              [styles.optionsNoFocus]: renderedOptions.length === 0,
            })}
            id={`${id}-options`}
            ref={listBoxRef}
            role="listbox"
            onKeyDown={event => {
              selectNextOption(event);
              const inputEl = inputRef.current;

              if (inputEl) {
                switch (event.key) {
                  case 'ArrowLeft':
                  case 'ArrowRight': {
                    const directionChange = event.key === 'ArrowLeft' ? -1 : 1;

                    inputEl.selectionStart = inputEl.selectionStart
                      ? inputEl.selectionStart + directionChange
                      : 0;
                    inputEl.selectionEnd = inputEl.selectionStart;
                    inputEl.focus();
                    break;
                  }

                  case 'Enter':
                    onSelect(selectedOption);
                    toggleShowOptions(false);
                    break;

                  case 'Home':
                    inputEl.selectionStart = 0;
                    inputEl.selectionEnd = 0;
                    inputEl.focus();
                    break;

                  case 'End':
                    inputEl.selectionStart = inputEl.value.length;
                    inputEl.selectionEnd = inputEl.selectionStart;
                    inputEl.focus();
                    break;

                  case 'Escape':
                    setValue('');
                    toggleShowOptions(false);
                    inputEl.focus();
                    break;

                  case 'Tab':
                    toggleShowOptions(false);
                    break;

                  default:
                }
              }
            }}
          >
            {renderedOptions &&
              renderedOptions.length > 0 &&
              renderedOptions.map((item, index) => {
                const key = index;

                const isSelected = selectedOption === index;

                return (
                  // eslint-disable-next-line jsx-a11y/click-events-have-key-events
                  <li
                    aria-posinset={index + 1}
                    aria-selected={isSelected}
                    aria-setsize={renderedOptions.length}
                    key={key}
                    id={`${id}-option-${index}`}
                    className={isSelected ? styles.selected : undefined}
                    role="option"
                    tabIndex={-1}
                    ref={el => {
                      if (el) {
                        optionRefs.current[index] = el;

                        if (isSelected) {
                          // this is a bit hacky but it seems to be the only way to focus
                          // without two list items being selected at the same time
                          // (and the focus being on the input instead of the list item)
                          el.focus();
                        }
                      }
                    }}
                    onClick={() => {
                      onSelect(index);
                      toggleShowOptions(false);
                    }}
                  >
                    {item}
                  </li>
                );
              })}
          </ul>
        </div>
      )}
    </div>
  );
};

export default FormComboBox;
