import {
  accordionSectionClasses,
  openAllParentAccordionSections,
} from '@common/components/AccordionSection';
import { openAllParentDetails } from '@common/components/Details';
import FormComboBox from '@common/components/form/FormComboBox';
import { openAllParentTabSections } from '@common/components/TabsSection';
import findAllByText from '@common/lib/dom/findAllByText';
import findParent from '@common/lib/dom/findParent';
import findPreviousSibling from '@common/lib/dom/findPreviousSibling';
import classNames from 'classnames';
import debounce from 'lodash/debounce';
import React, { Component, ReactNode } from 'react';
import Highlighter from 'react-highlight-words';
import styles from './PageSearchForm.module.scss';

interface SearchResult {
  element: Element;
  scrollIntoView: () => void;
  text: ReactNode;
  location: string;
}

interface Props {
  className?: string;
  elementSelectors: string[];
  id: string;
}

interface State {
  searchResults: SearchResult[];
  searchComplete: boolean;
}

class PageSearchForm extends Component<Props, State> {
  public state: State = {
    searchResults: [],
    searchComplete: false,
  };

  public static defaultProps = {
    elementSelectors: ['p', 'li > strong', 'h2', 'h3', 'h4'],
    id: 'pageSearchForm',
  };

  private debouncedSearch = debounce(this.search, 1000);

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

  private resetSearch() {
    this.setState({ searchResults: [], searchComplete: false });
  }

  private search(value: string) {
    const { elementSelectors } = this.props;

    if (value.length <= 3) {
      return;
    }

    const elements = findAllByText(value, elementSelectors.join(', '));

    const searchResults = elements.map(element => {
      const location = PageSearchForm.getLocationText(element);

      return {
        element,
        location,
        text: (
          <Highlighter
            searchWords={[value]}
            textToHighlight={element.textContent || ''}
          />
        ),
        scrollIntoView: () => {
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
        },
      };
    });

    this.setState({ searchResults, searchComplete: true });
  }

  public render() {
    const { className, id } = this.props;
    const { searchResults, searchComplete } = this.state;

    return (
      <form
        className={classNames(styles.container, className)}
        onSubmit={e => e.preventDefault()}
        autoComplete="off"
        role="search"
      >
        <FormComboBox
          classes={{
            inputLabel: 'govuk-visually-hidden',
          }}
          id={id}
          inputProps={{
            placeholder: 'Search this page',
          }}
          inputLabel="Find on this page"
          afterInput={({ value }) => (
            <button
              type="submit"
              className={styles.searchButton}
              value="Search this page"
              onClick={() => this.search(value)}
            />
          )}
          listBoxLabelId={`${id}-resultsLabel`}
          listBoxLabel={() => (
            <div id={`${id}-resultsLabel`} className={styles.resultsLabel}>
              Found <strong>{searchResults.length}</strong> results
            </div>
          )}
          options={
            searchComplete
              ? searchResults.map(result => {
                  return (
                    <>
                      <div className={styles.resultHeader}>{result.text}</div>
                      <div className={styles.resultLocation}>
                        {result.location}
                      </div>
                    </>
                  );
                })
              : undefined
          }
          onInputChange={event => {
            this.debouncedSearch(event.target.value);
          }}
          onSelect={selectedItem => {
            if (searchResults[selectedItem]) {
              searchResults[selectedItem].scrollIntoView();
            }
          }}
        />
      </form>
    );
  }
}

export default PageSearchForm;
