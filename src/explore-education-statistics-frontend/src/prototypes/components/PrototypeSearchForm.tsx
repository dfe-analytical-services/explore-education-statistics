import debounce from 'lodash/debounce';
import React, { Component, KeyboardEvent } from 'react';
import styles from './PrototypeSearchForm.module.scss';

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

class PrototypeSearchForm extends Component<{}, State> {
  public state = {
    currentlyHighlighted: undefined,
    searchResults: [],
    searchValue: '',
  };

  private findElementsWithText(text: string) {
    const lowerCase = text.toLocaleLowerCase();
    return Array.from(document.querySelectorAll('p')).filter(e =>
      e.innerHTML.toLocaleLowerCase().includes(lowerCase),
    );
  }

  private static parentUntilClassname(element: Element, className: string) {
    let parentElement = element.parentElement;
    while (
      parentElement &&
      parentElement !== document.documentElement &&
      !parentElement.classList.contains(className)
    ) {
      parentElement = parentElement.parentElement;
    }
    return parentElement || document.documentElement;
  }

  private static findSiblingsBeforeOfElementType(
    element: Element,
    ...types: string[]
  ) {
    let sibling = element.previousElementSibling;

    const typesUpper = types.map(_ => _.toUpperCase());

    while (sibling && !typesUpper.includes(sibling.nodeName)) {
      sibling = sibling.previousElementSibling;
    }

    return sibling;
  }

  private static substring(str: string, length: number) {
    if (str) {
      if (str.length > length) {
        return `${str.substring(0, length - 1)}…`;
      }
      return str;
    }
    return '';
  }

  private performSearch() {
    if (this.state.searchValue.length <= 3) {
      return;
    }

    const elements = this.findElementsWithText(this.state.searchValue);

    const searchResults: SearchResult[] = elements.map(element => {
      let scrollIntoView: () => void;

      const potentialAccordion = PrototypeSearchForm.parentUntilClassname(
        element,
        'govuk-accordion__section',
      );

      const locationHeaderElement = PrototypeSearchForm.findSiblingsBeforeOfElementType(
        element,
        'h3',
        'h2',
        'h4',
      );

      const location = [];

      if (locationHeaderElement) {
        location.push(
          PrototypeSearchForm.substring(
            locationHeaderElement.textContent || '',
            20,
          ),
        );
      }

      const insideAccordion = potentialAccordion.classList.contains(
        'govuk-accordion__section',
      );

      if (insideAccordion) {
        scrollIntoView = () => {
          potentialAccordion.classList.add(
            'govuk-accordion__section--expanded',
          );
          element.scrollIntoView();
        };
      } else {
        scrollIntoView = () => element.scrollIntoView();
      }

      return {
        element,
        location,
        scrollIntoView,
        text: PrototypeSearchForm.substring(element.textContent || '', 30),
      };
    });

    this.setState({ searchResults });
  }

  private handleTextChange = debounce(() => {
    this.performSearch();
  }, 1000);

  private onKeyDown = (e: KeyboardEvent) => {
    if (e.key === 'ArrowUp' || e.key === 'ArrowDown') {
      const direction = e.key === 'ArrowUp' ? -1 : 1;

      const len = this.state.searchResults.length;

      let currentlyHighlighted: number | undefined = this.state
        .currentlyHighlighted;

      if (currentlyHighlighted !== undefined) {
        currentlyHighlighted =
          ((this.state.currentlyHighlighted || 0) + direction + len) % len;
      } else {
        if (direction === -1) {
          currentlyHighlighted = len - 1;
        } else {
          currentlyHighlighted = 0;
        }
      }

      this.setState({ currentlyHighlighted });
    }
  };

  public render() {
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
            value={this.state.searchValue}
            onKeyDown={this.onKeyDown}
            onChange={e => {
              e.persist();

              this.setState({
                searchValue: e.target.value,
                searchResults: [],
                currentlyHighlighted: undefined,
              });

              this.handleTextChange();
            }}
          />
          <input
            type="submit"
            className={styles.dfeSearchButton}
            value="Search this page"
            onClick={() => this.performSearch()}
          />
        </div>
        {this.state.searchResults.length > 0 ? (
          <ul className={styles.results}>
            {this.state.searchResults.map((result: SearchResult, index) => (
              // eslint-disable-next-line jsx-a11y/no-noninteractive-element-interactions,jsx-a11y/click-events-have-key-events
              <li
                key={`search_result_${index}`}
                className={
                  this.state.currentlyHighlighted === index
                    ? styles.highlighted
                    : ''
                }
                onClick={result.scrollIntoView}
              >
                <span className={styles.resultHeader}>{result.text}</span>
                <span className={styles.resultLocation}>
                  {result.location.join(' > ')}
                </span>
              </li>
            ))}
          </ul>
        ) : (
          ''
        )}
      </form>
    );
  }
}

export default PrototypeSearchForm;
