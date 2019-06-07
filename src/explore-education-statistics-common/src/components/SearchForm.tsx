import { openAllParentAccordionSections } from '@common/components/AccordionSection';
import { openAllParentDetails } from '@common/components/Details';
import { openAllParentTabSections } from '@common/components/TabsSection';
import findAllByText from '@common/lib/dom/findAllByText';
import findParent from '@common/lib/dom/findParent';
import findPreviousSibling from '@common/lib/dom/findPreviousSibling';
import debounce from 'lodash/debounce';
import truncate from 'lodash/truncate';
import React, { ChangeEvent, Component } from 'react';
import styles from './SearchForm.module.scss';

interface SearchResult {
  element: Element;
  scrollIntoView: () => void;
  text: string;
  location: string[];
}

interface State {
  currentlyHighlighted?: number;
  searchResults: SearchResult[];
  searchValue: string;
}

class SearchForm extends Component<{}, State> {
  public state: State = {
    searchResults: [],
    searchValue: '',
  };

  private boundPerformSearch = debounce(this.performSearch, 1000);

  private static calculateElementLocation(element: HTMLElement) {
    const location: string[] = [];

    const locationHeaderElement = findPreviousSibling(element, 'h2, h3, h4');

    if (locationHeaderElement) {
      location.unshift(locationHeaderElement.textContent || '');
    }

    const sectionContainer = findParent(element, '.govuk-accordion__section');

    if (sectionContainer) {
      const accordionHeader = sectionContainer.querySelector(
        '.govuk-accordion__section-heading',
      );

      if (accordionHeader) {
        location.unshift(accordionHeader.textContent || '');
      }
    }

    return location;
  }

  private performSearch() {
    const { searchValue } = this.state;

    if (searchValue.length <= 3) {
      return;
    }

    const elements = findAllByText(searchValue, 'p');

    const searchResults: SearchResult[] = elements.map(element => {
      const location = SearchForm.calculateElementLocation(element);

      const scrollIntoView = () => {
        openAllParentAccordionSections(element);
        openAllParentTabSections(element);
        openAllParentDetails(element);

        this.resetSearch();

        setTimeout(() => {
          element.scrollIntoView();
        });
      };

      return {
        element,
        location,
        scrollIntoView,
        text: truncate(element.textContent || ''),
      };
    });

    this.setState({ searchResults });
  }

  private resetSearch() {
    this.setState({
      currentlyHighlighted: undefined,
      searchResults: [],
      searchValue: '',
    });
  }

  private onChange = (e: ChangeEvent<HTMLInputElement>) => {
    e.persist();

    this.setState({
      searchValue: e.currentTarget.value,
      searchResults: [],
      currentlyHighlighted: undefined,
    });

    this.boundPerformSearch();
  };

  public render() {
    const { searchResults, currentlyHighlighted, searchValue } = this.state;

    return (
      <form
        className={styles.container}
        onSubmit={e => e.preventDefault()}
        autoComplete="off"
        role="search"
      >
        <div className="govuk-form-group govuk-!-margin-bottom-0">
          <label className="govuk-label govuk-visually-hidden" htmlFor="search">
            Find on this page
          </label>

          <input
            className="govuk-input"
            id="search"
            placeholder="Search this page"
            type="search"
            value={searchValue}
            onKeyDown={e => {
              let { currentlyHighlighted: nextHighlighted } = this.state;

              if (e.key === 'ArrowUp' || e.key === 'ArrowDown') {
                const direction = e.key === 'ArrowUp' ? -1 : 1;

                const len = searchResults.length;

                if (nextHighlighted !== undefined) {
                  nextHighlighted =
                    ((nextHighlighted || 0) + direction + len) % len;
                } else if (direction === -1) {
                  nextHighlighted = len - 1;
                } else {
                  nextHighlighted = 0;
                }

                this.setState({ currentlyHighlighted: nextHighlighted });
              }

              if (e.key === 'Enter') {
                if (nextHighlighted !== undefined) {
                  searchResults[nextHighlighted].scrollIntoView();

                  e.preventDefault();
                }
              }
            }}
            onInput={this.onChange}
            onChange={this.onChange}
          />
          <input
            type="submit"
            className={styles.dfeSearchButton}
            value="Search this page"
            onClick={() => this.performSearch()}
          />
        </div>
        {searchResults.length > 0 && (
          <ul className={styles.results}>
            {searchResults.map((result: SearchResult, index) => {
              const key = `search_result_${index}`;

              return (
                // eslint-disable-next-line jsx-a11y/no-noninteractive-element-interactions,jsx-a11y/click-events-have-key-events
                <li
                  key={key}
                  className={
                    currentlyHighlighted === index ? styles.highlighted : ''
                  }
                  onClick={result.scrollIntoView}
                >
                  <span className={styles.resultHeader}>{result.text}</span>
                  <span className={styles.resultLocation}>
                    {result.location.join(' > ')}
                  </span>
                </li>
              );
            })}
          </ul>
        )}
      </form>
    );
  }
}

export default SearchForm;
