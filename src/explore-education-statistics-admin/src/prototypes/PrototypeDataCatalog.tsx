import PrototypePage from '@admin/prototypes/components/PrototypePage';
import RelatedInformation from '@common/components/RelatedInformation';
import classNames from 'classnames';
import Link from '@admin/components/Link';
import ButtonText from '@common/components/ButtonText';
import styles2 from '@common/components/PageSearchForm.module.scss';
import { releaseTypes } from '@common/services/types/releaseType';
import PrototypeSearchResult from '@admin/prototypes/components/PrototypeSearchResult';
import PrototypeSortFilters, {
  PrototypeMobileSortFilters,
} from '@admin/prototypes/components/PrototypeSortFilters';
import PrototypeFilters from '@admin/prototypes/components/PrototypeFilters3';
import styles from '@admin/prototypes/PrototypePublicPage.module.scss';
import { publications, themes } from '@admin/prototypes/data/newThemesData';
import orderBy from 'lodash/orderBy';
import React, { useMemo, useState } from 'react';
import Button from '@common/components/Button';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';

const PrototypeDataCatalogue = () => {
  const params = new URLSearchParams(window.location.search);
  const urlTheme = params.get('theme');
  const urlPublication = params.get('publication');
  const urlSource = params.get('source');

  const [fullList, setFullList] = useState(true);
  const [listCompact, setListCompact] = useState(false);
  const [searchInput, setSearchInput] = useState('');
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedTheme, setSelectedTheme] = useState(
    urlTheme === 'fe' ? 'Further education' : 'All themes',
  );
  const [selectedPublication, setSelectedPublication] = useState(
    urlPublication === 'traineeships'
      ? 'Apprenticeships and traineeships'
      : 'All publications',
  );
  const [selectedReleaseType, setSelectedReleaseType] = useState(
    'all-release-types',
  );
  const [currentPage, setCurrentPage] = useState<number>(0);
  const [totalResults, setTotalResults] = useState<number>();
  const [selectedSortOrder, setSelectedSortOrder] = useState('newest');
  const [showFilters, setShowFilters] = useState(false);
  const [sourcePublication, setSourcePublication] = useState(
    urlSource === 'publicationPage',
  );
  const latestRelease = 'Academic year 2021/22';
  const [selectedRelease, setSelectedRelease] = useState(latestRelease);

  const generateSlug = (title: string) => {
    const slug = title.toLowerCase();
    return slug.split(' ').join('-');
  };

  const getSelectedTheme = (themeId: string) => {
    return themes.find(theme => theme.id === themeId);
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
    const filteredByThemeAndTopic =
      selectedTheme === 'all-themes'
        ? publications
        : publications.filter(publication => publication.theme === themeTitle);

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

    setTotalResults(selectedTheme === 'Further education' ? 100 : 32);

    const orderValue = selectedSortOrder === 'alpha' ? 'title' : 'published';
    const direction = selectedSortOrder === 'newest' ? 'desc' : 'asc';

    const ordered = orderBy(searched, orderValue, direction);

    return spliceIntoChunks(ordered, 10);
  }, [selectedReleaseType, selectedSortOrder, searchQuery, selectedTheme]);

  return (
    <div className={styles.prototypePublicPage}>
      <PrototypePage
        wide={false}
        breadcrumbs={[
          {
            name: 'Data catalogue',
            link: '/prototypes/data-catalog?theme=clear',
          },
          { name: selectedTheme, link: '/prototypes/data-catalog?theme=fe' },
          { name: selectedPublication, link: '/prototypes/data-catalog' },
        ]}
      >
        <div className="govuk-grid-row">
          <div className="govuk-grid-column-two-thirds">
            <h1
              className={classNames('govuk-heading-xl', [
                sourcePublication && 'govuk-!-margin-bottom-0',
              ])}
            >
              Data catalogue
            </h1>
            {sourcePublication && (
              <div className="govuk-!-margin-bottom-4">
                <Link to="./releaseData#exploreData" back>
                  Back to apprenticeships and traineeships, academic year
                  2021/22
                </Link>
              </div>
            )}
            <p className="govuk-body-l">
              View all of the open data available and choose files to download.
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

              {selectedPublication === 'Apprenticeships and traineeships' && (
                <>
                  <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
                    Publication
                  </h3>
                  <ul className="govuk-list">
                    <li>
                      <Link to="./releaseData#exploreData">
                        Apprenticeships and traineeships
                      </Link>
                    </li>
                  </ul>
                </>
              )}
            </RelatedInformation>
          </div>
        </div>
        <hr />

        <div className="govuk-grid-row">
          <div className="govuk-grid-column-one-third">
            <div
              className="govuk-form-group govuk-!-margin-bottom-6"
              style={{ position: 'relative' }}
            >
              <h2 className="govuk-label-wrapper">
                <label className="govuk-label govuk-label--s" htmlFor="search">
                  Search
                </label>
              </h2>

              <input
                type="search"
                id="search"
                className="govuk-input"
                value=""
              />
            </div>
            <div
              className="govuk-form-group govuk-!-margin-bottom-6"
              style={{ position: 'relative' }}
            >
              <h2 className="govuk-label-wrapper">
                <label className="govuk-label govuk-label--s" htmlFor="theme">
                  Theme
                </label>
              </h2>
              <select
                className="govuk-select"
                id="theme"
                onBlur={e => {
                  params.delete('theme');
                  setSelectedTheme(e.target.value);
                }}
              >
                <option value={selectedTheme}>{selectedTheme}</option>
                <option value="All themes">All themes</option>
                <option value="Children's social care">
                  Children's social care
                </option>
                <option value="COVID-19">COVID-19</option>
                <option value="Destination of pupils and students">
                  Destination of pupils and students
                </option>
                <option value="Early years">Early years</option>
                <option value="Finance and funding">Finance and funding</option>
                <option value="Further education">Further education</option>
                <option value="Higher education">Higher education</option>
                <option value="Pupils and schools">Pupils and schools</option>
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
                onBlur={e => {
                  setSelectedPublication(e.target.value);
                  setTotalResults(32);
                }}
                /* eslint-disable-next-line react/jsx-props-no-spreading */
                {...(selectedTheme !== 'Further education' && {
                  disabled: true,
                })}
              >
                <option value={selectedPublication}>
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
                  Career pathways: post-16 qualifications held by employees
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
                  FE learners going into employment and learning destinations by
                  local authority district
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
            <div
              className="govuk-form-group govuk-!-margin-bottom-6"
              style={{ position: 'relative' }}
            >
              <h2 className="govuk-label-wrapper">
                <label
                  className="govuk-label govuk-label--s"
                  htmlFor="pubilication"
                >
                  Release
                </label>
              </h2>

              <select
                className="govuk-select"
                id="release"
                /* eslint-disable-next-line react/jsx-props-no-spreading */
                {...(selectedPublication !==
                  'Apprenticeships and traineeships' && { disabled: true })}
                onBlur={e => {
                  setSelectedRelease(e.target.value);
                }}
              >
                <option
                  value={selectedRelease}
                  selected={selectedRelease !== ''}
                >
                  {selectedRelease}
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
            <hr />
            <div className="govuk-radios govuk-radios--small">
              <fieldset className="govuk-fieldset govuk-!-margin-top-6">
                <legend className="govuk-heading-s govuk-!-margin-bottom-0 ">
                  File types
                </legend>
                <div className="govuk-radios__item">
                  <input
                    type="radio"
                    className="govuk-radios__input"
                    name="filetype"
                    id="filetype-1"
                  />
                  <label
                    className="govuk-label govuk-radios__label"
                    htmlFor="filetype-1"
                  >
                    All files
                  </label>
                </div>
                <div className="govuk-radios__item">
                  <input
                    type="radio"
                    className="govuk-radios__input"
                    name="filetype"
                    id="filetype-2"
                  />
                  <label
                    className="govuk-label govuk-radios__label"
                    htmlFor="filetype-2"
                  >
                    Open data only (csv)
                  </label>
                </div>
              </fieldset>
              <div className="govuk-!-margin-top-3">
                <a
                  href="#"
                  onClick={e => {
                    params.delete('theme');
                    params.delete('publication');
                    setSelectedTheme('All themes');
                    setSelectedPublication('All publications');
                    e.preventDefault();
                  }}
                >
                  Clear all filters
                </a>
              </div>
            </div>
          </div>

          <div className="govuk-grid-column-two-thirds">
            {selectedPublication === 'All publications' && (
              <div role="region" aria-live="polite" aria-atomic="true">
                <div className="dfe-flex dfe-justify-content--space-between dfe-align-items--center">
                  <h2 className="govuk-!-margin-bottom-2 govuk-heading-m">
                    {selectedPublication === 'All publications' &&
                      selectedTheme === 'All themes' && (
                        <>
                          <div className="govuk-caption-m">
                            All themes and all publications{' '}
                          </div>
                          XXXX datasets
                        </>
                      )}
                    {selectedPublication === 'All publications' &&
                      selectedTheme === 'Further education' && (
                        <>
                          <div className="govuk-caption-m">
                            {selectedTheme}{' '}
                          </div>
                          XX datasets
                        </>
                      )}
                    {selectedPublication !== 'All publications' && (
                      <>{totalResults !== 1 ? '32 datasets' : 'result'}</>
                    )}
                  </h2>
                  {selectedPublication === 'All publications' &&
                    selectedTheme === 'Further education' && (
                      <div>
                        <a
                          href="#"
                          onClick={e => {
                            setSelectedTheme('All themes');
                            setSelectedPublication('All publications');
                            e.preventDefault();
                          }}
                        >
                          All themes
                        </a>
                      </div>
                    )}
                </div>
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
                <div className="dfe-flex dfe-justify-content--space-between dfe-align-items--center">
                  <h2 className="govuk-heading-m">
                    {' '}
                    <div className="govuk-caption-m">{selectedPublication}</div>
                    32 datasets{' '}
                  </h2>
                  <div>
                    <a
                      href="#"
                      onClick={e => {
                        setSelectedTheme('Further education');
                        setSelectedPublication('All publications');
                        e.preventDefault();
                      }}
                    >
                      All publications
                    </a>
                  </div>
                </div>
                <SummaryList noBorder>
                  <SummaryListItem term="Type">
                    <span className="govuk-tag">National statistics</span>{' '}
                    {selectedRelease === latestRelease && (
                      <span className="govuk-tag">latest data</span>
                    )}
                    {selectedRelease !== latestRelease && (
                      <span className="govuk-tag govuk-tag--red">
                        Not the latest data
                      </span>
                    )}
                    {selectedRelease !== latestRelease && (
                      <p className="govuk-!-margin-top-3">
                        <a
                          href="#"
                          onClick={e => {
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
                  <SummaryListItem term="Publication">
                    Apprenticeships and traineeships
                  </SummaryListItem>
                  <SummaryListItem term="Release">
                    Academic year 2021/22
                  </SummaryListItem>
                  <SummaryListItem term="Last updated">
                    21 December 2022
                  </SummaryListItem>
                </SummaryList>
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
                <Button className="govuk-!-margin-bottom-0">
                  Download all 32 datasets for this release (.zip)
                </Button>
                <hr />
                <div className="govuk-!-margin-top-0 dfe-flex dfe-justify-content--space-between dfe-align-items--center">
                  <div>
                    <PrototypeSortFilters
                      sortOrder={selectedSortOrder}
                      onSort={sortOrder => {
                        setSelectedSortOrder(sortOrder);
                        setCurrentPage(0);
                      }}
                    />
                  </div>
                  {listCompact && (
                    <a
                      href="#"
                      onClick={e => {
                        setListCompact(false);
                        e.preventDefault();
                      }}
                    >
                      Show full details
                    </a>
                  )}
                  {!listCompact && (
                    <a
                      href="#"
                      onClick={e => {
                        setListCompact(true);
                        e.preventDefault();
                      }}
                    >
                      Show compact list
                    </a>
                  )}
                </div>
              </>
            )}
            {selectedPublication === 'All publications' &&
              selectedTheme === 'All themes' &&
              fullList && (
                <>
                  <hr />
                  <div className="govuk-!-margin-top-0 dfe-flex dfe-justify-content--space-between dfe-align-items--center">
                    <div>
                      <PrototypeSortFilters
                        sortOrder={selectedSortOrder}
                        onSort={sortOrder => {
                          setSelectedSortOrder(sortOrder);
                          setCurrentPage(0);
                        }}
                      />
                    </div>
                    {listCompact && (
                      <a
                        href="#"
                        onClick={e => {
                          setListCompact(false);
                          e.preventDefault();
                        }}
                      >
                        Show full details
                      </a>
                    )}
                    {!listCompact && (
                      <a
                        href="#"
                        onClick={e => {
                          setListCompact(true);
                          e.preventDefault();
                        }}
                      >
                        Show compact list
                      </a>
                    )}
                  </div>
                  <ul className="govuk-list">
                    <li>
                      <hr />
                      <h3>
                        <a href="#">
                          A1 National time series of children in need, referrals
                          and assessments
                        </a>
                      </h3>
                      <p>
                        Children in need, episodes of need, and referrals and
                        assessments completed by children's social care
                        services.
                      </p>
                      {!listCompact && (
                        <SummaryList noBorder>
                          <SummaryListItem term="Filetype and size">
                            csv, 17 Kb
                          </SummaryListItem>
                          <SummaryListItem term="Theme">
                            Children's social care
                          </SummaryListItem>
                          <SummaryListItem term="Publication">
                            Characteristics of children in need
                          </SummaryListItem>
                          <SummaryListItem term="Indicators">
                            Number, percentage, rate per 10,000 children aged
                            under 18 years
                          </SummaryListItem>
                          <SummaryListItem term="Filters">
                            Assessments, children in need, referrals
                          </SummaryListItem>
                          <SummaryListItem term="Geographic level">
                            National
                          </SummaryListItem>
                          <SummaryListItem term="Time period">
                            Full years 2013 to 2022
                          </SummaryListItem>
                          <SummaryListItem term="Published">
                            22 December 2022
                          </SummaryListItem>
                        </SummaryList>
                      )}
                    </li>
                    <li>
                      <hr />
                      <h3>
                        <a href="#">Annual Headlines - detailed series</a>
                      </h3>
                      <p>Time series of headline apprenticeship figures</p>
                      {!listCompact && (
                        <SummaryList noBorder>
                          <SummaryListItem term="Filetype and size">
                            csv, 45 Kb
                          </SummaryListItem>
                          <SummaryListItem term="Theme">
                            Further education
                          </SummaryListItem>
                          <SummaryListItem term="Publication">
                            Apprenticeships and traineeships
                          </SummaryListItem>
                          <SummaryListItem term="Indicators">
                            Starts, Achievements, Learner participation,
                            Percentage Starts, Percentage Achievements,
                            Percentage Learner participation
                          </SummaryListItem>
                          <SummaryListItem term="Filters">
                            Apprenticeship level, Funding type, Age group
                          </SummaryListItem>
                          <SummaryListItem term="Geographic level">
                            National
                          </SummaryListItem>
                          <SummaryListItem term="Time period">
                            Academic years 2015/16 to 2021/22
                          </SummaryListItem>
                          <SummaryListItem term="Published">
                            22 December 2022
                          </SummaryListItem>
                        </SummaryList>
                      )}
                    </li>
                    <li>
                      <hr />
                      <h3>
                        <a href="./data-selected">
                          Apprenticeship Achievement Rates Detailed Series
                        </a>
                      </h3>
                      <p>Apprenticeship national achievement rate tables</p>
                      {!listCompact && (
                        <SummaryList noBorder>
                          <SummaryListItem term="Filetype and size">
                            csv, 2 Mb
                          </SummaryListItem>
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
                        </SummaryList>
                      )}
                    </li>
                    <li>
                      <hr />
                      <h3>
                        <a href="#">Key stage 4 national level destinations</a>
                      </h3>
                      <p>
                        National level destinations data for students leaving
                        key stage 4 for different characteristic groups,
                        provider types, and qualification levels.
                      </p>
                      {!listCompact && (
                        <SummaryList noBorder>
                          <SummaryListItem term="Filetype and size">
                            csv, 1 Mb
                          </SummaryListItem>
                          <SummaryListItem term="Theme">
                            Destination of pupils and students
                          </SummaryListItem>
                          <SummaryListItem term="Publication">
                            Key stage 4 destination measures
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
                          <SummaryListItem term="Geographic level">
                            National
                          </SummaryListItem>
                          <SummaryListItem term="Time period">
                            Academic Years 2010/11 to 2020/21
                          </SummaryListItem>
                          <SummaryListItem term="Published">
                            22 December 2022
                          </SummaryListItem>
                        </SummaryList>
                      )}
                    </li>
                    <li>
                      <hr />
                      <h3>
                        <a href="#">School income - national rounded summary</a>
                      </h3>
                      <p>
                        This file contains national level rounded data on income
                        of local authority maintained schools. It was collected
                        via the Consistent Financial reporting data collection.
                      </p>
                      {!listCompact && (
                        <SummaryList noBorder>
                          <SummaryListItem term="Filetype and size">
                            csv, 161 Kb
                          </SummaryListItem>
                          <SummaryListItem term="Theme">
                            Finance and funding
                          </SummaryListItem>
                          <SummaryListItem term="Publication">
                            LA and school expenditure
                          </SummaryListItem>
                          <SummaryListItem term="Indicators">
                            Income (&pound; million), Income per pupil
                          </SummaryListItem>
                          <SummaryListItem term="Filters">
                            Category of income, phase of school
                          </SummaryListItem>
                          <SummaryListItem term="Geographic level">
                            National
                          </SummaryListItem>
                          <SummaryListItem term="Time period">
                            Financial years 2015-16 to 2021-22
                          </SummaryListItem>
                          <SummaryListItem term="Published">
                            22 December 2022
                          </SummaryListItem>
                        </SummaryList>
                      )}
                    </li>
                    <li>
                      <hr />
                      <h3>
                        <a href="#">01 - Absence rates by geographic level</a>
                      </h3>
                      <p>
                        Absence information for full academic year 2020/21 for
                        all enrolments in state-funded primary, secondary and
                        special schools including information on overall
                        absence, persistent absence and reason for absence for
                        pupils aged 5-15. Includes school level data.
                      </p>
                      {!listCompact && (
                        <SummaryList noBorder>
                          <SummaryListItem term="Filetype and size">
                            csv, 101 Mb
                          </SummaryListItem>
                          <SummaryListItem term="Theme">
                            Pupils and schools
                          </SummaryListItem>
                          <SummaryListItem term="Publication">
                            Pupil absence in schools in England
                          </SummaryListItem>
                          <SummaryListItem term="Indicators">
                            Income (&pound; million), Income per pupil
                          </SummaryListItem>
                          <SummaryListItem term="Filters">
                            Category of income, phase of school
                          </SummaryListItem>
                          <SummaryListItem term="Geographic level">
                            National
                          </SummaryListItem>
                          <SummaryListItem term="Time period">
                            Financial years 2015-16 to 2021-22
                          </SummaryListItem>
                          <SummaryListItem term="Published">
                            22 December 2022
                          </SummaryListItem>
                        </SummaryList>
                      )}
                    </li>
                  </ul>
                </>
              )}
            {selectedPublication === 'All publications' &&
              selectedTheme === 'All themes' &&
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
                </>
              )}
            {selectedPublication === 'All publications' &&
              selectedTheme === 'Further education' &&
              fullList && (
                <>
                  <hr />
                  <div className="govuk-!-margin-top-0 dfe-flex dfe-justify-content--space-between dfe-align-items--center">
                    <div>
                      <PrototypeSortFilters
                        sortOrder={selectedSortOrder}
                        onSort={sortOrder => {
                          setSelectedSortOrder(sortOrder);
                          setCurrentPage(0);
                        }}
                      />
                    </div>
                    {listCompact && (
                      <a
                        href="#"
                        onClick={e => {
                          setListCompact(false);
                          e.preventDefault();
                        }}
                      >
                        Show full details
                      </a>
                    )}
                    {!listCompact && (
                      <a
                        href="#"
                        onClick={e => {
                          setListCompact(true);
                          e.preventDefault();
                        }}
                      >
                        Show compact list
                      </a>
                    )}
                  </div>
                  <ul className="govuk-list">
                    <li>
                      <hr />
                      <h3>
                        <a href="#">Annual Headlines - detailed series</a>
                      </h3>
                      <p>Time series of headline apprenticeship figures</p>
                      {!listCompact && (
                        <SummaryList noBorder>
                          <SummaryListItem term="Filetype and size">
                            csv, 45 Kb
                          </SummaryListItem>
                          <SummaryListItem term="Theme">
                            Further education
                          </SummaryListItem>
                          <SummaryListItem term="Publication">
                            Apprenticeships and traineeships
                          </SummaryListItem>
                          <SummaryListItem term="Indicators">
                            Starts, Achievements, Learner participation,
                            Percentage Starts, Percentage Achievements,
                            Percentage Learner participation
                          </SummaryListItem>
                          <SummaryListItem term="Filters">
                            Apprenticeship level, Funding type, Age group
                          </SummaryListItem>
                          <SummaryListItem term="Geographic level">
                            National
                          </SummaryListItem>
                          <SummaryListItem term="Time period">
                            Academic years 2015/16 to 2021/22
                          </SummaryListItem>
                          <SummaryListItem term="Published">
                            22 December 2022
                          </SummaryListItem>
                        </SummaryList>
                      )}
                    </li>
                    <li>
                      <hr />
                      <h3>
                        <a href="#">
                          Apprenticeship achievements by enterprise
                          characteristics
                        </a>
                      </h3>
                      <p>
                        Data covering the industry characteristics of
                        apprenticeship achievements in England, where a match
                        has been made between the ILR and the ONS IDBR.
                      </p>
                      {!listCompact && (
                        <SummaryList noBorder>
                          <SummaryListItem term="Filetype and size">
                            csv, 84 Mb
                          </SummaryListItem>
                          <SummaryListItem term="Theme">
                            Further education
                          </SummaryListItem>
                          <SummaryListItem term="Publication">
                            Apprenticeships in England by industry
                            characteristics
                          </SummaryListItem>
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
                        </SummaryList>
                      )}
                    </li>
                    <li>
                      <hr />
                      <h3>
                        <a href="./data-selected">
                          Apprenticeship Achievement Rates Detailed Series
                        </a>
                      </h3>
                      <p>Apprenticeship national achievement rate tables</p>
                      {!listCompact && (
                        <SummaryList noBorder>
                          <SummaryListItem term="Filetype and size">
                            csv, 2 Mb
                          </SummaryListItem>
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
                        </SummaryList>
                      )}
                    </li>
                    <li>
                      <hr />
                      <h3>
                        <a href="#">
                          Employee numbers and median earnings by region,
                          sector, subsector, level and subject (csv, 32 Mb)
                        </a>
                      </h3>
                      <p>
                        Data showing employee numbers and median earnings by
                        region, sector, sub-sector, level and subject area.
                      </p>
                      {!listCompact && (
                        <SummaryList noBorder>
                          <SummaryListItem term="Filetype and size">
                            csv, 32 Mb
                          </SummaryListItem>
                          <SummaryListItem term="Theme">
                            Further education
                          </SummaryListItem>
                          <SummaryListItem term="Publication">
                            Career pathways: post-16 qualifications held by
                            employees
                          </SummaryListItem>
                          <SummaryListItem term="Indicators">
                            Level, sector and sub-sector
                          </SummaryListItem>
                          <SummaryListItem term="Filters">
                            Region and subject area.
                          </SummaryListItem>
                          <SummaryListItem term="Geographic level">
                            National, Regional
                          </SummaryListItem>
                          <SummaryListItem term="Time period">
                            Tax years 2018/19 to 2020/21
                          </SummaryListItem>
                          <SummaryListItem term="Published">
                            22 December 2022
                          </SummaryListItem>
                        </SummaryList>
                      )}
                    </li>
                    <li>
                      <hr />
                      <h3>
                        <a href="#">
                          Destinations by qualification Title, Sector Subject
                          area, prior attainment and free school meal
                          eligibility (csv, 14 Mb)
                        </a>
                      </h3>
                      <p>
                        Overall destination measures for each qualification.
                      </p>
                      {!listCompact && (
                        <SummaryList noBorder>
                          <SummaryListItem term="Filetype and size">
                            csv, 14 Mb
                          </SummaryListItem>
                          <SummaryListItem term="Theme">
                            Further education
                          </SummaryListItem>
                          <SummaryListItem term="Publication">
                            Detailed destinations of 16 to 18 year olds in
                            Further Education
                          </SummaryListItem>
                          <SummaryListItem term="Indicators">
                            Sustained positive destination rate, sustained
                            employment rate, sustained learning rate.
                          </SummaryListItem>
                          <SummaryListItem term="Filters">-</SummaryListItem>
                          <SummaryListItem term="Geographic level">
                            National
                          </SummaryListItem>
                          <SummaryListItem term="Time period">
                            Academic year 2018/19
                          </SummaryListItem>
                          <SummaryListItem term="Published">
                            22 December 2022
                          </SummaryListItem>
                        </SummaryList>
                      )}
                    </li>
                    <li>
                      <hr />
                      <h3>
                        <a href="#">
                          Number of FE learners going into employment and
                          learning destinations by local authority district
                        </a>
                      </h3>
                      <p>
                        Reports on the employment, and learning destinations of
                        adult FE & Skills learners, and all age apprentices that
                        achieved their learning aim, and Traineeship learners
                        that completed their aim
                      </p>
                      {!listCompact && (
                        <SummaryList noBorder>
                          <SummaryListItem term="Filetype and size">
                            csv, 474 Kb
                          </SummaryListItem>
                          <SummaryListItem term="Theme">
                            Further education
                          </SummaryListItem>
                          <SummaryListItem term="Publication">
                            FE learners going into employment and learning
                            destinations by local authority district
                          </SummaryListItem>
                          <SummaryListItem term="Indicators">
                            Earnings
                          </SummaryListItem>
                          <SummaryListItem term="Filters">
                            Full level 2, Full level 3, Level 4+
                          </SummaryListItem>
                          <SummaryListItem term="Geographic level">
                            National, Regional,Local Authority Distric.
                          </SummaryListItem>
                          <SummaryListItem term="Time period">
                            Academic years 2013/14 to 2017/18
                          </SummaryListItem>
                          <SummaryListItem term="Published">
                            22 December 2022
                          </SummaryListItem>
                        </SummaryList>
                      )}
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
                  <p>Time series of headline apprenticeship figures</p>
                  {!listCompact && (
                    <SummaryList noBorder>
                      <SummaryListItem term="Filetype and size">
                        csv, 45Kb
                      </SummaryListItem>
                      <SummaryListItem term="Indicators">
                        Starts, Achievements, Learner participation, Percentage
                        Starts, Percentage Achievements, Percentage Learner
                        participation
                      </SummaryListItem>
                      <SummaryListItem term="Filters">
                        Apprenticeship level, Funding type, Age group
                      </SummaryListItem>
                      <SummaryListItem term="Geographic level">
                        National
                      </SummaryListItem>
                      <SummaryListItem term="Time period">
                        Academic years 2015/16 to 2021/22
                      </SummaryListItem>
                    </SummaryList>
                  )}
                </li>
                <li>
                  <hr />
                  <h3>
                    <a href="#">
                      Apprenticeship Achievement Rates Demographics
                    </a>
                  </h3>
                  <p>Apprenticeship national achievement rate tables</p>
                  {!listCompact && (
                    <SummaryList noBorder>
                      <SummaryListItem term="Filetype and size">
                        csv, 28 Kb
                      </SummaryListItem>
                      <SummaryListItem term="Indicators">
                        Achievement rate, Achievers, Completers, Leavers, Pass
                        rate, Retention rate
                      </SummaryListItem>
                      <SummaryListItem term="Filters">
                        Age, Level, demographic - ethnicity, gender and lldd,
                        Standard /Framework flag
                      </SummaryListItem>
                      <SummaryListItem term="Geographic level">
                        National
                      </SummaryListItem>
                      <SummaryListItem term="Time period">
                        Academic years 2018/19 to 2020/21
                      </SummaryListItem>
                    </SummaryList>
                  )}
                </li>
                <li>
                  <hr />
                  <h3>
                    <a href="./data-selected">
                      Apprenticeship Achievement Rates Detailed Series
                    </a>
                  </h3>
                  <p>Apprenticeship national achievement rate tables</p>
                  {!listCompact && (
                    <SummaryList noBorder>
                      <SummaryListItem term="Filetype and size">
                        csv, 2 Mb
                      </SummaryListItem>
                      <SummaryListItem term="Indicators">
                        Achievement rate, Achievers, Completers, Leavers, Pass
                        rate, Retention rate
                      </SummaryListItem>
                      <SummaryListItem term="Filters">
                        Age, Level, demographic - ethnicity, gender and lldd,
                        Standard /Framework flag
                      </SummaryListItem>
                      <SummaryListItem term="Geographic level">
                        National
                      </SummaryListItem>
                      <SummaryListItem term="Time period">
                        Academic years 2018/19 to 2020/21
                      </SummaryListItem>
                    </SummaryList>
                  )}
                </li>
                <li>
                  <hr />
                  <h3>
                    <a href="#">Apprenticeship Achievement Rates Headlines</a>
                  </h3>
                  <p>Apprenticeship national achievement rate tables</p>
                  {!listCompact && (
                    <SummaryList noBorder>
                      <SummaryListItem term="Filetype and size">
                        csv, 9 Kb
                      </SummaryListItem>
                      <SummaryListItem term="Indicators">
                        Achievement rate, Leavers, Pass rate, Retention rate
                      </SummaryListItem>
                      <SummaryListItem term="Filters">
                        Level, Detailed Level, Sector Subject Area, Standard
                        /Framework flag
                      </SummaryListItem>
                      <SummaryListItem term="Geographic level">
                        National
                      </SummaryListItem>
                      <SummaryListItem term="Time period">
                        Academic years 2018/19 to 2020/21
                      </SummaryListItem>
                    </SummaryList>
                  )}
                </li>
                <li>
                  <hr />
                  <h3>
                    <a href="#">Apprenticeship Service - incentives</a>
                  </h3>
                  <p>
                    Incentive claims recorded on the apprenticeship service as
                    of June 2022
                  </p>
                  {!listCompact && (
                    <SummaryList noBorder>
                      <SummaryListItem term="Filetype and size">
                        csv, 18 kb
                      </SummaryListItem>
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
                    </SummaryList>
                  )}
                </li>
                <li>
                  <hr />
                  <h3>
                    <a href="#">Charts data</a>
                  </h3>
                  <p>
                    Historical time series of headline adult (19+) further
                    education and skills learner participation, containing
                    breakdowns by provision type and in some cases level. Also
                    includes all age apprenticeship participation figures.
                  </p>
                  {!listCompact && (
                    <SummaryList noBorder>
                      <SummaryListItem term="Filetype and size">
                        csv, 2 kb
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
                      <SummaryListItem term="Geographic level">
                        National
                      </SummaryListItem>
                      <SummaryListItem term="Time period">
                        Academic years 2005/06 to 2021/22
                      </SummaryListItem>
                    </SummaryList>
                  )}
                </li>
                <li>
                  <hr />
                  <h3>
                    <a href="#">
                      Duration, planned length of stay and length of employment
                    </a>
                  </h3>
                  <p>
                    Apprenticeship duration, apprenticeship planned length of
                    stay and length of employment
                  </p>
                  {!listCompact && (
                    <SummaryList noBorder>
                      <SummaryListItem term="Filetype and size">
                        csv, 28 kb
                      </SummaryListItem>
                      <SummaryListItem term="Indicators">
                        Starts, Starts (used in duration calculations), Average
                        expected duration
                      </SummaryListItem>
                      <SummaryListItem term="Filters">
                        Age group, Detailed level, Length of employment, Planned
                        length of stay
                      </SummaryListItem>
                      <SummaryListItem term="Geographic level">
                        National
                      </SummaryListItem>
                      <SummaryListItem term="Time period">
                        Academic years 2014/15 to 2021/22
                      </SummaryListItem>
                    </SummaryList>
                  )}
                </li>
                <li>
                  <hr />
                  <h3>
                    <a href="#">Find an apprenticeship adverts and vacancies</a>
                  </h3>
                  <p>
                    Adverts and vacancies as reported on the Find an
                    apprenticeship website
                  </p>
                  {!listCompact && (
                    <SummaryList noBorder>
                      <SummaryListItem term="Filetype and size">
                        csv, 20 kb
                      </SummaryListItem>
                      <SummaryListItem term="Indicators">
                        Adverts, Vacancies
                      </SummaryListItem>
                      <SummaryListItem term="Filters">
                        Month, level
                      </SummaryListItem>
                      <SummaryListItem term="Geographic level">
                        National
                      </SummaryListItem>
                      <SummaryListItem term="Time period">
                        August 2018 to October 2022
                      </SummaryListItem>
                    </SummaryList>
                  )}
                </li>
                <li>
                  <hr />
                  <h3>
                    <a href="#">Geographical breakdowns - detailed</a>
                  </h3>
                  <p>
                    Detailed geographical breakdowns (National, Regional, Local
                    Authority District) of apprenticeship starts and
                    achievements
                  </p>
                  {!listCompact && (
                    <SummaryList noBorder>
                      <SummaryListItem term="Filetype and size">
                        csv, 62 Mb
                      </SummaryListItem>
                      <SummaryListItem term="Indicators">
                        Starts, Achievements
                      </SummaryListItem>
                      <SummaryListItem term="Filters">
                        Apprenticeship level, Ethnicity group, Sex, Sector
                        subject area (tier 1), Region, Local Authority District
                      </SummaryListItem>
                      <SummaryListItem term="Geographic level">
                        Local Authority District; National; Regional
                      </SummaryListItem>
                      <SummaryListItem term="Time period">
                        Academic year 2021/22
                      </SummaryListItem>
                    </SummaryList>
                  )}
                </li>
                <li>
                  <hr />
                  <h3>
                    <a href="#">
                      Geographical breakdowns - latest regional summary
                    </a>
                  </h3>
                  <p>Headline regional breakdowns of apprenticeship starts</p>
                  {!listCompact && (
                    <SummaryList noBorder>
                      <SummaryListItem term="Filetype and size">
                        csv, 16 kb
                      </SummaryListItem>
                      <SummaryListItem term="Indicators">
                        Starts
                      </SummaryListItem>
                      <SummaryListItem term="Filters">
                        Region, Apprenticeship level
                      </SummaryListItem>
                      <SummaryListItem term="Geographic level">
                        National, Regional
                      </SummaryListItem>
                      <SummaryListItem term="Time period">
                        Academic years 2018/19 to 2021/22
                      </SummaryListItem>
                    </SummaryList>
                  )}
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
