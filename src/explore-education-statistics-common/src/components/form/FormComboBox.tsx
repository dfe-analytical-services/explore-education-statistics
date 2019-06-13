import { Dictionary } from '@common/types';
import React, {
  ChangeEventHandler,
  InputHTMLAttributes,
  KeyboardEvent,
  LiHTMLAttributes,
  ReactNode,
  useRef,
  useState,
} from 'react';
import styles from './FormComboBox.module.scss';

interface ListBoxItem {
  content: ReactNode;
  props?: LiHTMLAttributes<HTMLLIElement>;
}

interface Props {
  id: string;
  inputLabel?(): ReactNode;
  inputProps?: InputHTMLAttributes<HTMLInputElement>;
  afterInput?(props: { value: string; selectedItem: number }): ReactNode;
  listBoxItems?: ListBoxItem[];
  listBoxLabel?(props: { value: string; selectedItem: number }): ReactNode;
  listBoxLabelId?: string;
  onInputChange: ChangeEventHandler<HTMLInputElement>;
  onSelect(selectedItem: number): void;
}

const FormComboBox = ({
  id,
  inputLabel,
  inputProps,
  afterInput,
  listBoxItems,
  listBoxLabel,
  listBoxLabelId,
  onInputChange,
  onSelect,
}: Props) => {
  const listBoxRef = useRef<HTMLUListElement>(null);
  const inputRef = useRef<HTMLInputElement>(null);
  const itemRefs = useRef<Dictionary<HTMLLIElement>>({});

  const [value, setValue] = useState('');
  const [selectedItem, setSelectedItem] = useState(-1);

  const adjustListBoxScroll = (
    event: KeyboardEvent<HTMLElement>,
    nextSelectedItem: number,
  ) => {
    if (!listBoxItems || !listBoxRef.current || !itemRefs.current) {
      return;
    }

    const optionEl = itemRefs.current[nextSelectedItem];

    if (!optionEl) {
      return;
    }

    switch (event.key) {
      case 'ArrowUp':
        if (nextSelectedItem === listBoxItems.length - 1) {
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

    if (!listBoxItems || !listBoxItems.length) {
      return -1;
    }

    let nextSelectedItem = selectedItem;

    switch (event.key) {
      case 'ArrowUp':
        if (selectedItem <= 0) {
          nextSelectedItem = listBoxItems.length - 1;
        } else {
          nextSelectedItem = selectedItem - 1;
        }
        break;
      case 'ArrowDown':
        if (selectedItem >= listBoxItems.length - 1) {
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
        aria-expanded={listBoxItems ? listBoxItems.length > 0 : undefined}
        aria-owns={`${id}-options`}
        aria-haspopup="listbox"
        className="govuk-form-group"
        // eslint-disable-next-line jsx-a11y/role-has-required-aria-props
        role="combobox"
      >
        {inputLabel ? (
          inputLabel()
        ) : (
          <label className="govuk-label" htmlFor={`${id}-input`}>
            {inputLabel}
          </label>
        )}

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

        {afterInput && afterInput({ selectedItem, value })}

        {listBoxItems && (
          <div className={styles.optionsContainer}>
            {listBoxLabel && listBoxLabel({ selectedItem, value })}

            {listBoxItems.length > 0 && (
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
                {listBoxItems.map(({ content, props = {} }, index) => {
                  const key = index;

                  return (
                    // eslint-disable-next-line jsx-a11y/click-events-have-key-events
                    <li
                      {...props}
                      aria-selected={selectedItem === index}
                      key={key}
                      id={`${id}-option-${index}`}
                      className={
                        selectedItem === index ? styles.highlighted : ''
                      }
                      role="option"
                      ref={el => {
                        if (el) {
                          itemRefs.current[key] = el;
                        }
                      }}
                    >
                      {content}
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
