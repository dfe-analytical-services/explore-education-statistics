// import FormProvider from '@common/components/form/FormProvider';
// import { Form, FormFieldTextInput } from '@common/components/form';
import SearchIcon from '@common/components/SearchIcon';
import VisuallyHidden from '@common/components/VisuallyHidden';
import React from 'react';
import Autocomplete from 'accessible-autocomplete/react';
// import publicationQueries from '@frontend/queries/azurePublicationQueries';
import styles from '@common/components/form/FormSearchBar.module.scss';
// import { useQuery } from '@tanstack/react-query';
import { useRouter } from 'next/router';
import { truncate } from 'lodash';
import azurePublicationService, {
  AzurePublicationSuggestResult,
} from '@frontend/services/azurePublicationService';
import { createAzurePublicationSuggestRequest } from '@frontend/modules/find-statistics/utils/createAzurePublicationListRequest';
import logger from '@common/services/logger';

interface Props {
  label?: string;
  // value?: string;
  onSubmit: (value: string) => void;
}

export default function SearchForm({
  label = 'Search',
  // value: initialValue = '',
  onSubmit,
}: Props) {
  const router = useRouter();

  // const { data: suggestions } = useQuery({
  //   ...publicationQueries.suggestPublications(router.query, term),
  //   keepPreviousData: true,
  //   staleTime: 60000,
  // });

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
          // defaultValue={initialValue}
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
