import PrototypePage from '@admin/prototypes/components/PrototypePage';
import FormSearchBar from '@common/components/form/FormSearchBar';
import RelatedInformation from '@common/components/RelatedInformation';
import classNames from 'classnames';
import Link from '@admin/components/Link';
import { releaseTypes } from '@common/services/types/releaseType';
import PrototypeSortFilters from '@admin/prototypes/components/PrototypeSortFilters';
import styles from '@admin/prototypes/PrototypePublicPage.module.scss';
import { publications, themes } from '@admin/prototypes/data/newThemesData';
import orderBy from 'lodash/orderBy';
import React, { useMemo, useState } from 'react';
import Button from '@common/components/Button';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import ToggleMoreDetails from '@admin/prototypes/components/PrototypeToggleMoreDetails';

const PrototypeDataCatalogue = () => {
  const params = new URLSearchParams(window.location.search);
  const urlTheme = params.get('theme');
  const urlPublication = params.get('publication');
  const urlSource = params.get('source');
  const urlDataType = params.get('dataType');
  const urlCompactList = params.get('compactList');

  const [fullList] = useState(true);
  const [listCompact, setListCompact] = useState(
    !urlCompactList || urlCompactList === 'true',
  );
  const [searchQuery] = useState('');
  const [selectedTheme, setSelectedTheme] = useState(
    urlTheme === 'fe' ? 'Further education' : 'All themes',
  );
  const [selectedPublication, setSelectedPublication] = useState(
    urlPublication === 'traineeships'
      ? 'Apprenticeships and traineeships'
      : 'All publications',
  );
  const selectedReleaseType = 'all-release-types';
  // const [selectedReleaseType, setSelectedReleaseType] = useState(
  //  'all-release-types',
  // );
  const [currentPage, setCurrentPage] = useState<number>(0);
  const [totalResults, setTotalResults] = useState<number>();
  const [selectedSortOrder, setSelectedSortOrder] = useState('newest');
  const sourcePublication = urlSource === 'publicationPage';
  // const [sourcePublication, setSourcePublication] = useState(
  //   urlSource === 'publicationPage',
  // );
  const latestRelease = 'Academic year 2021/22';
  const [selectedRelease, setSelectedRelease] = useState(latestRelease);
  const [dataType, setDataType] = useState(
    urlDataType === 'api' ? 'api' : 'csv',
  );

  const getSelectedTheme = (themeId: string) => {
    return themes.find(theme => theme.id === themeId);
  };

  const themeSelect = document.getElementById('theme') as HTMLSelectElement;
  const publicationSelect = document.getElementById(
    'publication',
  ) as HTMLSelectElement;

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

  /* eslint-disable-next-line */
  const filteredPublications = useMemo(() => {
    const themeTitle = getSelectedTheme(selectedTheme)?.title;
    const filteredByTheme =
      selectedTheme === 'all-themes'
        ? publications
        : publications.filter(publication => publication.theme === themeTitle);

    const filtered = filteredByTheme.filter(publication => {
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

    setTotalResults(selectedTheme === 'Further education' ? 100 : 32);

    const orderValue = selectedSortOrder === 'alpha' ? 'title' : 'published';
    const direction = selectedSortOrder === 'newest' ? 'desc' : 'asc';

    const ordered = orderBy(searched, orderValue, direction);

    return spliceIntoChunks(ordered, 10);
  }, [selectedReleaseType, selectedSortOrder, searchQuery, selectedTheme]);

  return (
    <div
      className={classNames(styles.prototypePublicPage, [
        sourcePublication && styles.prototypeHideBreadcrumb,
      ])}
    >
      <PrototypePage
        wide={false}
        breadcrumbs={[
          {
            name: 'Data catalogue',
            link: './data-catalog?theme=clear',
          },
          { name: selectedTheme, link: `./data-catalog?theme=fe` },
          {
            name: selectedPublication,
            link: './data-catalog?theme=fe&publication=traineeships',
          },
          { name: selectedRelease, link: './data-catalog' },
        ]}
      >
        {sourcePublication && (
          <div className={styles.prototypeBackLink}>
            <Link to="./releaseData#exploreData" back>
              Back to apprenticeships and traineeships, academic year 2021/22
            </Link>
          </div>
        )}
        <div className="govuk-grid-row">
          <div className="govuk-grid-column-two-thirds">
            <h1 className={classNames('govuk-heading-xl')}>Data catalogue</h1>
            <p className="govuk-body-l">
              Find and download data sets with associated guidance files.
            </p>{' '}
          </div>
          <div className="govuk-grid-column-one-third">
            <RelatedInformation heading="Related information">
              <ul className="govuk-list">
                <li>
                  <Link to="#" target="_blank">
                    Find statistics and data
                  </Link>
                </li>
                <li>
                  <Link to="#" target="_blank">
                    Glossary
                  </Link>
                </li>
              </ul>
            </RelatedInformation>
          </div>
        </div>
        <hr />

        <div className="dfe-flex dfe-flex-wrap">
          <div className={styles.stickyWidthOneThird}>
            <div className={styles.stickyLinksContainer}>
              <form action="#" className="govuk-form">
                <div
                  className="govuk-form-group govuk-!-margin-bottom-6"
                  style={{ position: 'relative' }}
                >
                  <FormSearchBar
                    id="searchDataSewts"
                    label="Search data sets"
                    min={2}
                    name="search"
                    value=""
                  />
                </div>
                <fieldset className="govuk-fieldset">
                  <legend className="govuk-fieldset__legend govuk-fieldset__legend--m">
                    Filter data sets
                  </legend>

                  <div
                    className="govuk-form-group govuk-!-margin-bottom-6"
                    style={{ position: 'relative' }}
                  >
                    <h2 className="govuk-label-wrapper">
                      <label
                        className="govuk-label govuk-label--s"
                        htmlFor="theme"
                      >
                        Theme
                      </label>
                    </h2>
                    <select
                      className="govuk-select"
                      id="theme"
                      onBlur={_ => {}}
                      /* eslint-disable-next-line */
                      onChange={e => {
                        params.delete('theme');
                        setSelectedTheme(e.target.value);
                        setSelectedPublication('All publications');
                      }}
                    >
                      <option value={selectedTheme} selected>
                        {selectedTheme}
                      </option>
                      <option value="All themes">All themes</option>
                      <option value="Children's social care">
                        Children's social care
                      </option>
                      <option value="COVID-19">COVID-19</option>
                      <option value="Destination of pupils and students">
                        Destination of pupils and students
                      </option>
                      <option value="Early years">Early years</option>
                      <option value="Finance and funding">
                        Finance and funding
                      </option>
                      <option value="Further education">
                        Further education
                      </option>
                      <option value="Higher education">Higher education</option>
                      <option value="Pupils and schools">
                        Pupils and schools
                      </option>
                      <option value="School and college outcomes and performance">
                        School and college outcomes and performance
                      </option>
                      <option value="Teachers and school workforce">
                        Teachers and school workforce
                      </option>
                      <option value="UK education and training statistics">
                        UK education and training statistics
                      </option>
                    </select>
                  </div>
                  <div
                    className="govuk-form-group govuk-!-margin-bottom-6"
                    style={{ position: 'relative' }}
                  >
                    <h2 className="govuk-label-wrapper">
                      <label
                        className="govuk-label govuk-label--s"
                        htmlFor="pubilication"
                      >
                        Publication
                      </label>
                    </h2>

                    <select
                      className="govuk-select"
                      id="publication"
                      onBlur={_ => {}}
                      /* eslint-disable-next-line */
                      onChange={e => {
                        setSelectedPublication(e.target.value);
                        setTotalResults(32);
                      }}
                      /* eslint-disable-next-line react/jsx-props-no-spreading */
                      {...(selectedTheme !== 'Further education' && {
                        disabled: true,
                      })}
                    >
                      <option value={selectedPublication} selected>
                        {selectedPublication}
                      </option>
                      <option value="All publications">All publications</option>
                      <option value="Apprenticeships and traineeships">
                        Apprenticeships and traineeships
                      </option>
                      <option value="Apprenticeships in England by industry characteristics">
                        Apprenticeships in England by industry characteristics
                      </option>
                      <option value="Career pathways: post-16 qualifications held by employees">
                        Career pathways: post-16 qualifications held by
                        employees
                      </option>
                      <option
                        value="Detailed destinations of 16 to 18 year olds in Further
                  Education"
                      >
                        Detailed destinations of 16 to 18 year olds in Further
                        Education
                      </option>
                      <option
                        value="FE learners going into employment and learning destinations by
                  local authority district"
                      >
                        FE learners going into employment and learning
                        destinations by local authority district
                      </option>
                      <option value="Further education and skills">
                        Further education and skills
                      </option>
                      <option value="Further education: outcome-based success measures">
                        Further education: outcome-based success measures
                      </option>
                      <option value="Further education skills index">
                        Further education skills index
                      </option>
                      <option value="Skills Bootcamps outcomes">
                        Skills Bootcamps outcomes
                      </option>
                      <option value="Skills Bootcamps starts">
                        Skills Bootcamps starts
                      </option>
                    </select>
                  </div>

                  {selectedPublication ===
                    'Apprenticeships and traineeships' && (
                    <div
                      className="govuk-form-group govuk-!-margin-bottom-6"
                      style={{ position: 'relative' }}
                    >
                      <h2 className="govuk-label-wrapper">
                        <label
                          className="govuk-label govuk-label--s"
                          htmlFor="pubilication"
                        >
                          Related release
                        </label>
                      </h2>
                      <select
                        className="govuk-select"
                        id="release"
                        /* eslint-disable-next-line react/jsx-props-no-spreading */
                        {...(selectedPublication !==
                          'Apprenticeships and traineeships' && {
                          disabled: true,
                        })}
                        onBlur={_ => {}}
                        // eslint-disable-next-line jsx-a11y/no-onchange
                        onChange={e => {
                          setSelectedRelease(e.target.value);
                        }}
                      >
                        <option
                          value={selectedRelease}
                          selected={selectedRelease !== ''}
                        >
                          {selectedRelease}
                        </option>
                        <option value="Academic year 2021/22">
                          Academic year 2021/22
                        </option>
                        <option value="Academic year 2020/21">
                          Academic year 2020/21
                        </option>
                        <option value="Academic year 2019/20">
                          Academic year 2019/20
                        </option>
                        <option value="Academic year 2019/20">
                          Academic year 2018/19
                        </option>
                      </select>
                    </div>
                  )}
                  <div className="govuk-!-margin-top-3">
                    <a
                      href="#"
                      onClick={e => {
                        e.preventDefault();
                        setSelectedTheme('All themes');
                        setSelectedPublication('All publications');
                        if (themeSelect) {
                          themeSelect.selectedIndex = 1;
                        }
                        if (publicationSelect) {
                          publicationSelect.selectedIndex = 1;
                        }
                      }}
                    >
                      Clear filters
                    </a>
                  </div>
                </fieldset>
              </form>

              <hr />
              <div className="govuk-radios govuk-radios--small">
                <fieldset className="govuk-fieldset govuk-!-margin-top-6">
                  <legend className="govuk-heading-s govuk-!-margin-bottom-0 ">
                    Type of data
                  </legend>
                  <div className="govuk-radios__item">
                    <input
                      type="radio"
                      className="govuk-radios__input"
                      name="filetype"
                      id="filetype-2"
                      value="csv"
                      checked={dataType === 'csv'}
                      onChange={e => {
                        setDataType(e.target.value);
                      }}
                    />
                    <label
                      className="govuk-label govuk-radios__label"
                      htmlFor="filetype-2"
                    >
                      Data downloads (CSV)
                    </label>
                  </div>
                  <div className="govuk-radios__item">
                    <input
                      type="radio"
                      className="govuk-radios__input"
                      name="filetype"
                      id="filetype-1"
                      value="api"
                      checked={dataType === 'api'}
                      onChange={e => {
                        setDataType(e.target.value);
                      }}
                    />
                    <label
                      className="govuk-label govuk-radios__label"
                      htmlFor="filetype-1"
                    >
                      API data sets
                    </label>
                  </div>
                </fieldset>
              </div>

              <div className="govuk-radios govuk-radios--small">
                <fieldset className="govuk-fieldset govuk-!-margin-top-6">
                  <legend className="govuk-heading-s govuk-!-margin-bottom-0 ">
                    Result display
                  </legend>
                  <div className="govuk-radios__item">
                    <input
                      type="radio"
                      className="govuk-radios__input"
                      name="resultType"
                      id="resultType-1"
                      value="compact"
                      checked={listCompact}
                      onChange={() => {
                        setListCompact(true);
                      }}
                    />
                    <label
                      className="govuk-label govuk-radios__label"
                      htmlFor="resultType-1"
                    >
                      Show compact results
                    </label>
                  </div>
                  <div className="govuk-radios__item">
                    <input
                      type="radio"
                      className="govuk-radios__input"
                      name="resultType"
                      id="resultType-2"
                      value="full"
                      checked={!listCompact}
                      onChange={() => {
                        setListCompact(false);
                      }}
                    />
                    <label
                      className="govuk-label govuk-radios__label"
                      htmlFor="resultType-2"
                    >
                      Show expanded results
                    </label>
                  </div>
                </fieldset>
              </div>
            </div>
          </div>

          <div className="govuk-grid-column-two-thirds">
            {selectedPublication === 'All publications' && (
              <div role="region" aria-live="polite" aria-atomic="true">
                {selectedPublication === 'All publications' &&
                  selectedTheme === 'All themes' && (
                    <>
                      <h2 className="govuk-!-margin-bottom-3">
                        {dataType === 'csv'
                          ? '500 data sets'
                          : '120 API data sets'}
                      </h2>
                      <p>Page 1 of X, showing all available data sets</p>
                    </>
                  )}

                {selectedPublication === 'All publications' &&
                  selectedTheme === 'Further education' && (
                    <>
                      <h2 className="govuk-!-margin-bottom-3">
                        {dataType === 'csv'
                          ? '90 data sets'
                          : '30 API data sets'}
                      </h2>
                      <p>Page 1 of X, filtered by:</p>
                    </>
                  )}

                {selectedTheme && selectedTheme !== 'All themes' && (
                  <span className={styles.prototypeFilterTag}>
                    <Button
                      variant="secondary"
                      onClick={() => {
                        setSelectedTheme('All themes');
                        setSelectedPublication('All publications');
                        if (themeSelect) {
                          themeSelect.selectedIndex = 1;
                        }
                        if (publicationSelect) {
                          publicationSelect.selectedIndex = 1;
                        }
                      }}
                    >
                      <div className="dfe-flex dfe-align-items--center ">
                        <div>✕ </div>
                        <div className="govuk-!-margin-left-1 govuk-!-text-align-left">
                          <span className="govuk-visually-hidden">
                            Clear theme{' '}
                          </span>
                          <div
                            className="govuk-body-xs govuk-!-margin-0"
                            style={{ fontSize: '0.8rem' }}
                          >
                            THEME
                          </div>
                          <div>{selectedTheme}</div>
                        </div>
                      </div>
                    </Button>
                  </span>
                )}

                <p className="govuk-visually-hidden">
                  Sorted by newest publications
                </p>
                <a href="#searchResults" className="govuk-skip-link">
                  Skip to search results
                </a>
              </div>
            )}

            <span className="govuk-!-margin-bottom-0">
              {!isFiltered && (
                <p>
                  {totalResults !== 0 && (
                    <>
                      Page {currentPage + 1} of {filteredPublications.length},
                    </>
                  )}
                  {` `}showing all publications
                </p>
              )}
            </span>
            {selectedPublication !== 'All publications' && (
              <>
                <h2 className="govuk-!-margin-bottom-3">
                  {dataType === 'csv' ? `32 data sets` : `12 API data sets`}
                </h2>
                <p>Page 1 of X, filtered by:</p>
                {selectedTheme && selectedTheme !== 'All themes' && (
                  <span className={styles.prototypeFilterTag}>
                    <Button
                      variant="secondary"
                      onClick={() => {
                        setSelectedTheme('All themes');
                        setSelectedPublication('All publications');
                        if (themeSelect) {
                          themeSelect.selectedIndex = 1;
                        }
                        if (publicationSelect) {
                          publicationSelect.selectedIndex = 1;
                        }
                      }}
                    >
                      <div className="dfe-flex dfe-align-items--center ">
                        <div>✕ </div>
                        <div className="govuk-!-margin-left-1 govuk-!-text-align-left">
                          <span className="govuk-visually-hidden">
                            Clear theme{' '}
                          </span>
                          <div
                            className="govuk-body-xs govuk-!-margin-0"
                            style={{ fontSize: '0.8rem' }}
                          >
                            THEME
                          </div>
                          <div>{selectedTheme}</div>
                        </div>
                      </div>
                    </Button>
                  </span>
                )}
                {selectedPublication &&
                  selectedPublication !== 'All publications' && (
                    <span className={styles.prototypeFilterTag}>
                      <Button
                        variant="secondary"
                        onClick={() => {
                          setSelectedPublication('All publications');
                          if (publicationSelect) {
                            publicationSelect.selectedIndex = 1;
                          }
                        }}
                      >
                        <div className="dfe-flex dfe-align-items--center ">
                          <div>✕ </div>
                          <div className="govuk-!-margin-left-1 govuk-!-text-align-left">
                            <span className="govuk-visually-hidden">
                              Clear publication{' '}
                            </span>
                            <div
                              className="govuk-body-xs govuk-!-margin-0"
                              style={{ fontSize: '0.8rem' }}
                            >
                              PUBLICATION
                            </div>
                            <div>{selectedPublication}</div>
                          </div>
                        </div>
                      </Button>
                    </span>
                  )}
                {/* 
                <SummaryList noBorder>
                  <SummaryListItem term="Type">
                    <span className="govuk-tag">National statistics</span>{' '}
                    {selectedRelease === latestRelease && (
                      <span className="govuk-tag">latest data</span>
                    )}
                    {selectedRelease !== latestRelease && (
                      <span className="govuk-tag govuk-tag--orange">
                        Not the latest data
                      </span>
                    )}
                  </SummaryListItem>
                  <SummaryListItem term="Release">
                    {selectedRelease}
                    {selectedRelease !== latestRelease && (
                      <p className="govuk-!-margin-top-3">
                        <a
                          href="#"
                          onClick={_ => {
                            setSelectedRelease(latestRelease);
                          }}
                        >
                          View the latest data: {latestRelease}
                        </a>
                      </p>
                    )}
                  </SummaryListItem>
                  <SummaryListItem term="Theme">
                    Further education
                  </SummaryListItem>
                  <SummaryListItem term="Last updated">
                    21 December 2022
                  </SummaryListItem>
                  <SummaryListItem term="Options">
                    <ul className="govuk-list">
                      {dataType === 'csv' && (
                        <li>
                          <Button className="govuk-!-margin-bottom-3">
                            Download all 32 datasets (.zip)
                          </Button>
                        </li>
                      )}

                      <li>
                        <Link to="./releaseData#exploreData">
                          View this release
                        </Link2
                      </li>
                    </ul>
                  </SummaryListItem>
                </SummaryList>
                */}
                {/* <dl>
                  <div className="dfe-flex">
                    <dt style={{ flexBasis: '200px' }}>Type</dt>
                    <dd>
                      <span className="govuk-tag">National statistics</span>{' '}
                      <span className="govuk-tag">latest data</span>
                    </dd>
                  </div>
                  <div className="dfe-flex">
                    <dt style={{ flexBasis: '200px' }}>Theme</dt>
                    <dd>Further education</dd>
                  </div>
                  <div className="dfe-flex">
                    <dt style={{ flexBasis: '200px' }}>Publication</dt>
                    <dd>Apprenticeships and traineeships</dd>
                  </div>
                  <div className="dfe-flex">
                    <dt style={{ flexBasis: '200px' }}>Release</dt>
                    <dd>Academic year 2021/22 </dd>
                  </div>
            </dl> */}{' '}
                <hr />
                <h2 className="govuk-!-heading-m govuk-heading-m">
                  {selectedPublication}{' '}
                  {dataType === 'csv' ? 'downloads' : 'API data sets'}
                </h2>
                <p className="govuk-!-margin-top-4 dfe-flex dfe-justify-content--space-between">
                  <div>
                    <span className="govuk-tag">National statistics</span>{' '}
                    {selectedRelease === latestRelease && (
                      <span className="govuk-tag">latest data</span>
                    )}
                    {selectedRelease !== latestRelease && (
                      <span className="govuk-tag govuk-tag--orange">
                        Not the latest data
                      </span>
                    )}
                  </div>
                  <div>
                    <Link
                      to="./releaseData#exploreData"
                      className="govuk-!-margin-left-3"
                    >
                      View this publication
                    </Link>
                  </div>
                </p>
                {selectedRelease !== latestRelease && (
                  <p className="govuk-!-margin-top-3">
                    <a
                      href="#"
                      onClick={() => {
                        setSelectedRelease(latestRelease);
                      }}
                    >
                      View the latest data: {latestRelease}
                    </a>
                  </p>
                )}
                {dataType === 'csv' && (
                  <Button className="govuk-!-margin-bottom-3 govuk-!-margin-top-3">
                    Download all 32 data sets (ZIP)
                  </Button>
                )}
                {/* 
                <div>
                  <PrototypeSortFilters
                    sortOrder={selectedSortOrder}
                    onSort={sortOrder => {
                      setSelectedSortOrder(sortOrder);
                      setCurrentPage(0);
                    }}
                  />
                </div>
                */}
                <div
                  className="govuk-accordion__controls govuk-!-margin-top-3 govuk-!-margin-bottom-0"
                  style={{ marginLeft: '2px' }}
                >
                  <button
                    type="button"
                    className="govuk-accordion__show-all"
                    onClick={() => {
                      setListCompact(!listCompact);
                    }}
                  >
                    <span
                      className={classNames('govuk-accordion-nav__chevron', {
                        'govuk-accordion-nav__chevron--down': listCompact,
                      })}
                    />
                    <span className="govuk-accordion__show-all-text">
                      {`${
                        listCompact
                          ? 'Show all expanded details'
                          : 'Hide all expanded details'
                      } `}
                    </span>
                  </button>
                </div>
              </>
            )}
            {selectedPublication === 'All publications' &&
              selectedTheme === 'All themes' &&
              fullList && (
                <>
                  <hr />

                  <div>
                    <PrototypeSortFilters
                      sortOrder={selectedSortOrder}
                      onSort={sortOrder => {
                        setSelectedSortOrder(sortOrder);
                        setCurrentPage(0);
                      }}
                    />
                  </div>
                  <div
                    className="govuk-accordion__controls govuk-!-margin-top-3 govuk-!-margin-bottom-0"
                    style={{ marginLeft: '2px' }}
                  >
                    <button
                      type="button"
                      className="govuk-accordion__show-all"
                      onClick={() => {
                        setListCompact(!listCompact);
                      }}
                    >
                      <span
                        className={classNames('govuk-accordion-nav__chevron', {
                          'govuk-accordion-nav__chevron--down': listCompact,
                        })}
                      />
                      <span className="govuk-accordion__show-all-text">
                        {`${
                          listCompact
                            ? 'Show all expanded details'
                            : 'Hide all expanded details'
                        } `}
                      </span>
                    </button>
                  </div>

                  <ul className="govuk-list">
                    {dataType === 'api' && (
                      <li>
                        <hr />
                        <h3>
                          <a href="#">
                            {' '}
                            Academy Transfers and Funding 2013-14 to 2021-22
                          </a>
                        </h3>
                        <p className="govuk-!-margin-bottom-2">
                          This API data set contains the number of academies
                          that have moved trusts from the financial year 2013
                          -14 2014 to 2021-22 and the total grant funding
                          provided. It also compares the reason that academies
                          move trust from the financial year 2016-17 to 2021-22.
                        </p>
                        <SummaryList
                          compact
                          noBorder
                          className="govuk-!-margin-bottom-0"
                        >
                          <SummaryListItem term="Theme">
                            Children's social care
                          </SummaryListItem>
                          <SummaryListItem term="Publication">
                            Characteristics of children in need
                          </SummaryListItem>
                        </SummaryList>
                        <ToggleMoreDetails listCompact={listCompact}>
                          <SummaryList
                            compact
                            noBorder
                            className="govuk-!-margin-bottom-0"
                          >
                            {dataType === 'api' && (
                              <SummaryListItem term="API status">
                                <span className="govuk-tag govuk-tag--turquoise">
                                  ACTIVE
                                </span>{' '}
                                Version 2.1
                              </SummaryListItem>
                            )}
                            <SummaryListItem term="Geographic level">
                              National
                            </SummaryListItem>
                            <SummaryListItem term="Indicators">
                              Number of academies in England as at 31 March,
                              Number of academies where grant funding was
                              provided, Number of academies where no grant
                              funding was provided, Percentage of academies that
                              moved trust in England, Percentage of academies
                              where grant funding was provided, Percentage of
                              academies where no grant funding was provided,
                              Total grant funding provided{' '}
                              <strong>+ 20 more</strong>
                            </SummaryListItem>
                            <SummaryListItem term="Filters">
                              No filters
                            </SummaryListItem>

                            <SummaryListItem term="Time period">
                              Academic years 2013-14 to 2021-22
                            </SummaryListItem>
                            <SummaryListItem term="Published">
                              22 December 2022
                            </SummaryListItem>
                          </SummaryList>
                        </ToggleMoreDetails>
                      </li>
                    )}
                    <li>
                      <hr />
                      <h3>
                        <a href="#">
                          A1 National time series of children in need, referrals
                          and assessments
                        </a>
                      </h3>
                      <p className="govuk-!-margin-bottom-2">
                        Children in need, episodes of need, and referrals and
                        assessments completed by children's social care
                        services.{' '}
                      </p>
                      <SummaryList
                        compact
                        noBorder
                        className="govuk-!-margin-bottom-0"
                      >
                        <SummaryListItem term="Theme">
                          Children's social care
                        </SummaryListItem>
                        <SummaryListItem term="Publication">
                          Characteristics of children in need
                        </SummaryListItem>
                      </SummaryList>
                      <ToggleMoreDetails listCompact={listCompact}>
                        <SummaryList compact noBorder>
                          {dataType === 'api' && (
                            <SummaryListItem term="API status">
                              <span className="govuk-tag govuk-tag--turquoise">
                                ACTIVE
                              </span>{' '}
                              Version 2.1
                            </SummaryListItem>
                          )}
                          <SummaryListItem term="Geographic level">
                            National
                          </SummaryListItem>
                          <SummaryListItem term="Indicators">
                            Number, percentage, rate per 10,000 children aged
                            under 18 years
                          </SummaryListItem>
                          <SummaryListItem term="Filters">
                            Assessments, children in need, referrals
                          </SummaryListItem>
                          <SummaryListItem term="Time period">
                            Full years 2013 to 2022
                          </SummaryListItem>
                          <SummaryListItem term="Published">
                            22 December 2022
                          </SummaryListItem>
                          {dataType === 'csv' && (
                            <SummaryListItem term="Filetype and size">
                              <a href="#">Download data set (CSV, 17 KB)</a>
                            </SummaryListItem>
                          )}
                        </SummaryList>
                      </ToggleMoreDetails>
                    </li>
                    <li>
                      <hr />
                      <h3>
                        <a href="#">Annual Headlines - detailed series</a>
                      </h3>
                      <p className="govuk-!-margin-bottom-2">
                        Time series of headline apprenticeship figures.{' '}
                      </p>
                      <SummaryList
                        compact
                        noBorder
                        className="govuk-!-margin-bottom-0"
                      >
                        <SummaryListItem term="Theme">
                          Further education
                        </SummaryListItem>
                        <SummaryListItem term="Publication">
                          Apprenticeships and traineeships
                        </SummaryListItem>
                      </SummaryList>
                      <ToggleMoreDetails listCompact={listCompact}>
                        <SummaryList compact noBorder>
                          {dataType === 'api' && (
                            <SummaryListItem term="API status">
                              <span className="govuk-tag govuk-tag--turquoise">
                                ACTIVE
                              </span>{' '}
                              Version 1.4
                            </SummaryListItem>
                          )}
                          <SummaryListItem term="Geographic level">
                            National
                          </SummaryListItem>
                          <SummaryListItem term="Indicators">
                            Starts, Achievements, Learner participation,
                            Percentage Starts, Percentage Achievements,
                            Percentage Learner participation
                          </SummaryListItem>
                          <SummaryListItem term="Filters">
                            Apprenticeship level, Funding type, Age group
                          </SummaryListItem>
                          <SummaryListItem term="Time period">
                            Academic years 2015/16 to 2021/22
                          </SummaryListItem>
                          <SummaryListItem term="Published">
                            22 December 2022
                          </SummaryListItem>
                          {dataType === 'csv' && (
                            <SummaryListItem term="Filetype and size">
                              <a href="#">Download data set (CSV, 45 KB)</a>
                            </SummaryListItem>
                          )}
                        </SummaryList>
                      </ToggleMoreDetails>
                    </li>
                    <li>
                      <hr />
                      <h3>
                        <Link to={`./data-selected?dataType=${dataType}`}>
                          Apprenticeship Achievement Rates Detailed Series
                        </Link>
                      </h3>
                      <p className="govuk-!-margin-bottom-2">
                        Apprenticeship national achievement rate tables
                      </p>
                      <SummaryList
                        compact
                        noBorder
                        className="govuk-!-margin-bottom-0"
                      >
                        <SummaryListItem term="Theme">
                          Further education
                        </SummaryListItem>
                        <SummaryListItem term="Publication">
                          Apprenticeships and traineeships
                        </SummaryListItem>
                      </SummaryList>
                      <ToggleMoreDetails listCompact={listCompact}>
                        <SummaryList compact noBorder>
                          {dataType === 'api' && (
                            <SummaryListItem term="API status">
                              <span className="govuk-tag govuk-tag--turquoise">
                                ACTIVE
                              </span>{' '}
                              Version 2.0
                            </SummaryListItem>
                          )}

                          <SummaryListItem term="Indicators">
                            Achievement rate, Achievers, Completers, Leavers,
                            Pass rate, Retention rate
                          </SummaryListItem>
                          <SummaryListItem term="Filters">
                            Age, Level, demographic - ethnicity, gender and
                            lldd, Standard /Framework flag
                          </SummaryListItem>
                          <SummaryListItem term="Geographic level">
                            National
                          </SummaryListItem>
                          <SummaryListItem term="Time period">
                            Academic years 2018/19 to 2020/21
                          </SummaryListItem>
                          {dataType === 'csv' && (
                            <SummaryListItem term="Filetype and size">
                              <a href="#">Download data set (CSV, 2 MB)</a>
                            </SummaryListItem>
                          )}
                        </SummaryList>
                      </ToggleMoreDetails>
                    </li>
                    <li>
                      <hr />
                      <h3>
                        <a href="#">Key stage 4 national level destinations</a>
                      </h3>
                      <p className="govuk-!-margin-bottom-2">
                        National level destinations data for students leaving
                        key stage 4 for different characteristic groups,
                        provider types, and qualification levels.
                      </p>

                      <SummaryList
                        compact
                        noBorder
                        className="govuk-!-margin-bottom-0"
                      >
                        <SummaryListItem term="Theme">
                          Destination of pupils and students
                        </SummaryListItem>
                        <SummaryListItem term="Publication">
                          Key stage 4 destination measures
                        </SummaryListItem>
                      </SummaryList>
                      <ToggleMoreDetails listCompact={listCompact}>
                        <SummaryList compact noBorder>
                          {dataType === 'api' && (
                            <SummaryListItem term="API status">
                              <span className="govuk-tag govuk-tag--turquoise">
                                ACTIVE
                              </span>{' '}
                              Version 1.0
                            </SummaryListItem>
                          )}
                          <SummaryListItem term="Geographic level">
                            National
                          </SummaryListItem>
                          <SummaryListItem term="Indicators">
                            Headline destination measure, destination, education
                            destination breakdown, other education breakdown,
                            apprenticeship level breakdown, Not sustained
                            breakdown, activity not captured breakdown
                          </SummaryListItem>
                          <SummaryListItem term="Filters">
                            Data type, institution groups, student
                            characteristics
                          </SummaryListItem>

                          <SummaryListItem term="Time period">
                            Academic Years 2010/11 to 2020/21
                          </SummaryListItem>
                          <SummaryListItem term="Published">
                            22 December 2022
                          </SummaryListItem>
                          {dataType === 'csv' && (
                            <SummaryListItem term="Filetype and size">
                              <a href="#">Download data set (CSV, 1 MB)</a>
                            </SummaryListItem>
                          )}
                        </SummaryList>
                      </ToggleMoreDetails>
                    </li>
                    <li>
                      <hr />
                      <h3>
                        <a href="#">School income - national rounded summary</a>
                      </h3>
                      <p className="govuk-!-margin-bottom-2">
                        This file contains national level rounded data on income
                        of local authority maintained schools. It was collected
                        via the Consistent Financial reporting data collection.
                      </p>
                      <SummaryList
                        compact
                        noBorder
                        className="govuk-!-margin-bottom-0"
                      >
                        <SummaryListItem term="Theme">
                          Finance and funding
                        </SummaryListItem>
                        <SummaryListItem term="Publication">
                          LA and school expenditure
                        </SummaryListItem>
                      </SummaryList>
                      <ToggleMoreDetails listCompact={listCompact}>
                        <SummaryList compact noBorder>
                          {dataType === 'api' && (
                            <SummaryListItem term="API status">
                              <span className="govuk-tag govuk-tag--turquoise">
                                ACTIVE
                              </span>{' '}
                              Version 1.4
                            </SummaryListItem>
                          )}
                          <SummaryListItem term="Geographic level">
                            National
                          </SummaryListItem>
                          <SummaryListItem term="Indicators">
                            Income (&pound; million), Income per pupil
                          </SummaryListItem>
                          <SummaryListItem term="Filters">
                            Category of income, phase of school
                          </SummaryListItem>
                          <SummaryListItem term="Time period">
                            Financial years 2015-16 to 2021-22
                          </SummaryListItem>
                          <SummaryListItem term="Published">
                            22 December 2022
                          </SummaryListItem>
                          {dataType === 'csv' && (
                            <SummaryListItem term="Filetype and size">
                              <a href="#">Download data set (CSV, 161 KB)</a>
                            </SummaryListItem>
                          )}
                        </SummaryList>
                      </ToggleMoreDetails>
                    </li>
                    <li>
                      <hr />
                      <h3>
                        <a href="#">01 - Absence rates by geographic level</a>
                      </h3>
                      <p className="govuk-!-margin-bottom-2">
                        Absence information for full academic year 2020/21 for
                        all enrolments in state-funded primary, secondary and
                        special schools including information on overall
                        absence, persistent absence and reason for absence for
                        pupils aged 5-15. Includes school level data.
                      </p>
                      <SummaryList
                        compact
                        noBorder
                        className="govuk-!-margin-bottom-0"
                      >
                        <SummaryListItem term="Theme">
                          Pupils and schools
                        </SummaryListItem>
                        <SummaryListItem term="Publication">
                          Pupil absence in schools in England
                        </SummaryListItem>
                      </SummaryList>
                      <ToggleMoreDetails listCompact={listCompact}>
                        <SummaryList compact noBorder>
                          {dataType === 'api' && (
                            <SummaryListItem term="API status">
                              <span className="govuk-tag govuk-tag--red">
                                DEPRECATED
                              </span>{' '}
                              Version 1.1
                            </SummaryListItem>
                          )}
                          <SummaryListItem term="Geographic level">
                            National
                          </SummaryListItem>
                          <SummaryListItem term="Indicators">
                            Income (&pound; million), Income per pupil
                          </SummaryListItem>
                          <SummaryListItem term="Filters">
                            Category of income, phase of school
                          </SummaryListItem>
                          <SummaryListItem term="Time period">
                            Financial years 2015-16 to 2021-22
                          </SummaryListItem>
                          <SummaryListItem term="Published">
                            22 December 2022
                          </SummaryListItem>
                          {dataType === 'csv' && (
                            <SummaryListItem term="Filetype and size">
                              <a href="#">Download data set (CSV, 101 MB)</a>
                            </SummaryListItem>
                          )}
                        </SummaryList>
                      </ToggleMoreDetails>
                    </li>
                  </ul>
                </>
              )}
            {selectedPublication === 'All publications' &&
              selectedTheme === 'All themes' &&
              !fullList && (
                <div className={styles.prototypeGrid2col}>
                  <div
                    className={classNames(styles.prototypeCardChevronOneThird)}
                  >
                    <h3>
                      <a
                        href="#"
                        className={classNames(
                          styles.prototypeCardChevronLink,
                          'govuk-link--no-visited-state',
                        )}
                        onClick={e => {
                          setSelectedTheme('Childrens social care');
                          e.preventDefault();
                        }}
                      >
                        Childrens social care
                      </a>
                    </h3>
                    <p>9 Publications</p>
                  </div>
                  <div
                    className={classNames(styles.prototypeCardChevronOneThird)}
                  >
                    <h3>
                      <a
                        href="#"
                        className={classNames(
                          styles.prototypeCardChevronLink,
                          'govuk-link--no-visited-state',
                        )}
                        onClick={e => {
                          setSelectedPublication('COVID 19');
                          e.preventDefault();
                        }}
                      >
                        COVID 19
                      </a>
                    </h3>
                    <p>7 Publications</p>
                  </div>
                  <div
                    className={classNames(styles.prototypeCardChevronOneThird)}
                  >
                    <h3>
                      <a
                        href="#"
                        className={classNames(
                          styles.prototypeCardChevronLink,
                          'govuk-link--no-visited-state',
                        )}
                        onClick={e => {
                          setSelectedTheme(
                            'Destinations of pupils and students',
                          );
                          e.preventDefault();
                        }}
                      >
                        Destinations of pupils and students
                      </a>
                    </h3>
                    <p>7 Publications</p>
                  </div>
                  <div
                    className={classNames(styles.prototypeCardChevronOneThird)}
                  >
                    <h3>
                      <a
                        href="#"
                        className={classNames(
                          styles.prototypeCardChevronLink,
                          'govuk-link--no-visited-state',
                        )}
                        onClick={e => {
                          setSelectedTheme('Early years');
                          e.preventDefault();
                        }}
                      >
                        Early years
                      </a>
                    </h3>
                    <p>4 Publications</p>
                  </div>
                  <div
                    className={classNames(styles.prototypeCardChevronOneThird)}
                  >
                    <h3>
                      <a
                        href="#"
                        className={classNames(
                          styles.prototypeCardChevronLink,
                          'govuk-link--no-visited-state',
                        )}
                        onClick={e => {
                          setSelectedTheme('Finance and funding');
                          e.preventDefault();
                        }}
                      >
                        Finance and funding
                      </a>
                    </h3>
                    <p>4 Publications</p>
                  </div>
                  <div
                    className={classNames(styles.prototypeCardChevronOneThird)}
                  >
                    <h3>
                      <a
                        href="#"
                        className={classNames(
                          styles.prototypeCardChevronLink,
                          'govuk-link--no-visited-state',
                        )}
                        onClick={e => {
                          setSelectedTheme('Further education');
                          e.preventDefault();
                        }}
                      >
                        Further education
                      </a>
                    </h3>
                    <p>9 Publications</p>
                  </div>
                  <div
                    className={classNames(styles.prototypeCardChevronOneThird)}
                  >
                    <h3>
                      <a
                        href="#"
                        className={classNames(
                          styles.prototypeCardChevronLink,
                          'govuk-link--no-visited-state',
                        )}
                        onClick={e => {
                          setSelectedTheme('Pupils and schools');
                          e.preventDefault();
                        }}
                      >
                        Pupils and schools
                      </a>
                    </h3>
                    <p>20 Publications</p>
                  </div>
                  <div
                    className={classNames(styles.prototypeCardChevronOneThird)}
                  >
                    <h3>
                      <a
                        href="#"
                        className={classNames(
                          styles.prototypeCardChevronLink,
                          'govuk-link--no-visited-state',
                        )}
                        onClick={e => {
                          setSelectedTheme(
                            'School and college outcomes and performance',
                          );
                          e.preventDefault();
                        }}
                      >
                        School and college outcomes and performance
                      </a>
                    </h3>
                    <p>9 Publications</p>
                  </div>
                  <div
                    className={classNames(styles.prototypeCardChevronOneThird)}
                  >
                    <h3>
                      <a
                        href="#"
                        className={classNames(
                          styles.prototypeCardChevronLink,
                          'govuk-link--no-visited-state',
                        )}
                        onClick={e => {
                          setSelectedTheme('Teachers and school workforce');
                          e.preventDefault();
                        }}
                      >
                        Teachers and school workforce
                      </a>
                    </h3>
                    <p>5 Publications</p>
                  </div>
                  <div
                    className={classNames(styles.prototypeCardChevronOneThird)}
                  >
                    <h3>
                      <a
                        href="#"
                        className={classNames(
                          styles.prototypeCardChevronLink,
                          'govuk-link--no-visited-state',
                        )}
                        onClick={e => {
                          setSelectedTheme(
                            'UK education and training statistics',
                          );
                          e.preventDefault();
                        }}
                      >
                        UK education and training statistics
                      </a>
                    </h3>
                    <p>1 Publication</p>
                  </div>
                </div>
              )}
            {selectedPublication === 'All publications' &&
              selectedTheme === 'Further education' &&
              fullList && (
                <>
                  <hr />

                  <div>
                    <PrototypeSortFilters
                      sortOrder={selectedSortOrder}
                      onSort={sortOrder => {
                        setSelectedSortOrder(sortOrder);
                        setCurrentPage(0);
                      }}
                    />
                  </div>
                  <div
                    className="govuk-accordion__controls govuk-!-margin-top-3 govuk-!-margin-bottom-0"
                    style={{ marginLeft: '2px' }}
                  >
                    <button
                      type="button"
                      className="govuk-accordion__show-all"
                      onClick={() => {
                        setListCompact(!listCompact);
                      }}
                    >
                      <span
                        className={classNames('govuk-accordion-nav__chevron', {
                          'govuk-accordion-nav__chevron--down': listCompact,
                        })}
                      />
                      <span className="govuk-accordion__show-all-text">
                        {`${
                          listCompact
                            ? 'Show all expanded details'
                            : 'Hide all expanded details'
                        } `}
                      </span>
                    </button>
                  </div>

                  <ul className="govuk-list">
                    <li>
                      <hr />
                      <h3>
                        <a href="#">Annual Headlines - detailed series</a>
                      </h3>
                      <p className="govuk-!-margin-bottom-2">
                        Time series of headline apprenticeship figures
                      </p>
                      <SummaryList
                        compact
                        noBorder
                        className="govuk-!-margin-bottom-0"
                      >
                        <SummaryListItem term="Theme">
                          Further education
                        </SummaryListItem>
                        <SummaryListItem term="Publication">
                          Apprenticeships and traineeships
                        </SummaryListItem>
                      </SummaryList>
                      <ToggleMoreDetails listCompact={listCompact}>
                        <SummaryList compact noBorder>
                          {dataType === 'api' && (
                            <SummaryListItem term="API status">
                              <span className="govuk-tag govuk-tag--turquoise">
                                ACTIVE
                              </span>{' '}
                              Version 1.4
                            </SummaryListItem>
                          )}
                          <SummaryListItem term="Geographic level">
                            National
                          </SummaryListItem>
                          <SummaryListItem term="Indicators">
                            Starts, Achievements, Learner participation,
                            Percentage Starts, Percentage Achievements,
                            Percentage Learner participation
                          </SummaryListItem>
                          <SummaryListItem term="Filters">
                            Apprenticeship level, Funding type, Age group
                          </SummaryListItem>

                          <SummaryListItem term="Time period">
                            Academic years 2015/16 to 2021/22
                          </SummaryListItem>
                          <SummaryListItem term="Published">
                            22 December 2022
                          </SummaryListItem>
                          {dataType === 'csv' && (
                            <SummaryListItem term="Filetype and size">
                              <a href="#">Download data set (CSV, 45 KB)</a>
                            </SummaryListItem>
                          )}
                        </SummaryList>
                      </ToggleMoreDetails>
                    </li>
                    <li>
                      <hr />
                      <h3>
                        <a href="#">
                          Apprenticeship achievements by enterprise
                          characteristics
                        </a>
                      </h3>
                      <p className="govuk-!-margin-bottom-2">
                        Data covering the industry characteristics of
                        apprenticeship achievements in England, where a match
                        has been made between the ILR and the ONS IDBR.
                      </p>
                      <SummaryList
                        compact
                        noBorder
                        className="govuk-!-margin-bottom-0"
                      >
                        <SummaryListItem term="Theme">
                          Further education
                        </SummaryListItem>
                        <SummaryListItem term="Publication">
                          Apprenticeships in England by industry characteristics
                        </SummaryListItem>
                      </SummaryList>
                      <ToggleMoreDetails listCompact={listCompact}>
                        <SummaryList compact noBorder>
                          {dataType === 'api' && (
                            <SummaryListItem term="API status">
                              <span className="govuk-tag govuk-tag--turquoise">
                                ACTIVE
                              </span>{' '}
                              Version 1.4
                            </SummaryListItem>
                          )}

                          <SummaryListItem term="Indicators">
                            Levy status, enterprise, two-digit SIC of enterprise
                          </SummaryListItem>
                          <SummaryListItem term="Filters">
                            IMD, sector of enterprise and standard
                            classification.
                          </SummaryListItem>
                          <SummaryListItem term="Geographic level">
                            National, Regional
                          </SummaryListItem>
                          <SummaryListItem term="Time period">
                            Academic years 2018/19 to 2020/21
                          </SummaryListItem>
                          <SummaryListItem term="Published">
                            22 December 2022
                          </SummaryListItem>
                          {dataType === 'csv' && (
                            <SummaryListItem term="Filetype and size">
                              <a href="#">Download data set (CSV, 84 MB)</a>
                            </SummaryListItem>
                          )}
                        </SummaryList>
                      </ToggleMoreDetails>
                    </li>
                    <li>
                      <hr />
                      <h3>
                        <Link to={`./data-selected?dataType=${dataType}`}>
                          Apprenticeship Achievement Rates Detailed Series
                        </Link>
                      </h3>
                      <p className="govuk-!-margin-bottom-2">
                        Apprenticeship national achievement rate tables
                      </p>
                      <SummaryList
                        compact
                        noBorder
                        className="govuk-!-margin-bottom-0"
                      >
                        <SummaryListItem term="Theme">
                          Further education
                        </SummaryListItem>
                        <SummaryListItem term="Publication">
                          Apprenticeships in England by industry characteristics
                        </SummaryListItem>
                      </SummaryList>
                      <ToggleMoreDetails listCompact={listCompact}>
                        <SummaryList compact noBorder>
                          {dataType === 'api' && (
                            <SummaryListItem term="API status">
                              <span className="govuk-tag govuk-tag--turquoise">
                                ACTIVE
                              </span>{' '}
                              Version 2.0
                            </SummaryListItem>
                          )}
                          <SummaryListItem term="Geographic level">
                            National
                          </SummaryListItem>
                          <SummaryListItem term="Indicators">
                            Achievement rate, Achievers, Completers, Leavers,
                            Pass rate, Retention rate
                          </SummaryListItem>
                          <SummaryListItem term="Filters">
                            Age, Level, demographic - ethnicity, gender and
                            lldd, Standard /Framework flag
                          </SummaryListItem>
                          <SummaryListItem term="Time period">
                            Academic years 2018/19 to 2020/21
                          </SummaryListItem>
                          {dataType === 'csv' && (
                            <SummaryListItem term="Filetype and size">
                              <a href="#">Download data set (CSV, 2 MB)</a>
                            </SummaryListItem>
                          )}
                        </SummaryList>
                      </ToggleMoreDetails>
                    </li>
                    <li>
                      <hr />
                      <h3>
                        <a href="#">
                          Employee numbers and median earnings by region,
                          sector, subsector, level and subject
                        </a>
                      </h3>
                      <p className="govuk-!-margin-bottom-2">
                        Data showing employee numbers and median earnings by
                        region, sector, sub-sector, level and subject area.
                      </p>
                      <SummaryList
                        compact
                        noBorder
                        className="govuk-!-margin-bottom-0"
                      >
                        <SummaryListItem term="Theme">
                          Further education
                        </SummaryListItem>
                        <SummaryListItem term="Publication">
                          Career pathways: post-16 qualifications held by
                          employees
                        </SummaryListItem>
                      </SummaryList>
                      <ToggleMoreDetails listCompact={listCompact}>
                        <SummaryList compact noBorder>
                          {dataType === 'api' && (
                            <SummaryListItem term="API status">
                              <span className="govuk-tag govuk-tag--turquoise">
                                ACTIVE
                              </span>{' '}
                              Version 1.8
                            </SummaryListItem>
                          )}
                          <SummaryListItem term="Geographic level">
                            National, Regional
                          </SummaryListItem>

                          <SummaryListItem term="Indicators">
                            Level, sector and sub-sector
                          </SummaryListItem>
                          <SummaryListItem term="Filters">
                            Region and subject area.
                          </SummaryListItem>

                          <SummaryListItem term="Time period">
                            Tax years 2018/19 to 2020/21
                          </SummaryListItem>
                          <SummaryListItem term="Published">
                            22 December 2022
                          </SummaryListItem>
                          {dataType === 'csv' && (
                            <SummaryListItem term="Filetype and size">
                              <a href="#">Download data set (CSV, 32 MB)</a>
                            </SummaryListItem>
                          )}
                        </SummaryList>
                      </ToggleMoreDetails>
                    </li>
                    <li>
                      <hr />
                      <h3>
                        <a href="#">
                          Destinations by qualification Title, Sector Subject
                          area, prior attainment and free school meal
                          eligibility
                        </a>
                      </h3>
                      <p className="govuk-!-margin-bottom-2">
                        Overall destination measures for each qualification.
                      </p>
                      <SummaryList
                        compact
                        noBorder
                        className="govuk-!-margin-bottom-0"
                      >
                        <SummaryListItem term="Theme">
                          Further education
                        </SummaryListItem>
                        <SummaryListItem term="Publication">
                          Detailed destinations of 16 to 18 year olds in Further
                          Education
                        </SummaryListItem>
                      </SummaryList>
                      <ToggleMoreDetails listCompact={listCompact}>
                        <SummaryList compact noBorder>
                          {dataType === 'api' && (
                            <SummaryListItem term="API status">
                              <span className="govuk-tag govuk-tag--turquoise">
                                ACTIVE
                              </span>{' '}
                              Version 1.3
                            </SummaryListItem>
                          )}
                          <SummaryListItem term="Geographic level">
                            National
                          </SummaryListItem>
                          <SummaryListItem term="Indicators">
                            Sustained positive destination rate, sustained
                            employment rate, sustained learning rate.
                          </SummaryListItem>
                          <SummaryListItem term="Filters">-</SummaryListItem>
                          <SummaryListItem term="Time period">
                            Academic year 2018/19
                          </SummaryListItem>
                          <SummaryListItem term="Published">
                            22 December 2022
                          </SummaryListItem>
                          {dataType === 'csv' && (
                            <SummaryListItem term="Filetype and size">
                              <a href="#">Download data set (CSV, 14 MB)</a>
                            </SummaryListItem>
                          )}
                        </SummaryList>
                      </ToggleMoreDetails>
                    </li>
                    <li>
                      <hr />
                      <h3>
                        <a href="#">
                          Number of FE learners going into employment and
                          learning destinations by local authority district
                        </a>
                      </h3>
                      <p className="govuk-!-margin-bottom-2">
                        Reports on the employment, and learning destinations of
                        adult FE & Skills learners, and all age apprentices that
                        achieved their learning aim, and Traineeship learners
                        that completed their aim
                      </p>
                      <SummaryList
                        compact
                        noBorder
                        className="govuk-!-margin-bottom-0"
                      >
                        <SummaryListItem term="Theme">
                          Further education
                        </SummaryListItem>
                        <SummaryListItem term="Publication">
                          FE learners going into employment and learning
                          destinations by local authority district
                        </SummaryListItem>
                      </SummaryList>
                      <ToggleMoreDetails listCompact={listCompact}>
                        <SummaryList compact noBorder>
                          {dataType === 'api' && (
                            <SummaryListItem term="API status">
                              <span className="govuk-tag govuk-tag--turquoise">
                                ACTIVE
                              </span>{' '}
                              Version 2.1
                            </SummaryListItem>
                          )}
                          <SummaryListItem term="Geographic level">
                            National, Regional,Local Authority Distric.
                          </SummaryListItem>
                          <SummaryListItem term="Indicators">
                            Earnings
                          </SummaryListItem>
                          <SummaryListItem term="Filters">
                            Full level 2, Full level 3, Level 4+
                          </SummaryListItem>
                          <SummaryListItem term="Time period">
                            Academic years 2013/14 to 2017/18
                          </SummaryListItem>
                          <SummaryListItem term="Published">
                            22 December 2022
                          </SummaryListItem>
                          {dataType === 'csv' && (
                            <SummaryListItem term="Filetype and size">
                              <a href="#">Download data set (CSV, 474 KB)</a>
                            </SummaryListItem>
                          )}
                        </SummaryList>
                      </ToggleMoreDetails>
                    </li>
                  </ul>
                </>
              )}
            {selectedPublication === 'All publications' &&
              selectedTheme === 'Further education' &&
              !fullList && (
                <>
                  <div className={styles.prototypeGrid2col}>
                    <div
                      className={classNames(
                        styles.prototypeCardChevronOneThird,
                      )}
                    >
                      <h3>
                        <a
                          href="#"
                          className={classNames(
                            styles.prototypeCardChevronLink,
                            'govuk-link--no-visited-state',
                          )}
                          onClick={e => {
                            setSelectedPublication(
                              'Apprenticeships and traineeships',
                            );
                            e.preventDefault();
                          }}
                        >
                          Apprenticeships and traineeships
                        </a>
                      </h3>
                      <p className="govuk-caption-m govuk-!-margin-top-1">
                        Academic Year 2021/22
                        <br />
                        National and official statistics
                      </p>
                      <p>32 datasets</p>
                    </div>
                    <div className={classNames(styles.prototypeCardChevron)}>
                      <h3
                        className={classNames(
                          styles.prototypeCardChevronLink,
                          'govuk-link--no-visited-state',
                        )}
                      >
                        <a href="#">
                          Apprenticeships in England by industry characteristics
                        </a>
                      </h3>
                      <p className="govuk-caption-m govuk-!-margin-top-1">
                        Academic Year 2020/21
                        <br />
                        Experimental statistics
                      </p>
                      <p>15 datasets</p>
                    </div>
                    <div className={classNames(styles.prototypeCardChevron)}>
                      <h3
                        className={classNames(
                          styles.prototypeCardChevronLink,
                          'govuk-link--no-visited-state',
                        )}
                      >
                        <a href="#">
                          Detailed destinations of 16 to 18 year olds in Further
                          Education
                        </a>
                      </h3>
                      <p className="govuk-caption-m govuk-!-margin-top-1">
                        Academic Year 2018/19
                        <br />
                        Ad hoc statistics
                      </p>
                      <p>3 datasets</p>
                    </div>
                    <div className={classNames(styles.prototypeCardChevron)}>
                      <h3
                        className={classNames(
                          styles.prototypeCardChevronLink,
                          'govuk-link--no-visited-state',
                        )}
                      >
                        <a href="#">
                          Career pathways: post-16 qualifications held by
                          employees
                        </a>
                      </h3>
                      <p className="govuk-caption-m govuk-!-margin-top-1">
                        Tax Year 2018-19
                        <br />
                        Ad hoc statistics
                      </p>
                      <p>5 datasets</p>
                    </div>
                    <div className={classNames(styles.prototypeCardChevron)}>
                      <h3
                        className={classNames(
                          styles.prototypeCardChevronLink,
                          'govuk-link--no-visited-state',
                        )}
                      >
                        <a href="#">
                          Detailed destinations of 16 to 18 year olds in Further
                          Education
                        </a>
                      </h3>
                      <p className="govuk-caption-m govuk-!-margin-top-1">
                        Academic Year 2018/19
                        <br />
                        Ad hoc statistics
                      </p>
                      <p>3 datasets</p>
                    </div>
                    <div className={classNames(styles.prototypeCardChevron)}>
                      <h3
                        className={classNames(
                          styles.prototypeCardChevronLink,
                          'govuk-link--no-visited-state',
                        )}
                      >
                        <a href="#">
                          FE learners going into employment and learning
                          destinations by local authority district
                        </a>
                      </h3>
                      <p className="govuk-caption-m govuk-!-margin-top-1">
                        Academic Year 2018/19
                        <br />
                        Ad hoc statistics
                      </p>
                      <p>3 datasets</p>
                    </div>
                    <div className={classNames(styles.prototypeCardChevron)}>
                      <h3
                        className={classNames(
                          styles.prototypeCardChevronLink,
                          'govuk-link--no-visited-state',
                        )}
                      >
                        <a href="#">Further education and skills</a>
                      </h3>
                      <p className="govuk-caption-m govuk-!-margin-top-1">
                        Academic Year 2018/19
                        <br />
                        National and official statistics
                      </p>
                      <p>3 datasets</p>
                    </div>
                    <div className={classNames(styles.prototypeCardChevron)}>
                      <h3
                        className={classNames(
                          styles.prototypeCardChevronLink,
                          'govuk-link--no-visited-state',
                        )}
                      >
                        <a href="#">
                          Further education: outcome-based success measures
                        </a>
                      </h3>
                      <p className="govuk-caption-m govuk-!-margin-top-1">
                        Academic Year 2018/19
                        <br />
                        National and official statistics
                      </p>
                      <p>3 datasets</p>
                    </div>
                    <div className={classNames(styles.prototypeCardChevron)}>
                      <h3
                        className={classNames(
                          styles.prototypeCardChevronLink,
                          'govuk-link--no-visited-state',
                        )}
                      >
                        <a href="#">Further education skills index</a>
                      </h3>
                      <p className="govuk-caption-m govuk-!-margin-top-1">
                        Academic Year 2018/19
                        <br />
                        National and official statistics
                      </p>
                      <p>3 datasets</p>
                    </div>
                    <div className={classNames(styles.prototypeCardChevron)}>
                      <h3
                        className={classNames(
                          styles.prototypeCardChevronLink,
                          'govuk-link--no-visited-state',
                        )}
                      >
                        <a href="#">Skills Bootcamps outcomes</a>
                      </h3>
                      <p className="govuk-caption-m govuk-!-margin-top-1">
                        Academic Year 2018/19
                        <br />
                        Ad hoc statistics
                      </p>
                      <p>3 datasets</p>
                    </div>
                    <div className={classNames(styles.prototypeCardChevron)}>
                      <h3
                        className={classNames(
                          styles.prototypeCardChevronLink,
                          'govuk-link--no-visited-state',
                        )}
                      >
                        <a href="#">Skills Bootcamps starts</a>
                      </h3>
                      <p className="govuk-caption-m govuk-!-margin-top-1">
                        Academic Year 2018/19
                        <br />
                        Ad hoc statistics
                      </p>
                      <p>3 datasets</p>
                    </div>
                  </div>
                  <hr />
                </>
              )}
            {selectedPublication === 'Apprenticeships and traineeships' && (
              <ul className="govuk-list">
                <li>
                  <hr />
                  <h3>
                    <a href="#">Annual Headlines - detailed series</a>
                  </h3>
                  <p className="govuk-!-margin-bottom-2">
                    Time series of headline apprenticeship figures
                  </p>
                  <SummaryList
                    compact
                    noBorder
                    className="govuk-!-margin-bottom-0"
                  >
                    <SummaryListItem term="Theme">
                      Further education
                    </SummaryListItem>
                    <SummaryListItem term="Publication">
                      Apprenticeships and traineeships
                    </SummaryListItem>
                  </SummaryList>
                  <ToggleMoreDetails listCompact={listCompact}>
                    <SummaryList compact noBorder>
                      {dataType === 'api' && (
                        <SummaryListItem term="API status">
                          <span className="govuk-tag govuk-tag--turquoise">
                            ACTIVE
                          </span>{' '}
                          Version 1.4
                        </SummaryListItem>
                      )}
                      <SummaryListItem term="Geographic level">
                        National
                      </SummaryListItem>
                      <SummaryListItem term="Indicators">
                        Starts, Achievements, Learner participation, Percentage
                        Starts, Percentage Achievements, Percentage Learner
                        participation
                      </SummaryListItem>
                      <SummaryListItem term="Filters">
                        Apprenticeship level, Funding type, Age group
                      </SummaryListItem>

                      <SummaryListItem term="Time period">
                        Academic years 2015/16 to 2021/22
                      </SummaryListItem>
                      {dataType === 'csv' && (
                        <SummaryListItem term="Filetype and size">
                          <a href="#">Download data set (CSV, 45 KB)</a>
                        </SummaryListItem>
                      )}
                    </SummaryList>
                  </ToggleMoreDetails>
                </li>
                <li>
                  <hr />
                  <h3>
                    <a href="#">
                      Apprenticeship Achievement Rates Demographics
                    </a>
                  </h3>
                  <p className="govuk-!-margin-bottom-2">
                    Apprenticeship national achievement rate tables
                  </p>
                  <SummaryList
                    compact
                    noBorder
                    className="govuk-!-margin-bottom-0"
                  >
                    <SummaryListItem term="Theme">
                      Further education
                    </SummaryListItem>
                    <SummaryListItem term="Publication">
                      Apprenticeships and traineeships
                    </SummaryListItem>
                  </SummaryList>
                  <ToggleMoreDetails listCompact={listCompact}>
                    <SummaryList compact noBorder>
                      {dataType === 'api' && (
                        <SummaryListItem term="API status">
                          <span className="govuk-tag govuk-tag--turquoise">
                            ACTIVE
                          </span>{' '}
                          Version 1.5
                        </SummaryListItem>
                      )}
                      <SummaryListItem term="Geographic level">
                        National
                      </SummaryListItem>
                      <SummaryListItem term="Indicators">
                        Achievement rate, Achievers, Completers, Leavers, Pass
                        rate, Retention rate
                      </SummaryListItem>
                      <SummaryListItem term="Filters">
                        Age, Level, demographic - ethnicity, gender and lldd,
                        Standard /Framework flag
                      </SummaryListItem>
                      <SummaryListItem term="Time period">
                        Academic years 2018/19 to 2020/21
                      </SummaryListItem>
                      {dataType === 'csv' && (
                        <SummaryListItem term="Filetype and size">
                          <a href="#">Download data set (CSV, 28 KB)</a>
                        </SummaryListItem>
                      )}
                    </SummaryList>
                  </ToggleMoreDetails>
                </li>
                <li>
                  <hr />
                  <h3>
                    <Link to={`./data-selected?dataType=${dataType}`}>
                      Apprenticeship Achievement Rates Detailed Series
                    </Link>
                  </h3>
                  <p className="govuk-!-margin-bottom-2">
                    Apprenticeship national achievement rate tables
                  </p>

                  <SummaryList
                    compact
                    noBorder
                    className="govuk-!-margin-bottom-0"
                  >
                    <SummaryListItem term="Theme">
                      Further education
                    </SummaryListItem>
                    <SummaryListItem term="Publication">
                      Apprenticeships and traineeships
                    </SummaryListItem>
                  </SummaryList>
                  <ToggleMoreDetails listCompact={listCompact}>
                    <SummaryList compact noBorder>
                      {dataType === 'api' && (
                        <SummaryListItem term="API status">
                          <span className="govuk-tag govuk-tag--turquoise">
                            ACTIVE
                          </span>{' '}
                          Version 2.0
                        </SummaryListItem>
                      )}
                      <SummaryListItem term="Geographic level">
                        National
                      </SummaryListItem>
                      <SummaryListItem term="Indicators">
                        Achievement rate, Achievers, Completers, Leavers, Pass
                        rate, Retention rate
                      </SummaryListItem>
                      <SummaryListItem term="Filters">
                        Age, Level, demographic - ethnicity, gender and lldd,
                        Standard /Framework flag
                      </SummaryListItem>

                      <SummaryListItem term="Time period">
                        Academic years 2018/19 to 2020/21
                      </SummaryListItem>
                      {dataType === 'csv' && (
                        <SummaryListItem term="Filetype and size">
                          <a href="#">Download data set (CSV, 2 MB)</a>
                        </SummaryListItem>
                      )}
                    </SummaryList>
                  </ToggleMoreDetails>
                </li>
                <li>
                  <hr />
                  <h3>
                    <a href="#">Apprenticeship Achievement Rates Headlines</a>
                  </h3>
                  <p className="govuk-!-margin-bottom-2">
                    Apprenticeship national achievement rate tables
                  </p>
                  <SummaryList
                    compact
                    noBorder
                    className="govuk-!-margin-bottom-0"
                  >
                    <SummaryListItem term="Theme">
                      Further education
                    </SummaryListItem>
                    <SummaryListItem term="Publication">
                      Apprenticeships and traineeships
                    </SummaryListItem>
                  </SummaryList>
                  <ToggleMoreDetails listCompact={listCompact}>
                    <SummaryList compact noBorder>
                      {dataType === 'api' && (
                        <SummaryListItem term="API status">
                          <span className="govuk-tag govuk-tag--turquoise">
                            ACTIVE
                          </span>{' '}
                          Version 1.1
                        </SummaryListItem>
                      )}
                      <SummaryListItem term="Geographic level">
                        National
                      </SummaryListItem>
                      <SummaryListItem term="Indicators">
                        Achievement rate, Leavers, Pass rate, Retention rate
                      </SummaryListItem>
                      <SummaryListItem term="Filters">
                        Level, Detailed Level, Sector Subject Area, Standard
                        /Framework flag
                      </SummaryListItem>

                      <SummaryListItem term="Time period">
                        Academic years 2018/19 to 2020/21
                      </SummaryListItem>
                      {dataType === 'csv' && (
                        <SummaryListItem term="Filetype and size">
                          <a href="#">Download data set (CSV, 9 KB)</a>
                        </SummaryListItem>
                      )}
                    </SummaryList>
                  </ToggleMoreDetails>
                </li>
                <li>
                  <hr />
                  <h3>
                    <a href="#">Apprenticeship Service - incentives</a>
                  </h3>
                  <p className="govuk-!-margin-bottom-2">
                    Incentive claims recorded on the apprenticeship service as
                    of June 2022
                  </p>
                  <SummaryList
                    compact
                    noBorder
                    className="govuk-!-margin-bottom-0"
                  >
                    <SummaryListItem term="Theme">
                      Further education
                    </SummaryListItem>
                    <SummaryListItem term="Publication">
                      Apprenticeships and traineeships
                    </SummaryListItem>
                  </SummaryList>
                  <ToggleMoreDetails listCompact={listCompact}>
                    <SummaryList compact noBorder>
                      {dataType === 'api' && (
                        <SummaryListItem term="API status">
                          <span className="govuk-tag govuk-tag--turquoise">
                            ACTIVE
                          </span>{' '}
                          Version 1.0
                        </SummaryListItem>
                      )}
                      <SummaryListItem term="Indicators">
                        Incentive claims
                      </SummaryListItem>
                      <SummaryListItem term="Filters">
                        Age group, Detailed apprenticeship level, Apprenticeship
                        start month, Sector subject area Tier 1
                      </SummaryListItem>
                      <SummaryListItem term="Geographic level">
                        National
                      </SummaryListItem>
                      <SummaryListItem term="Time period">
                        August 2020 onwards
                      </SummaryListItem>
                      {dataType === 'csv' && (
                        <SummaryListItem term="Filetype and size">
                          <a href="#">Download data set (CSV, 9 KB)</a>
                        </SummaryListItem>
                      )}
                    </SummaryList>
                  </ToggleMoreDetails>
                </li>
                <li>
                  <hr />
                  <h3>
                    <a href="#">Charts data</a>
                  </h3>
                  <p className="govuk-!-margin-bottom-2">
                    Historical time series of headline adult (19+) further
                    education and skills learner participation, containing
                    breakdowns by provision type and in some cases level. Also
                    includes all age apprenticeship participation figures.
                  </p>
                  <SummaryList
                    compact
                    noBorder
                    className="govuk-!-margin-bottom-0"
                  >
                    <SummaryListItem term="Theme">
                      Further education
                    </SummaryListItem>
                    <SummaryListItem term="Publication">
                      Apprenticeships and traineeships
                    </SummaryListItem>
                  </SummaryList>
                  <ToggleMoreDetails listCompact={listCompact}>
                    <SummaryList compact noBorder>
                      {dataType === 'api' && (
                        <SummaryListItem term="API status">
                          <span className="govuk-tag govuk-tag--turquoise">
                            ACTIVE
                          </span>{' '}
                          Version 2.0
                        </SummaryListItem>
                      )}
                      <SummaryListItem term="Geographic level">
                        National
                      </SummaryListItem>
                      <SummaryListItem term="Indicators">
                        Adult Apprenticeship, Adult Community learning, Adult
                        Education & training, Adult FE & skills, Adult FE &
                        skills - Level 4+, Adult FE & skills - Full Level 2,
                        Adult FE & skills - Full Level3, Apprenticeships -
                        Advanced, Apprenticeships - Higher, Apprenticeships -
                        Intermediate, All age Apprenticeships
                      </SummaryListItem>
                      <SummaryListItem term="Filters">-</SummaryListItem>

                      <SummaryListItem term="Time period">
                        Academic years 2005/06 to 2021/22
                      </SummaryListItem>
                      {dataType === 'csv' && (
                        <SummaryListItem term="Filetype and size">
                          <a href="#">Download data set (CSV, 9 KB)</a>
                        </SummaryListItem>
                      )}
                    </SummaryList>
                  </ToggleMoreDetails>
                </li>
                <li>
                  <hr />
                  <h3>
                    <a href="#">
                      Duration, planned length of stay and length of employment
                    </a>
                  </h3>
                  <p className="govuk-!-margin-bottom-2">
                    Apprenticeship duration, apprenticeship planned length of
                    stay and length of employment
                  </p>
                  <SummaryList
                    compact
                    noBorder
                    className="govuk-!-margin-bottom-0"
                  >
                    <SummaryListItem term="Theme">
                      Further education
                    </SummaryListItem>
                    <SummaryListItem term="Publication">
                      Apprenticeships and traineeships
                    </SummaryListItem>
                  </SummaryList>
                  <ToggleMoreDetails listCompact={listCompact}>
                    <SummaryList compact noBorder>
                      {dataType === 'api' && (
                        <SummaryListItem term="API status">
                          <span className="govuk-tag govuk-tag--turquoise">
                            ACTIVE
                          </span>{' '}
                          Version 1.2
                        </SummaryListItem>
                      )}
                      <SummaryListItem term="Geographic level">
                        National
                      </SummaryListItem>
                      <SummaryListItem term="Indicators">
                        Starts, Starts (used in duration calculations), Average
                        expected duration
                      </SummaryListItem>
                      <SummaryListItem term="Filters">
                        Age group, Detailed level, Length of employment, Planned
                        length of stay
                      </SummaryListItem>
                      <SummaryListItem term="Time period">
                        Academic years 2014/15 to 2021/22
                      </SummaryListItem>
                      {dataType === 'csv' && (
                        <SummaryListItem term="Filetype and size">
                          <a href="#">Download data set (CSV, 9 KB)</a>
                        </SummaryListItem>
                      )}
                    </SummaryList>
                  </ToggleMoreDetails>
                </li>
                <li>
                  <hr />
                  <h3>
                    <a href="#">Find an apprenticeship adverts and vacancies</a>
                  </h3>
                  <p className="govuk-!-margin-bottom-2">
                    Adverts and vacancies as reported on the Find an
                    apprenticeship website
                  </p>
                  <SummaryList
                    compact
                    noBorder
                    className="govuk-!-margin-bottom-0"
                  >
                    <SummaryListItem term="Theme">
                      Further education
                    </SummaryListItem>
                    <SummaryListItem term="Publication">
                      Apprenticeships and traineeships
                    </SummaryListItem>
                  </SummaryList>
                  <ToggleMoreDetails listCompact={listCompact}>
                    <SummaryList compact noBorder>
                      {dataType === 'api' && (
                        <SummaryListItem term="API status">
                          <span className="govuk-tag govuk-tag--turquoise">
                            ACTIVE
                          </span>{' '}
                          Version 1.0
                        </SummaryListItem>
                      )}
                      <SummaryListItem term="Geographic level">
                        National
                      </SummaryListItem>
                      <SummaryListItem term="Indicators">
                        Adverts, Vacancies
                      </SummaryListItem>
                      <SummaryListItem term="Filters">
                        Month, level
                      </SummaryListItem>
                      <SummaryListItem term="Time period">
                        August 2018 to October 2022
                      </SummaryListItem>
                      {dataType === 'csv' && (
                        <SummaryListItem term="Filetype and size">
                          <a href="#">Download data set (CSV, 9 KB)</a>
                        </SummaryListItem>
                      )}
                    </SummaryList>
                  </ToggleMoreDetails>
                </li>
                <li>
                  <hr />
                  <h3>
                    <a href="#">Geographical breakdowns - detailed</a>
                  </h3>
                  <p className="govuk-!-margin-bottom-2">
                    Detailed geographical breakdowns (National, Regional, Local
                    Authority District) of apprenticeship starts and
                    achievements
                  </p>
                  <SummaryList
                    compact
                    noBorder
                    className="govuk-!-margin-bottom-0"
                  >
                    <SummaryListItem term="Theme">
                      Further education
                    </SummaryListItem>
                    <SummaryListItem term="Publication">
                      Apprenticeships and traineeships
                    </SummaryListItem>
                  </SummaryList>
                  <ToggleMoreDetails listCompact={listCompact}>
                    <SummaryList compact noBorder>
                      {dataType === 'api' && (
                        <SummaryListItem term="API status">
                          <span className="govuk-tag govuk-tag--turquoise">
                            ACTIVE
                          </span>{' '}
                          Version 1.0
                        </SummaryListItem>
                      )}
                      <SummaryListItem term="Geographic level">
                        Local Authority District; National; Regional
                      </SummaryListItem>
                      <SummaryListItem term="Indicators">
                        Starts, Achievements
                      </SummaryListItem>
                      <SummaryListItem term="Filters">
                        Apprenticeship level, Ethnicity group, Sex, Sector
                        subject area (tier 1), Region, Local Authority District
                      </SummaryListItem>
                      <SummaryListItem term="Time period">
                        Academic year 2021/22
                      </SummaryListItem>
                      {dataType === 'csv' && (
                        <SummaryListItem term="Filetype and size">
                          <a href="#">Download data set (CSV, 9 KB)</a>
                        </SummaryListItem>
                      )}
                    </SummaryList>
                  </ToggleMoreDetails>
                </li>
                <li>
                  <hr />
                  <h3>
                    <a href="#">
                      Geographical breakdowns - latest regional summary
                    </a>
                  </h3>
                  <p className="govuk-!-margin-bottom-2">
                    Headline regional breakdowns of apprenticeship starts
                  </p>
                  <SummaryList
                    compact
                    noBorder
                    className="govuk-!-margin-bottom-0"
                  >
                    <SummaryListItem term="Theme">
                      Further education
                    </SummaryListItem>
                    <SummaryListItem term="Publication">
                      Apprenticeships and traineeships
                    </SummaryListItem>
                  </SummaryList>
                  <ToggleMoreDetails listCompact={listCompact}>
                    <SummaryList compact noBorder>
                      {dataType === 'api' && (
                        <SummaryListItem term="API status">
                          <span className="govuk-tag govuk-tag--turquoise">
                            ACTIVE
                          </span>{' '}
                          Version 1.0
                        </SummaryListItem>
                      )}
                      <SummaryListItem term="Filters">
                        Region, Apprenticeship level
                      </SummaryListItem>
                      <SummaryListItem term="Indicators">
                        Starts
                      </SummaryListItem>
                      <SummaryListItem term="Geographic level">
                        National, Regional
                      </SummaryListItem>
                      <SummaryListItem term="Time period">
                        Academic years 2018/19 to 2021/22
                      </SummaryListItem>
                      {dataType === 'csv' && (
                        <SummaryListItem term="Filetype and size">
                          <a href="#">Download data set (CSV, 9 KB)</a>
                        </SummaryListItem>
                      )}
                    </SummaryList>
                  </ToggleMoreDetails>
                </li>
              </ul>
            )}
            {totalResults !== 0 && (
              <>
                <hr />
                <p>Showing page {currentPage + 1} of X</p>
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

export default PrototypeDataCatalogue;
