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
  selectedItem: number;
}

interface Props {
  afterInput?: ReactNode | ((props: ComboBoxRenderProps) => ReactNode);
  classes?: Partial<Record<'inputLabel', string>>;
  id: string;
  inputLabel: ReactNode;
  inputProps?: InputHTMLAttributes<HTMLInputElement>;
  initialOption?: number;
  listBoxLabel?: ReactNode | ((props: ComboBoxRenderProps) => ReactNode);
  listBoxLabelId?: string;
  options?: ReactNode[] | ((props: ComboBoxRenderProps) => ReactNode[]);
  onInputChange: ChangeEventHandler<HTMLInputElement>;
  onSelect(selectedItem: number): void;
}

const FormComboBox = ({
  afterInput,
  classes = {},
  id,
  inputLabel,
  inputProps,
  initialOption = -1,
  options,
  listBoxLabel,
  listBoxLabelId,
  onInputChange,
  onSelect,
}: Props) => {
  const listBoxRef = useRef<HTMLUListElement>(null);
  const inputRef = useRef<HTMLInputElement>(null);
  const itemRefs = useRef<Dictionary<HTMLLIElement>>({});

  const [value, setValue] = useState('');
  const [selectedItem, setSelectedItem] = useState(initialOption);

  const renderedOptions =
    typeof options === 'function' ? options({ selectedItem, value }) : options;

  const adjustListBoxScroll = (
    event: KeyboardEvent<HTMLElement>,
    nextSelectedItem: number,
  ) => {
    if (!renderedOptions || !listBoxRef.current || !itemRefs.current) {
      return;
    }

    const optionEl = itemRefs.current[nextSelectedItem];

    if (!optionEl) {
      return;
    }

    switch (event.key) {
      case 'ArrowUp':
        if (nextSelectedItem === renderedOptions.length - 1) {
          listBoxRef.current.scrollTop = listBoxRef.current.scrollHeight;
        } else {
          listBoxRef.current.scrollTop -= optionEl.offsetHeight;
        }
        break;
      case 'ArrowDown':
        if (nextSelectedItem === 0) {
          listBoxRef.current.scrollTop = 0;
        } else {
          listBoxRef.current.scrollTop += optionEl.offsetHeight;
        }
        break;
      default:
    }
  };

  const selectNextItem = (event: KeyboardEvent<HTMLElement>) => {
    event.persist();
    event.preventDefault();

    if (!renderedOptions || !renderedOptions.length) {
      return -1;
    }

    let nextSelectedItem = selectedItem;

    switch (event.key) {
      case 'ArrowUp':
        if (selectedItem <= 0) {
          nextSelectedItem = renderedOptions.length - 1;
        } else {
          nextSelectedItem = selectedItem - 1;
        }
        break;
      case 'ArrowDown':
        if (selectedItem >= renderedOptions.length - 1) {
          nextSelectedItem = 0;
        } else {
          nextSelectedItem = selectedItem + 1;
        }
        break;
      default:
        return selectedItem;
    }

    setSelectedItem(nextSelectedItem);
    adjustListBoxScroll(event, nextSelectedItem);

    return nextSelectedItem;
  };

  return (
    <div className={styles.container}>
      <div
        aria-expanded={renderedOptions ? renderedOptions.length > 0 : undefined}
        aria-owns={`${id}-options`}
        aria-haspopup="listbox"
        className="govuk-form-group"
        // eslint-disable-next-line jsx-a11y/role-has-required-aria-props
        role="combobox"
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
            selectedItem > -1 ? `${id}-option-${selectedItem}` : undefined
          }
          aria-controls={`${id}-options`}
          className="govuk-input"
          id={`${id}-input`}
          value={value}
          ref={inputRef}
          onChange={event => {
            event.persist();

            setValue(event.target.value);
            itemRefs.current = {};

            onInputChange(event);
          }}
          onKeyDown={event => {
            if (event.key === 'ArrowUp' || event.key === 'ArrowDown') {
              selectNextItem(event);

              if (listBoxRef.current) {
                listBoxRef.current.focus();
              }
            }
          }}
        />

        {typeof afterInput === 'function'
          ? afterInput({ selectedItem, value })
          : afterInput}

        {renderedOptions && (
          <div className={styles.optionsContainer}>
            {typeof listBoxLabel === 'function'
              ? listBoxLabel({ selectedItem, value })
              : listBoxLabel}

            {renderedOptions.length > 0 && (
              <ul
                aria-labelledby={listBoxLabelId}
                className={styles.options}
                id={`${id}-options`}
                ref={listBoxRef}
                role="listbox"
                tabIndex={-1}
                onKeyDown={event => {
                  selectNextItem(event);

                  const inputEl = inputRef.current;

                  if (inputEl) {
                    if (
                      event.key === 'ArrowLeft' ||
                      event.key === 'ArrowRight'
                    ) {
                      const directionChange =
                        event.key === 'ArrowLeft' ? -1 : 1;

                      inputEl.selectionStart = inputEl.selectionStart
                        ? inputEl.selectionStart + directionChange
                        : 0;
                      inputEl.selectionEnd = inputEl.selectionStart;
                      inputEl.focus();
                    }

                    if (event.key === 'Home') {
                      inputEl.selectionStart = 0;
                      inputEl.selectionEnd = 0;
                      inputEl.focus();
                    }

                    if (event.key === 'End') {
                      inputEl.selectionStart = inputEl.value.length;
                      inputEl.selectionEnd = inputEl.selectionStart;
                      inputEl.focus();
                    }

                    if (event.key === 'Enter') {
                      onSelect(selectedItem);
                    }
                  }
                }}
              >
                {renderedOptions.map((item, index) => {
                  const key = index;

                  return (
                    // eslint-disable-next-line jsx-a11y/click-events-have-key-events
                    <li
                      aria-selected={selectedItem === index}
                      key={key}
                      id={`${id}-option-${index}`}
                      className={
                        selectedItem === index ? styles.selected : undefined
                      }
                      role="option"
                      ref={el => {
                        if (el) {
                          itemRefs.current[key] = el;
                        }
                      }}
                      onClick={() => onSelect(index)}
                    >
                      {item}
                    </li>
                  );
                })}
              </ul>
            )}
          </div>
        )}
      </div>
    </div>
  );
};

export default FormComboBox;
