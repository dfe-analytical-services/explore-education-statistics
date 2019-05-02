import debounce from 'lodash/debounce';
import intersection from 'lodash/intersection';
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

class PrototypeSearchForm extends Component<{}, State> {
  public state: State = {
    searchResults: [],
    searchValue: '',
  };

  private boundPerformSearch = debounce(this.performSearch, 1000);

  private static findElementsWithText(text: string) {
    const lowerCase = text.toLocaleLowerCase();
    return Array.from(document.querySelectorAll('p')).filter(e =>
      e.innerHTML.toLocaleLowerCase().includes(lowerCase),
    );
  }

  private static parentUntilClassname(
    element: Element,
    ...className: string[]
  ) {
    let { parentElement } = element;
    while (
      parentElement &&
      parentElement !== document.documentElement &&
      intersection(className, parentElement.classList).length === 0
    ) {
      // eslint-disable-next-line prefer-destructuring
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

  private static calculateLocationOfElement(
    element: HTMLElement,
    collapsedContainer: HTMLElement,
  ) {
    const location: string[] = [];

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

    if (collapsedContainer.classList.contains('govuk-accordion__section')) {
      const accordionHeader = collapsedContainer.querySelector(
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

  private static openTab(target: Element, container: Element) {
    // find the panel in which the element lives
    const panel = PrototypeSearchForm.parentUntilClassname(
      target,
      'govuk-tabs__panel',
    );

    // find all the sections within the container
    const allSections = container.querySelectorAll('.govuk-tabs__panel');

    // get the index of the panel within the container
    const selectedPanel = Array.from(allSections).indexOf(panel);

    // only proceed if all this is valid
    if (selectedPanel >= 0) {
      // find the selected anchor
      const anchor = container
        .querySelectorAll('ul.govuk-tabs__list > li.govuk-tabs__list-item')
        .item(selectedPanel)
        .querySelector('a');

      if (anchor) {
        // Deselect all the selected tabs, make sure we found the anchor first
        container
          .querySelectorAll(
            'ul.govuk-tabs__list > li.govuk-tabs__list-item a.govuk-tabs__tab--selected',
          )
          .forEach(selectedElement => {
            selectedElement.classList.remove('govuk-tabs__tab--selected');
            selectedElement.setAttribute('aria-selected', 'false');
            selectedElement.setAttribute('tabIndex', '-1');
          });

        // make the selected tab selected
        anchor.classList.add('govuk-tabs__tab--selected');
        anchor.setAttribute('aria-selected', 'true');
        anchor.removeAttribute('tabIndex');

        // hide all the panels
        Array.from(
          container.querySelectorAll(
            '.govuk-tabs__panel:not(.govuk-tabs__panel--hidden)',
          ),
        ).forEach(hiddenElement =>
          hiddenElement.classList.add('govuk-tabs__panel--hidden'),
        );

        // show the selected panel
        panel.classList.remove('govuk-tabs__panel--hidden');

        // click the anchor to get its component to update any internal state
        anchor.dispatchEvent(
          new MouseEvent('click', {
            view: window,
            bubbles: true,
            cancelable: true,
            buttons: 1,
          }),
        );
      }
    }
  }

  private static openDetails(collapsedContainer: Element) {
    collapsedContainer.setAttribute('open', '');
    const textEl = collapsedContainer.querySelector('.govuk-details__text');
    if (textEl) {
      textEl.setAttribute('aria-hidden', 'false');
      textEl.setAttribute('style', '');
    }
    const summary = collapsedContainer.querySelector('summary');
    if (summary) {
      summary.setAttribute('aria-expanded', 'true');
    }
  }

  private static openAccordion(potentialAccordion: HTMLElement) {
    potentialAccordion.classList.add('govuk-accordion__section--expanded');
  }

  private performSearch() {
    const { searchValue } = this.state;

    if (searchValue.length <= 3) {
      return;
    }

    const elements = PrototypeSearchForm.findElementsWithText(searchValue);

    const searchResults: SearchResult[] = elements.map(element => {
      const accordionContainer = PrototypeSearchForm.parentUntilClassname(
        element,
        'govuk-accordion__section',
      );

      const detailsContainer = PrototypeSearchForm.parentUntilClassname(
        element,
        'govuk-details',
      );

      const tabContainer = PrototypeSearchForm.parentUntilClassname(
        element,
        'govuk-tabs',
      );

      const location = PrototypeSearchForm.calculateLocationOfElement(
        element,
        accordionContainer,
      );

      const scrollIntoView = () => {
        if (accordionContainer.classList.contains('govuk-accordion__section')) {
          PrototypeSearchForm.openAccordion(accordionContainer);
        }

        if (detailsContainer.classList.contains('govuk-details')) {
          PrototypeSearchForm.openDetails(detailsContainer);
        }

        if (tabContainer.classList.contains('govuk-tabs')) {
          PrototypeSearchForm.openTab(element, tabContainer);
        }

        this.resetSearch();
        element.scrollIntoView();
      };

      return {
        element,
        location,
        scrollIntoView,
        text: PrototypeSearchForm.substring(element.textContent || '', 30),
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

export default PrototypeSearchForm;
