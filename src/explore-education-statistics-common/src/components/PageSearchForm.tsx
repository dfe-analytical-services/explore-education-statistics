import {
  accordionSectionClasses,
  openAllParentAccordionSections,
} from '@common/components/AccordionSection';
import { openAllParentDetails } from '@common/components/Details';
import FormComboBox from '@common/components/form/FormComboBox';
import { openAllParentTabSections } from '@common/components/TabsSection';
import findAllByText from '@common/lib/dom/findAllByText';
import findAllParents from '@common/lib/dom/findAllParents';
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
  minInput: number;
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
    minInput: 3,
  };

  private static getLocationText(element: HTMLElement): string {
    const location: string[] = [];

    const prependNearestHeading = (currentElement: HTMLElement) => {
      const elementTag = currentElement.tagName.toLowerCase();

      if (['h2', 'h3', 'h4'].includes(elementTag)) {
        return;
      }

      const headingEl = findPreviousSibling(currentElement, `h2, h3, h4`);

      if (headingEl) {
        location.unshift(headingEl.textContent || '');
      }
    };

    prependNearestHeading(element);

    findAllParents(element, '*').forEach(parent => {
      prependNearestHeading(parent);

      if (
        parent.parentElement &&
        parent.parentElement.classList.contains(accordionSectionClasses.section)
      ) {
        const accordionHeader = parent.parentElement.querySelector(
          `.${accordionSectionClasses.sectionButton}`,
        );

        if (accordionHeader) {
          location.unshift(accordionHeader.textContent || '');
        }
      }
    });

    return location.join(' > ');
  }

  private search = (value: string) => {
    const { elementSelectors, minInput } = this.props;

    if (value.length < minInput) {
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
            // Bit of a hack, but hopefully screen readers will
            // still change focus to the selected element even if we
            // proceed to remove the tabindex shortly afterwards
            // TODO: Verify this works
            const previousTabIndex = element.getAttribute('tabindex');

            if (!previousTabIndex) {
              element.setAttribute('tabindex', '-1');
            }

            element.focus();
            element.scrollIntoView({
              behavior: 'smooth',
              block: 'start',
            });

            if (previousTabIndex) {
              element.setAttribute('tabindex', previousTabIndex);
            } else {
              element.removeAttribute('tabindex');
            }
          });
        },
      };
    });

    this.setState({ searchResults, searchComplete: true });
  };

  // eslint-disable-next-line react/sort-comp
  private debouncedSearch = debounce(this.search, 1000);

  private resetSearch() {
    this.setState({ searchResults: [], searchComplete: false });
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
              Found <strong>{searchResults.length}</strong>
              {` ${searchResults.length === 1 ? 'result' : 'results'}`}
            </div>
          )}
          options={
            searchComplete
              ? searchResults.map(result => {
                  return (
                    <>
                      <div className={styles.resultHeader}>{result.text}</div>
                      {result.location && (
                        <div className={styles.resultLocation}>
                          {result.location}
                        </div>
                      )}
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
