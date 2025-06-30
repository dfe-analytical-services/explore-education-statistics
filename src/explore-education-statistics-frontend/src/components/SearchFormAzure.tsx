import SearchIcon from '@common/components/SearchIcon';
import VisuallyHidden from '@common/components/VisuallyHidden';
import styles from '@common/components/form/FormSearchBar.module.scss';
import logger from '@common/services/logger';
import { createAzurePublicationSuggestRequest } from '@frontend/modules/find-statistics/utils/createAzurePublicationListRequest';
import azurePublicationService, {
  AzurePublicationSuggestResult,
} from '@frontend/services/azurePublicationService';
import React, { useEffect, useRef } from 'react';
import Autocomplete from 'accessible-autocomplete/react';
import { truncate } from 'lodash';
import { useRouter } from 'next/router';

interface Props {
  label?: string;
  onSubmit: (value: string) => void;
}

export default function SearchForm({ label = 'Search', onSubmit }: Props) {
  const router = useRouter();

  const wrapper = useRef<HTMLFormElement>(null);

  // The accessible-autocomplete component generates a text input element.
  // We need to access the created elements to:
  // 1) change input type to 'search'
  // 2) add form submission when user presses 'Enter'
  useEffect(() => {
    const autocompleteInput = wrapper.current?.querySelector(
      '#search',
    ) as HTMLInputElement;
    if (!autocompleteInput) {
      return undefined;
    }

    // 1
    autocompleteInput.setAttribute('type', 'search');

    function handleEnter(evt: KeyboardEvent) {
      const dropdownVisible =
        autocompleteInput.getAttribute('aria-expanded') === 'true';
      if (dropdownVisible && evt.key && evt.key === 'Enter') {
        const searchTerm = new FormData(wrapper.current!).get(
          'search',
        ) as string;
        onSubmit(searchTerm || '');
      }
    }

    // 2) The accessible-autocomplete component has an edge case where when the menu is visible, it
    // prevents default on the Enter key event, even if the user hasn't put keyboard focus on a
    // suggestion. This results in a scenario where the user types something, does _not_ interact
    // with the autocomplete menu at all, and then hits Enter to try to submit the form - but it
    // isn't submitted.
    // So let's call our onSubmit when user presses Enter.
    autocompleteInput.addEventListener('keyup', handleEnter);

    return () => {
      autocompleteInput.removeEventListener('keyup', handleEnter);
    };
  }, [onSubmit]);

  const fetchResults = (
    enteredText: string,
    populateResults: (suggestions: AzurePublicationSuggestResult[]) => void,
  ) => {
    azurePublicationService
      .suggestPublications(
        createAzurePublicationSuggestRequest(router.query, enteredText),
      )
      .then(populateResults)
      .catch(error => {
        populateResults([]);
        logger.error(error);
      });
  };

  const suggestionTemplate = (
    result: AzurePublicationSuggestResult,
  ) => `<p class="autocomplete__option-item">
          <span class="autocomplete__option-title">
            ${result.title}
          </span>
          <span class="autocomplete__option-summary">
            ${truncate(result.summary, { length: 140 })}
          </span>
        </p>`;

  return (
    <form
      id="searchForm"
      onSubmit={event => {
        event.preventDefault();
        const searchTerm = new FormData(event.currentTarget).get(
          'search',
        ) as string;
        onSubmit(searchTerm || '');
      }}
      ref={wrapper}
    >
      <label htmlFor="search" className="govuk-label govuk-label--m">
        {label}
      </label>
      <div className="autocomplete__item-wrap">
        <Autocomplete
          id="search"
          name="search"
          minLength={3}
          source={fetchResults}
          templates={{
            inputValue: result => result?.title,
            suggestion: suggestionTemplate,
          }}
          confirmOnBlur={false}
          showNoOptionsFound={false}
          onConfirm={result => {
            if (result) {
              return router.push(
                `/find-statistics/${result.publicationSlug}/${result.releaseSlug}`,
              );
            }
            return null;
          }}
          tNoResults={() => 'No search suggestions found'}
          tAssistiveHint={() =>
            'When search suggestions are available use up and down arrows to review and enter to select. Touch device users, explore by touch or with swipe gestures.'
          }
        />
        <button
          type="submit"
          className={`${styles.button} autocomplete__submit-button`}
        >
          <SearchIcon className={styles.icon} />
          <VisuallyHidden>Search</VisuallyHidden>
        </button>
      </div>
    </form>
  );
}
