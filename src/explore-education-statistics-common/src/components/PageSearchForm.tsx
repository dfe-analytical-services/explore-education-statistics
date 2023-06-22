import {
  accordionSectionClasses,
  openAllParentAccordionSections,
} from '@common/components/AccordionSection';
import { openAllParentDetails } from '@common/components/Details';
import FormComboBox from '@common/components/form/FormComboBox';
import { openAllParentTabSections } from '@common/components/TabsSection';
import useDebouncedCallback from '@common/hooks/useDebouncedCallback';
import delay from '@common/utils/delay';
import findAllByText from '@common/utils/dom/findAllByText';
import findAllParents from '@common/utils/dom/findAllParents';
import findPreviousSibling from '@common/utils/dom/findPreviousSibling';
import classNames from 'classnames';
import React, { ReactNode, useState } from 'react';
import Highlighter from 'react-highlight-words';
import styles from './PageSearchForm.module.scss';

interface SearchResult {
  element: Element;
  scrollIntoView: () => void;
  text: ReactNode;
  location: string;
}

export interface PageSearchFormProps {
  className?: string;
  elementSelectors?: string[];
  id?: string;
  inputLabel: string;
  minInput?: number;
  rootElement?: string;
  onSearch?: (s: string) => void;
}

const defaultSelectors = ['p', 'li > strong', 'h2', 'h3', 'h4'];

const PageSearchForm = ({
  className,
  elementSelectors = defaultSelectors,
  id = 'pageSearchForm',
  inputLabel,
  minInput = 3,
  rootElement = '#main-content',
  onSearch,
}: PageSearchFormProps) => {
  const [searchResults, setSearchResults] = useState<SearchResult[]>([]);
  const [searchComplete, setSearchComplete] = useState(false);

  const getLocationText = (element: HTMLElement): string => {
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
  };

  const [search] = useDebouncedCallback((value: string) => {
    const isAcronym = value === value.toUpperCase() && value.length > 1;

    if (!isAcronym && value.length < minInput) {
      resetSearch();
      return;
    }

    if (typeof onSearch === 'function') {
      onSearch(value);
    }

    const elements = findAllByText(
      isAcronym ? value : value.toLocaleLowerCase(),
      elementSelectors.join(', '),
      {
        rootElement: (document.querySelector(rootElement) ||
          document) as Element,
        useLowerCase: !isAcronym,
      },
    );

    const nextSearchResults = elements.map(element => {
      const location = getLocationText(element);

      return {
        element,
        location,
        text: (
          <Highlighter
            searchWords={[value]}
            textToHighlight={element.textContent || ''}
          />
        ),
        scrollIntoView: async () => {
          openAllParentAccordionSections(element);

          // Add delays as we need to allow time for the
          // DOM to update before we do the next change.
          await delay();
          openAllParentTabSections(element);

          await delay();
          openAllParentDetails(element);

          resetSearch();

          await delay();

          // Bit of a hack, but hopefully screen readers will
          // still change focus to the selected element even if we
          // proceed to remove the tabindex shortly afterwards
          // TODO: Verify this works
          const previousTabIndex = element.getAttribute('tabIndex');

          if (!previousTabIndex) {
            element.setAttribute('tabIndex', '-1');
          }

          element.scrollIntoView({
            behavior: 'smooth',
            block: 'start',
          });
          element.focus();

          // Long delay as we want to let screen reader
          // finish reading before we actually remove
          // the tabindex (just in case).
          await delay(5000);

          if (previousTabIndex) {
            element.setAttribute('tabIndex', previousTabIndex);
          } else {
            element.removeAttribute('tabIndex');
          }
        },
      };
    });

    setSearchResults(nextSearchResults);
    setSearchComplete(true);
  }, 1000);

  const resetSearch = () => {
    setSearchResults([]);
    setSearchComplete(false);
  };

  return (
    <form
      className={classNames(
        styles.container,
        className,
        'govuk-!-margin-bottom-6',
      )}
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
          type: 'search',
        }}
        inputLabel={inputLabel}
        listBoxLabelId={`${id}-resultsLabel`}
        listBoxLabel={() => (
          <div
            id={`${id}-resultsLabel`}
            className={styles.resultsLabel}
            aria-live="polite"
            aria-atomic
          >
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
                      <div className={styles.resultLocation} aria-hidden>
                        {result.location}
                      </div>
                    )}
                  </>
                );
              })
            : undefined
        }
        onInputChange={event => {
          search(event.target.value);
        }}
        onSelect={selectedItem => {
          if (searchResults[selectedItem]) {
            const selectedResult = searchResults[selectedItem];
            selectedResult.scrollIntoView();
          }
        }}
      />
    </form>
  );
};

export default PageSearchForm;
