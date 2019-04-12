import React, { ChangeEvent, Component, KeyboardEvent } from 'react';
import styles from './PrototypeSearchForm.module.scss';

interface SearchResult {
  element: Element;
  scrollIntoView: () => void;
  text: string;
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

  private textChangeTimeoutHandle?: any;

  private findElementsWithText(text: string) {
    return Array.from(document.querySelectorAll('p')).filter(e =>
      e.innerHTML.includes(text),
    );
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
        scrollIntoView,
        text: (element.textContent || '').substring(0, 10),
      };
    });

    this.setState({ searchResults });
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
                  Location &gt; Location{' '}
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
