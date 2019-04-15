import React, { ChangeEvent, Component, KeyboardEvent } from 'react';
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
  public state: State = {
    currentlyHighlighted: undefined,
    searchResults: [],
    searchValue: '',
  };

  private textChangeTimeoutHandle?: any;

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

  private performSearch(search: string) {
    const elements = this.findElementsWithText(search);

    const searchResults: SearchResult[] = elements.map(element => {
      let scrollIntoView: () => void;

      const potentialAccordion = PrototypeSearchForm.parentUntilClassname(
        element,
        'govuk-accordion__section',
      );

      const insideAccordion = potentialAccordion.classList.contains(
        'govuk-accordion__section',
      );
      const location = PrototypeSearchForm.calculateLocationOfElement(
        element,
        insideAccordion,
        potentialAccordion,
      );

      if (insideAccordion) {
        scrollIntoView = () => {
          PrototypeSearchForm.openAccordion(potentialAccordion);
          this.resetSearch();
          element.scrollIntoView();
        };
      } else {
        scrollIntoView = () => {
          this.resetSearch();
          element.scrollIntoView();
        };
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

  private static openAccordion(potentialAccordion: HTMLElement) {
    potentialAccordion.classList.add('govuk-accordion__section--expanded');
  }

  private resetSearch() {
    this.setState({
      currentlyHighlighted: undefined,
      searchResults: [],
      searchValue: '',
    });
  }

  private static calculateLocationOfElement(
    element: HTMLElement,
    insideAccordion: boolean,
    potentialAccordion: HTMLElement,
  ) {
    const location = [];

    const locationHeaderElement = PrototypeSearchForm.findSiblingsBeforeOfElementType(
      element,
      'h3',
      'h2',
      'h4',
    );

    if (locationHeaderElement) {
      location.unshift(
        PrototypeSearchForm.substring(
          locationHeaderElement.textContent || '',
          20,
        ),
      );
    }

    if (insideAccordion) {
      const accordionHeader = potentialAccordion.querySelector(
        '.govuk-accordion__section-heading button',
      );
      if (accordionHeader) {
        location.unshift(
          PrototypeSearchForm.substring(accordionHeader.textContent || '', 20),
        );
      }
    }

    return location;
  }

  private textChanged = (e: ChangeEvent<HTMLInputElement>) => {
    this.setState({ searchValue: e.target.value });

    if (this.textChangeTimeoutHandle) {
      clearTimeout(this.textChangeTimeoutHandle);
    }

    this.setState({ searchResults: [], currentlyHighlighted: undefined });

    this.textChangeTimeoutHandle = setTimeout(
      () => this.textChangeTimeout(),
      1000,
    );
  };

  private textChangeTimeout() {
    if (this.state.searchValue.length > 3) {
      this.performSearch(this.state.searchValue);
    }
  }

  private onKeyDown = (e: KeyboardEvent) => {
    if (e.keyCode === 38 || e.keyCode === 40) {
      const direction = e.keyCode === 38 ? -1 : 1;

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

    if (e.keyCode === 13) {
      if (this.state.currentlyHighlighted !== undefined) {
        this.state.searchResults[
          this.state.currentlyHighlighted
        ].scrollIntoView();

        e.preventDefault();
      }
    }
  };

  public render() {
    return (
      <form
        className={styles.container}
        onSubmit={e => e.preventDefault()}
        onKeyDown={this.onKeyDown}
        autoComplete="off"
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
            onChange={this.textChanged}
          />
          <input
            type="submit"
            className={styles.dfeSearchButton}
            value="Search this page"
            onClick={() => this.textChangeTimeout()}
          />
        </div>
        {this.state.searchResults.length > 0 ? (
          <ul className={styles.results} onKeyDown={this.onKeyDown}>
            {this.state.searchResults.map((result: SearchResult, index) => (
              <li
                key={`search_result_${index}`}
                onClick={result.scrollIntoView}
                className={
                  this.state.currentlyHighlighted === index
                    ? styles.highlighted
                    : ''
                }
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
