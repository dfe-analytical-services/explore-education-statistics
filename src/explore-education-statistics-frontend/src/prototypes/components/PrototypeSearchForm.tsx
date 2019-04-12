import React, { ChangeEvent, Component } from 'react';
import styles from './PrototypeSearchForm.module.scss';

interface SearchResult {
  element: Element;
  scrollIntoView: () => void;
  text: string;
}

interface State {
  searchResults: SearchResult[];
  searchValue: string;
}

class PrototypeSearchForm extends Component<{}, State> {
  public state = {
    searchResults: [],
    searchValue: '',
  };

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

  private textChangeTimeoutHandle?: any;

  private textChanged = (e: ChangeEvent<HTMLInputElement>) => {
    this.setState({ searchValue: e.target.value });

    if (this.textChangeTimeoutHandle) {
      clearTimeout(this.textChangeTimeoutHandle);
    }

    this.textChangeTimeoutHandle = setTimeout(
      () => this.textChangeTimeout(),
      1000,
    );
  };

  private textChangeTimeout() {
    this.performSearch(this.state.searchValue);
  }

  public render() {
    return (
      <form className={styles.container}>
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
          />
        </div>
        {this.state.searchResults.length > 0 ? (
          <ul className={styles.results}>
            {this.state.searchResults.map((result: SearchResult, index) => (
              <li key={`search_result_${index}`}>
                <a onClick={result.scrollIntoView}>{result.text}</a>
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
