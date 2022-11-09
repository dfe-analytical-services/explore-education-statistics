import PrototypePage from '@admin/prototypes/components/PrototypePage';
import classNames from 'classnames';
import RelatedInformation from '@common/components/RelatedInformation';
import { useMobileMedia } from '@common/hooks/useMedia';
import Link from '@admin/components/Link';
import ButtonText from '@common/components/ButtonText';
import styles2 from '@common/components/PageSearchForm.module.scss';
import { releaseTypes } from '@common/services/types/releaseType';
import PrototypeSearchResult from '@admin/prototypes/components/PrototypeSearchResult';
import PrototypeSortFilters, {
  PrototypeMobileSortFilters,
} from '@admin/prototypes/components/PrototypeSortFilters';
import PrototypeFilters from '@admin/prototypes/components/PrototypeFilters';
import styles from '@admin/prototypes/PrototypePublicPage.module.scss';
import { publications, themes } from '@admin/prototypes/data/themesData';
import orderBy from 'lodash/orderBy';
import React, { useMemo, useState } from 'react';
import Button from '@common/components/Button';

const PrototypeFindStats = () => {
  const [searchInput, setSearchInput] = useState('');
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedTheme, setSelectedTheme] = useState('all-themes');
  const [selectedTopic, setSelectedTopic] = useState('');
  const [selectedReleaseType, setSelectedReleaseType] = useState(
    'all-release-types',
  );
  const [currentPage, setCurrentPage] = useState<number>(0);
  const [totalResults, setTotalResults] = useState<number>();
  const [selectedSortOrder, setSelectedSortOrder] = useState('newest');
  const [showFilters, setShowFilters] = useState(false);
  const { isMedia: isMobileMedia } = useMobileMedia();

  const generateSlug = (title: string) => {
    const slug = title.toLowerCase();
    return slug.split(' ').join('-');
  };

  const getSelectedTheme = (themeId: string) => {
    return themes.find(theme => theme.id === themeId);
  };

  const getSelectedTopic = (themeId: string, topicId: string) => {
    return themes
      .find(theme => theme.id === themeId)
      ?.topics.find(topic => topic.id === topicId);
  };

  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const spliceIntoChunks = (arr: any[], chunkSize: number) => {
    const res = [];
    const arrayToChunk = [...arr];
    while (arrayToChunk.length > 0) {
      const chunk = arrayToChunk.splice(0, chunkSize);
      res.push(chunk);
    }
    return res;
  };

  const isFiltered =
    searchQuery ||
    (selectedTheme && selectedTheme !== 'all-themes') ||
    (selectedReleaseType && selectedReleaseType !== 'all-release-types');

  const filteredPublications = useMemo(() => {
    const themeTitle = getSelectedTheme(selectedTheme)?.title;
    const topicTitle = getSelectedTopic(selectedTheme, selectedTopic)?.title;
    const filteredByThemeAndTopic =
      selectedTheme === 'all-themes'
        ? publications
        : publications.filter(publication => {
            return !selectedTopic || selectedTopic.startsWith('all-topics')
              ? publication.theme === themeTitle
              : publication.theme === themeTitle &&
                  publication.topic === topicTitle;
          });

    const filtered = filteredByThemeAndTopic.filter(publication => {
      return selectedReleaseType && selectedReleaseType !== 'all-release-types'
        ? publication.type ===
            releaseTypes[selectedReleaseType as keyof typeof releaseTypes]
        : publication;
    });

    const searched = searchQuery
      ? filtered.filter(
          pub =>
            pub.title.toLowerCase().includes(searchQuery.toLowerCase()) ||
            pub.summary.toLowerCase().includes(searchQuery.toLowerCase()),
        )
      : filtered;

    setTotalResults(searched.length);

    const orderValue = selectedSortOrder === 'alpha' ? 'title' : 'published';
    const direction = selectedSortOrder === 'newest' ? 'desc' : 'asc';

    const ordered = orderBy(searched, orderValue, direction);

    return spliceIntoChunks(ordered, 10);
  }, [
    selectedReleaseType,
    selectedSortOrder,
    searchQuery,
    selectedTheme,
    selectedTopic,
  ]);

  return (
    <div className={styles.prototypePublicPage}>
      <PrototypePage wide={false}>
        <div className="govuk-grid-row">
          <div className="govuk-grid-column-two-thirds">
            <h1 className="govuk-heading-xl">Find statistics and data</h1>
            <p className="govuk-body-l">
              Search and browse statistical summaries and download associated
              data to help you understand and analyse our range of statistics.
            </p>
          </div>
          <div className="govuk-grid-column-one-third">
            <RelatedInformation heading="Related information">
              <ul className="govuk-list">
                <li>
                  <Link
                    to="https://www.gov.uk/government/organisations/ofsted/about/statistics"
                    target="_blank"
                  >
                    Ofsted statistics
                  </Link>
                </li>
                <li>
                  <Link
                    to="https://www.education-ni.gov.uk/topics/statistics-and-research/statistics"
                    target="_blank"
                  >
                    Educational statistics for Northern Ireland
                  </Link>
                </li>
                <li>
                  <Link
                    to="https://www.gov.scot/collections/school-education-statistics/"
                    target="_blank"
                  >
                    Educational statistics for Scotland
                  </Link>
                </li>
                <li>
                  <Link
                    to="https://statswales.gov.wales/Catalogue/Education-and-Skills"
                    target="_blank"
                  >
                    Educational statistics for Wales
                  </Link>
                </li>
              </ul>
            </RelatedInformation>
          </div>
        </div>
        <div className="govuk-grid-row">
          <div className="govuk-grid-column-two-thirds">
            <form
              onSubmit={e => {
                e.preventDefault();
                setCurrentPage(0);
                setSearchQuery(searchInput);
              }}
            >
              <div
                className="govuk-form-group govuk-!-margin-bottom-9"
                style={{ position: 'relative' }}
              >
                <h2 className="govuk-label-wrapper">
                  <label
                    className="govuk-label govuk-label--m"
                    htmlFor="search"
                  >
                    Search
                  </label>
                </h2>

                <input
                  type="text"
                  id="search"
                  className="govuk-input"
                  value={searchInput}
                  onChange={e => setSearchInput(e.target.value)}
                />
                <button
                  type="submit"
                  className={styles2.searchButton}
                  value="Search"
                >
                  <span className="govuk-visually-hidden">Search</span>
                </button>
              </div>
            </form>
          </div>
        </div>

        <div className="govuk-grid-row">
          <div className="govuk-grid-column-one-third">
            <PrototypeFilters
              selectedReleaseType={selectedReleaseType}
              selectedTheme={selectedTheme}
              selectedTopic={selectedTopic}
              showFilters={showFilters}
              themes={themes}
              totalResults={totalResults}
              onCloseFilters={() => setShowFilters(false)}
              onSelectReleaseType={type => {
                setSelectedReleaseType(type);
                setCurrentPage(0);
              }}
              onSelectTheme={theme => {
                setSelectedTheme(theme);
                setCurrentPage(0);
              }}
              onSelectTopic={topic => {
                setSelectedTopic(topic);
                setCurrentPage(0);
              }}
            />
          </div>
          <div className="govuk-grid-column-two-thirds">
            <div role="region" aria-live="polite" aria-atomic="true">
              <h2 className="govuk-!-margin-bottom-2">
                {totalResults} {totalResults !== 1 ? 'results' : 'result'}
              </h2>
              <p className="govuk-visually-hidden">
                Sorted by newest publications
              </p>
              <a href="#searchResults" className="govuk-skip-link">
                Skip to search results
              </a>
            </div>

            <span className="govuk-!-margin-bottom-0">
              {!isFiltered && (
                <h3 className="govuk-heading-s">Showing all publications</h3>
              )}

              {isFiltered && (
                <div className="dfe-flex dfe-flex-wrap dfe-align-items--center">
                  <div
                    className="dfe-flex dfe-justify-content--space-between govuk-!-margin-bottom-2"
                    style={{ width: '100%' }}
                  >
                    <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
                      Filtered by
                    </h3>
                    <ButtonText
                      onClick={() => {
                        setSelectedTheme('all-themes');
                        setSelectedReleaseType('all-release-types');
                        setSelectedTopic('');
                        setSearchQuery('');
                        setSearchInput('');
                        setCurrentPage(0);
                      }}
                    >
                      Clear all filters
                    </ButtonText>
                  </div>

                  {searchQuery && (
                    <span className={styles.prototypeFilterTag}>
                      <Button
                        variant="secondary"
                        onClick={() => {
                          setSearchQuery('');
                          setSearchInput('');
                          setCurrentPage(0);
                        }}
                      >
                        ✕{' '}
                        <span className="govuk-visually-hidden">
                          {' '}
                          Clear search
                        </span>
                        {searchQuery}{' '}
                      </Button>
                    </span>
                  )}
                  {selectedTheme && selectedTheme !== 'all-themes' && (
                    <span className={styles.prototypeFilterTag}>
                      <Button
                        variant="secondary"
                        onClick={() => {
                          setSelectedTheme('all-themes');
                          setSelectedTopic('');
                          setCurrentPage(0);
                        }}
                      >
                        ✕{' '}
                        <span className="govuk-visually-hidden">
                          Clear theme{' '}
                        </span>
                        {getSelectedTheme(selectedTheme)?.title}
                      </Button>
                    </span>
                  )}
                  {selectedTopic && !selectedTopic.startsWith('all-topics') && (
                    <span
                      className={classNames(
                        styles.prototypeFilterTag,
                        styles.prototypeFilterTopic,
                      )}
                    >
                      <Button
                        variant="secondary"
                        onClick={() => {
                          setSelectedTopic(`all-topics-${selectedTheme}`);
                          setCurrentPage(0);
                        }}
                      >
                        ✕{' '}
                        <span className="govuk-visually-hidden">
                          Clear topic{' '}
                        </span>
                        {getSelectedTopic(selectedTheme, selectedTopic)?.title}
                      </Button>
                    </span>
                  )}
                  {selectedReleaseType &&
                    selectedReleaseType !== 'all-release-types' && (
                      <span className={styles.prototypeFilterTag}>
                        <Button
                          variant="secondary"
                          onClick={() => {
                            setSelectedReleaseType('all-release-types');
                            setCurrentPage(0);
                          }}
                        >
                          ✕{' '}
                          <span className="govuk-visually-hidden">
                            Clear release type{' '}
                          </span>
                          {
                            releaseTypes[
                              selectedReleaseType as keyof typeof releaseTypes
                            ]
                          }
                        </Button>
                      </span>
                    )}
                </div>
              )}
            </span>

            <hr />

            {isMobileMedia ? (
              <div className="dfe-flex dfe-justify-content--space-between dfe-align-items--center">
                <ButtonText
                  className="govuk-!-margin-bottom-1"
                  onClick={() => {
                    setShowFilters(true);
                  }}
                >
                  Filter results
                </ButtonText>
                <PrototypeMobileSortFilters
                  sortOrder={selectedSortOrder}
                  onSort={sortOrder => {
                    setSelectedSortOrder(sortOrder);
                    setCurrentPage(0);
                  }}
                />
              </div>
            ) : (
              <PrototypeSortFilters
                sortOrder={selectedSortOrder}
                onSort={sortOrder => {
                  setSelectedSortOrder(sortOrder);
                  setCurrentPage(0);
                }}
              />
            )}

            <hr />

            <div id="searchResults">
              {filteredPublications.length === 0 && <p>No matching results.</p>}
              {filteredPublications.length > 0 &&
                filteredPublications[currentPage].length > 0 &&
                filteredPublications[currentPage].map((publication, index) => (
                  <PrototypeSearchResult
                    // eslint-disable-next-line react/no-array-index-key
                    key={`publication-${index}`}
                    title={publication.title}
                    slug={generateSlug(publication.title)}
                    summary={publication.summary}
                    theme={publication.theme}
                    type={publication.type}
                    published={publication.published}
                  />
                ))}
            </div>
            {totalResults !== 0 && (
              <>
                <p>
                  Showing page {currentPage + 1} of{' '}
                  {filteredPublications.length}
                </p>
                <nav
                  className="dfe-pagination"
                  role="navigation"
                  aria-label="Pagination"
                >
                  <ul className={styles.prototypePagination}>
                    {currentPage > 0 && (
                      <li>
                        <a
                          className={styles.prototypePaginationLink}
                          href="#"
                          onClick={() => {
                            setCurrentPage(currentPage - 1);
                          }}
                        >
                          <span className={styles.prototypePaginationTitle}>
                            Previous
                          </span>
                          <span className="govuk-visually-hidden">:</span>
                          <span className="dfe-pagination__page">
                            {currentPage} of {filteredPublications.length}
                          </span>
                        </a>
                      </li>
                    )}
                    {currentPage < filteredPublications.length - 1 && (
                      <li>
                        <a
                          className={styles.prototypePaginationLink}
                          href="#"
                          onClick={() => {
                            setCurrentPage(currentPage + 1);
                          }}
                        >
                          <span className={styles.prototypePaginationTitle}>
                            Next
                          </span>
                          <span className="govuk-visually-hidden">:</span>
                          <span className="dfe-pagination__page">
                            {' '}
                            {currentPage + 2} of {filteredPublications.length}
                          </span>
                        </a>
                      </li>
                    )}
                  </ul>
                </nav>
              </>
            )}
          </div>
        </div>
      </PrototypePage>
    </div>
  );
};

export default PrototypeFindStats;
