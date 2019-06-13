import {
  accordionSectionClasses,
  openAllParentAccordionSections,
} from '@common/components/AccordionSection';
import { openAllParentDetails } from '@common/components/Details';
import { openAllParentTabSections } from '@common/components/TabsSection';
import findAllByText from '@common/lib/dom/findAllByText';
import findParent from '@common/lib/dom/findParent';
import findPreviousSibling from '@common/lib/dom/findPreviousSibling';
import { Dictionary } from '@common/types';
import classNames from 'classnames';
import debounce from 'lodash/debounce';
import React, {
  ChangeEvent,
  Component,
  createRef,
  KeyboardEvent,
  ReactNode,
} from 'react';
import Highlighter from 'react-highlight-words';
import styles from './SearchForm.module.scss';

interface SearchResult {
  element: Element;
  scrollIntoView: () => void;
  text: ReactNode;
  location: string;
}

interface Props {
  className?: string;
  id?: string;
  elementSelectors: string[];
}

interface State {
  selectedResult: number;
  searchCompleted: boolean;
  searchResults: SearchResult[];
  searchValue: string;
}

class SearchForm extends Component<Props, State> {
  public state: State = {
    selectedResult: -1,
    searchCompleted: false,
    searchResults: [],
    searchValue: '',
  };

  public static defaultProps = {
    elementSelectors: ['p', 'li > strong', 'h2', 'h3', 'h4'],
    id: 'pageSearchForm',
  };

  private boundPerformSearch = debounce(this.performSearch, 1000);

  private readonly inputRef = createRef<HTMLInputElement>();

  private readonly resultsRef = createRef<HTMLUListElement>();

  private optionsRefs: Dictionary<HTMLLIElement> = {};

  private static getLocationText(element: HTMLElement): string {
    const location = [];

    const locationHeaderElement = findPreviousSibling(element, 'h2, h3, h4');

    if (locationHeaderElement) {
      location.unshift(locationHeaderElement.textContent || '');
    }

    const accordionSection = findParent(
      element,
      `.${accordionSectionClasses.section}`,
    );

    if (accordionSection) {
      const accordionHeader = accordionSection.querySelector(
        `.${accordionSectionClasses.sectionButton}`,
      );

      if (accordionHeader) {
        location.unshift(accordionHeader.textContent || '');
      }
    }

    return location.join(' > ');
  }

  private selectNextResult = (event: KeyboardEvent<HTMLElement>) => {
    event.persist();
    event.preventDefault();

    const { selectedResult, searchResults } = this.state;

    if (!searchResults.length) {
      return -1;
    }

    let nextSelectedResult = selectedResult;

    switch (event.key) {
      case 'ArrowUp':
        if (selectedResult <= 0) {
          nextSelectedResult = searchResults.length - 1;
        } else {
          nextSelectedResult = selectedResult - 1;
        }
        break;
      case 'ArrowDown':
        if (selectedResult >= searchResults.length - 1) {
          nextSelectedResult = 0;
        } else {
          nextSelectedResult = selectedResult + 1;
        }
        break;
      default:
        return selectedResult;
    }

    this.setState({ selectedResult: nextSelectedResult }, () => {
      this.adjustResultScroll(event);
    });

    return nextSelectedResult;
  };

  private adjustResultScroll = (event: KeyboardEvent<HTMLElement>) => {
    if (!this.resultsRef.current) {
      return;
    }

    const { selectedResult, searchResults } = this.state;

    const optionEl = this.optionsRefs[selectedResult];

    if (!optionEl) {
      return;
    }

    switch (event.key) {
      case 'ArrowUp':
        if (selectedResult === searchResults.length - 1) {
          this.resultsRef.current.scrollTop = this.resultsRef.current.scrollHeight;
        } else {
          this.resultsRef.current.scrollTop -= optionEl.offsetHeight;
        }
        break;
      case 'ArrowDown':
        if (selectedResult === 0) {
          this.resultsRef.current.scrollTop = 0;
        } else {
          this.resultsRef.current.scrollTop += optionEl.offsetHeight;
        }
        break;
      default:
    }
  };

