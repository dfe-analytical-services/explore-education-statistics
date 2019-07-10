import useClickAway from '@common/hooks/useClickAway';
import useToggle from '@common/hooks/useToggle';
import { Dictionary } from '@common/types';
import classNames from 'classnames';
import React, {
  ChangeEventHandler,
  InputHTMLAttributes,
  KeyboardEvent,
  ReactNode,
  useRef,
  useState,
} from 'react';
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

  const [value, setValue] = useState('');
  const [selectedOption, setSelectedOption] = useState(initialOption);
  const [showOptions, toggleShowOptions] = useToggle(false);

  React.useEffect(() => {
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
      <div
        // eslint-disable-next-line jsx-a11y/role-has-required-aria-props
        role="combobox"
        aria-expanded={renderedOptions ? renderedOptions.length > 0 : false}
        aria-owns={`${id}-options`}
        aria-haspopup="listbox"
        className="govuk-form-group"
      >
        <label
          className={classNames('govuk-label', classes.inputLabel)}
          htmlFor={`${id}-input`}
        >
          {inputLabel}
        </label>

        <input
          type="text"
          {...inputProps}
          aria-autocomplete="list"
          aria-activedescendant={
            selectedOption > -1 ? `${id}-option-${selectedOption}` : undefined
          }
          aria-controls={`${id}-options`}
          className="govuk-input"
          id={`${id}-input`}
          value={value}
          ref={inputRef}
          onChange={event => {
            event.persist();

            setValue(event.target.value);
            optionRefs.current = {};

            onInputChange(event);
            toggleShowOptions(true);
          }}
          onKeyDown={event => {
            if (event.key === 'ArrowUp' || event.key === 'ArrowDown') {
              selectNextOption(event);

              if (listBoxRef.current) {
                listBoxRef.current.focus();
              }
            }

            if (event.key === 'Escape') {
              setValue('');
              toggleShowOptions(false);
            }
          }}
        />

        {typeof afterInput === 'function'
          ? afterInput({ selectedOption, value })
          : afterInput}

        {showOptions && renderedOptions && (
          <div className={styles.optionsContainer}>
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
              tabIndex={-1}
              onKeyDown={event => {
                selectNextOption(event);

                const inputEl = inputRef.current;

                if (inputEl) {
                  switch (event.key) {
                    case 'ArrowLeft':
                    case 'ArrowRight': {
                      const directionChange =
                        event.key === 'ArrowLeft' ? -1 : 1;

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

                    default:
                  }
                }
              }}
            >
              {renderedOptions &&
                renderedOptions.length > 0 &&
                renderedOptions.map((item, index) => {
                  const key = index;

                  return (
                    // eslint-disable-next-line jsx-a11y/click-events-have-key-events
                    <li
                      aria-selected={selectedOption === index}
                      key={key}
                      id={`${id}-option-${index}`}
                      className={
                        selectedOption === index ? styles.selected : undefined
                      }
                      role="option"
                      ref={el => {
                        if (el) {
                          optionRefs.current[key] = el;
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
    </div>
  );
};

export default FormComboBox;