  private performSearch() {
    const { elementSelectors } = this.props;
    const { searchValue } = this.state;

    if (searchValue.length <= 3) {
      return;
    }

    this.optionsRefs = {};

    const elements = findAllByText(searchValue, elementSelectors.join(', '));

    const searchResults: SearchResult[] = elements.map(element => {
      const location = SearchForm.getLocationText(element);

      const scrollIntoView = () => {
        openAllParentAccordionSections(element);
        openAllParentTabSections(element);
        openAllParentDetails(element);

        this.resetSearch();

        setTimeout(() => {
          element.scrollIntoView({
            behavior: 'smooth',
            block: 'start',
          });
        });
      };

      return {
        element,
        location,
        scrollIntoView,
        text: (
          <Highlighter
            searchWords={[searchValue]}
            textToHighlight={element.textContent || ''}
          />
        ),
      };
    });

    this.setState({ searchResults, searchCompleted: true });
  }

  private resetSearch() {
    this.setState({
      selectedResult: -1,
      searchCompleted: false,
      searchResults: [],
      searchValue: '',
    });
  }

  private onChange = (event: ChangeEvent<HTMLInputElement>) => {
    event.persist();

    this.setState({
      selectedResult: -1,
      searchCompleted: false,
      searchResults: [],
      searchValue: event.currentTarget.value,
    });

    this.boundPerformSearch();
  };

  public render() {
    const { className, id } = this.props;
    const {
      searchCompleted,
      searchResults,
      selectedResult,
      searchValue,
    } = this.state;

    return (
      <form
        className={classNames(styles.container, className)}
        onSubmit={e => e.preventDefault()}
        autoComplete="off"
        role="search"
      >
        <div
          aria-expanded={searchResults.length > 0}
          aria-owns={`${id}-results`}
          aria-haspopup="listbox"
          className="govuk-form-group govuk-!-margin-bottom-0"
          // eslint-disable-next-line jsx-a11y/role-has-required-aria-props
          role="combobox"
        >
          <label
            className="govuk-label govuk-visually-hidden"
            htmlFor={`${id}-search`}
          >
            Find on this page
          </label>

          <input
            aria-autocomplete="list"
            aria-activedescendant={`${id}-result-${selectedResult}`}
            aria-controls={`${id}-results`}
            className="govuk-input"
            id={`${id}-search`}
            placeholder="Search this page"
            type="search"
            value={searchValue}
            ref={this.inputRef}
            onKeyDown={event => {
              if (event.key === 'ArrowUp' || event.key === 'ArrowDown') {
                this.selectNextResult(event);

                if (this.resultsRef.current) {
                  this.resultsRef.current.focus();
                }
              }
            }}
            onInput={this.onChange}
            onChange={this.onChange}
          />
          <button
            type="submit"
            className={styles.searchButton}
            value="Search this page"
            onClick={() => this.performSearch()}
          />

          {searchCompleted && (
            <div className={styles.resultsContainer}>
              <div id={`${id}-resultsLabel`} className={styles.resultsLabel}>
                Found <strong>{searchResults.length}</strong> results
              </div>

              {searchResults.length > 0 && (
                <ul
                  aria-labelledby={`${id}-resultsLabel`}
                  className={styles.results}
                  id={`${id}-results`}
                  ref={this.resultsRef}
                  role="listbox"
                  tabIndex={-1}
                  onKeyDown={event => {
                    const nextSelectedResult = this.selectNextResult(event);

                    const inputEl = this.inputRef.current;

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
                    }

                    if (event.key === 'Enter') {
                      if (searchResults[nextSelectedResult]) {
                        searchResults[nextSelectedResult].scrollIntoView();
                      }
                    }
                  }}
                >
                  {searchResults.map((result: SearchResult, index) => {
                    const key = index;

                    return (
                      // eslint-disable-next-line jsx-a11y/click-events-have-key-events
                      <li
                        aria-selected={selectedResult === index}
                        key={key}
                        id={`${id}-result-${index}`}
                        className={
                          selectedResult === index ? styles.highlighted : ''
                        }
                        onClick={result.scrollIntoView}
                        role="option"
                        ref={el => {
                          if (el) {
                            this.optionsRefs[key] = el;
                          }
                        }}
                      >
                        <div className={styles.resultHeader}>{result.text}</div>
                        <div className={styles.resultLocation}>
                          {result.location}
                        </div>
                      </li>
                    );
                  })}
                </ul>
              )}
            </div>
          )}
        </div>
      </form>
    );
  }
}

export default SearchForm;
