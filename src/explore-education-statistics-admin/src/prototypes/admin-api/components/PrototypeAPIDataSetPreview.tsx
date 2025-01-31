import PrototypePrevNextNav from '@admin/prototypes/components/PrototypePrevNextNav';
import pageStyles from '@admin/prototypes/PrototypePublicPage.module.scss';
import { PrototypeSubject } from '@admin/prototypes/admin-api/PrototypePublicationSubjects';
import styles from '@admin/prototypes/admin-api/components/PrototypeAPIDataSetPreview.module.scss';
import FormattedDate from '@common/components/FormattedDate';
import NotificationBanner from '@common/components/NotificationBanner';
import ButtonText from '@common/components/ButtonText';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import classNames from 'classnames';
import React, { useEffect, useRef, useState } from 'react';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import UrlContainer from '@common/components/UrlContainer';
import { Link } from 'react-router-dom';

interface Props {
  publicationSubject?: PrototypeSubject;
  notificationSummary?: string;
  onClose: () => void;
  onSubmit?: () => void;
}

const PrototypeAPIDataSetPreview = ({
  publicationSubject,
  notificationSummary,
  onClose,
  onSubmit,
}: Props) => {
  const ref = useRef<HTMLDivElement>(null);
  const contentRef = useRef<HTMLDivElement>(null);
  const [fullWidth, setFullWidth] = useState(false);
  const [fullTable, setFullTable] = useState(false);
  const dataType = 'api';
  const [sectionSelected, setSectionSelected] = useState(
    'dataSummary' as string | undefined,
  );
  const [sectionShowAll, setSectionShowAll] = useState(true);

  const changeSectionState = (newSection: string | undefined) => {
    setSectionSelected(newSection);
  };

  useEffect(() => {
    ref.current?.scrollIntoView(true);
  });

  return (
    <div ref={ref}>
      {notificationSummary && (
        <div className={styles.buttons}>
          <ButtonGroup>
            <Button onClick={onSubmit}>Send notification</Button>
            <Button onClick={onClose} variant="secondary">
              Edit notification
            </Button>
          </ButtonGroup>
        </div>
      )}

      <ButtonText
        className="govuk-!-margin-bottom-6 govuk-!-padding-left-3 govuk-link govuk-back-link"
        onClick={onClose}
      >
        Close preview
      </ButtonText>
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-full">
          <h1 className="govuk-heading-xl govuk-!-margin-bottom-3">
            <span className="govuk-caption-xl">Data catalogue</span>{' '}
            {publicationSubject?.title}
          </h1>
          <span className="govuk-tag">National statistics</span>{' '}
          <span className="govuk-tag">latest data</span>
          <p className="govuk-body-l govuk-!-margin-top-6">
            Apprenticeship and traineeship starts, achievements and
            participation. Includes breakdowns by age, sex, ethnicity, subject,
            provider, geography etc.
          </p>
        </div>
      </div>

      {notificationSummary && (
        <NotificationBanner
          heading="Upcoming changes to this API"
          title="Important"
        >
          <p>{notificationSummary}</p>
          <p className="govuk-hint">
            Message published: <FormattedDate>{new Date()}</FormattedDate>
          </p>
        </NotificationBanner>
      )}

      <hr className="govuk-!-margin-top-6" />

      <div className="govuk-grid-row">
        <div className={pageStyles.stickyLinksContainer}>
          <div>
            <div
              className={classNames(
                'govuk-grid-column-one-quarter',
                pageStyles.stickyLinksContainer,
              )}
            >
              <div className={classNames(pageStyles.stickyLinks)}>
                <h2 className="govuk-heading-m">Contents</h2>

                <ul className="govuk-list  govuk-list--spaced" id="pageNav">
                  <li>
                    <a
                      className={classNames(
                        'govuk-link--no-visited-state',
                        pageStyles.prototypeLinkNoUnderline,
                        {
                          'govuk-!-font-weight-bold':
                            sectionSelected === 'dataSummary',
                        },
                      )}
                      href={sectionShowAll ? '#dataSummary' : '#'}
                      onClick={_ => {
                        setSectionSelected('dataSummary');
                      }}
                    >
                      Data set summary
                    </a>
                  </li>
                  <li>
                    <a
                      className={classNames(
                        'govuk-link--no-visited-state',
                        pageStyles.prototypeLinkNoUnderline,
                        {
                          'govuk-!-font-weight-bold':
                            sectionSelected === 'dataPreview',
                        },
                      )}
                      href={sectionShowAll ? '#dataPreview' : '#'}
                      onClick={_ => {
                        setSectionSelected('dataPreview');
                      }}
                    >
                      Data preview
                    </a>
                  </li>
                  <li>
                    <a
                      className={classNames(
                        'govuk-link--no-visited-state',
                        pageStyles.prototypeLinkNoUnderline,
                        {
                          'govuk-!-font-weight-bold':
                            sectionSelected === 'changelog',
                        },
                      )}
                      href={sectionShowAll ? '#changelog' : '#'}
                      onClick={_ => {
                        setSectionSelected('changelog');
                      }}
                    >
                      Changelog
                    </a>
                  </li>
                  <li>
                    <a
                      className={classNames(
                        'govuk-link--no-visited-state',
                        pageStyles.prototypeLinkNoUnderline,
                        {
                          'govuk-!-font-weight-bold':
                            sectionSelected === 'versionHistory',
                        },
                      )}
                      href={sectionShowAll ? '#versionHistory' : '#'}
                      onClick={_ => {
                        setSectionSelected('versionHistory');
                      }}
                    >
                      Version history
                    </a>
                  </li>
                  <li>
                    <a
                      className={classNames(
                        'govuk-link--no-visited-state',
                        pageStyles.prototypeLinkNoUnderline,
                        {
                          'govuk-!-font-weight-bold':
                            sectionSelected === 'endPoints',
                        },
                      )}
                      href={sectionShowAll ? '#endPoints' : '#'}
                      onClick={_ => {
                        setSectionSelected('endPoints');
                      }}
                    >
                      API endpoints quick start
                    </a>
                  </li>
                  <li>
                    <a
                      className={classNames(
                        'govuk-link--no-visited-state',
                        pageStyles.prototypeLinkNoUnderline,
                        {
                          'govuk-!-font-weight-bold':
                            sectionSelected === 'download',
                        },
                      )}
                      href={sectionShowAll ? '#download' : '#'}
                      onClick={_ => {
                        setSectionSelected('download');
                      }}
                    >
                      Download data or create tables
                    </a>
                  </li>
                </ul>

                <h3 className="govuk-heading-s">Help and guidance</h3>
                <ul className="govuk-list govuk-list--spaced">
                  <li>
                    <Link
                      to="https://dfe-analytical-services.github.io/explore-education-statistics-api-docs/"
                      className={classNames(
                        'govuk-link--no-visited-state',
                        pageStyles.prototypeLinkNoUnderline,
                      )}
                    >
                      API documentation
                    </Link>
                  </li>
                  <li>
                    <Link
                      to="./releaseData"
                      className={classNames(
                        'govuk-link--no-visited-state',
                        pageStyles.prototypeLinkNoUnderline,
                      )}
                    >
                      View related release
                    </Link>
                  </li>
                </ul>
                <h3 className="govuk-heading-s">Page view</h3>
                <ul className="govuk-list">
                  <li>
                    <a
                      className={classNames(
                        'govuk-link--no-visited-state',
                        pageStyles.prototypeLinkNoUnderline,
                      )}
                      href="#"
                      onClick={e => {
                        e.preventDefault();
                        setSectionShowAll(!sectionShowAll);
                      }}
                    >
                      {sectionShowAll
                        ? 'Show as individual sections'
                        : 'Show all sections on page'}
                    </a>
                  </li>
                </ul>

                {sectionShowAll && (
                  <>
                    <hr />
                    <a href="#">
                      <svg
                        role="presentation"
                        focusable="false"
                        xmlns="http://www.w3.org/2000/svg"
                        width="13"
                        height="17"
                        viewBox="0 0 13 17"
                        className="govuk-!-margin-right-1"
                      >
                        <path
                          fill="currentColor"
                          d="M6.5 0L0 6.5 1.4 8l4-4v12.7h2V4l4.3 4L13 6.4z"
                        />
                      </svg>
                      Back to top
                    </a>
                  </>
                )}
              </div>
            </div>
          </div>
        </div>
        <div className="govuk-grid-column-three-quarters" ref={contentRef}>
          {(sectionShowAll || sectionSelected === 'dataSummary') && (
            <section id="dataSummary" className={pageStyles.sectionScroll}>
              <h2 className="govuk-heading-l govuk-!-margin-top-0 ">
                Data set summary
              </h2>
              <SummaryList noBorder className="govuk-!-margin-bottom-">
                {dataType === 'api' && (
                  <SummaryListItem term="API status">
                    <>
                      <span className="govuk-tag govuk-tag--turquoise">
                        ACTIVE
                      </span>{' '}
                      v1.0
                    </>
                  </SummaryListItem>
                )}

                <SummaryListItem term="Theme">
                  Further education
                </SummaryListItem>
                <SummaryListItem term="Publication">
                  Apprenticeships and traineeships
                </SummaryListItem>

                <SummaryListItem term="Published">
                  22 December 2022
                </SummaryListItem>
                <SummaryListItem term="Geographic level">
                  National
                </SummaryListItem>
                <SummaryListItem term="Time period">
                  Academic years 2018/19 to 2020/21
                </SummaryListItem>
                <SummaryListItem term="Indicators">
                  Achievement rate, Achievers, Completers, Leavers, Pass rate,
                  Retention rate
                </SummaryListItem>
                <SummaryListItem term="Filters">
                  Age, Level, demographic - ethnicity, gender and lldd, Standard
                  /Framework flag
                </SummaryListItem>
              </SummaryList>
              {!sectionShowAll && (
                <PrototypePrevNextNav
                  changeSectionState={changeSectionState}
                  nextId="dataPreview"
                  nextTitle="Data preview"
                />
              )}
              {sectionShowAll && (
                <hr className="govuk-visibility-hidden govuk-!-margin-bottom-9" />
              )}
            </section>
          )}

          {(sectionShowAll || sectionSelected === 'dataPreview') && (
            <section id="dataPreview" className={pageStyles.sectionScroll}>
              <h2 className="govuk-heading-l" id="dataPreview">
                Data preview
              </h2>
              <div className="govuk-!-margin-bottom-6">
                <h3 className="govuk-!-margin-bottom-0">
                  Snapshot showing {fullTable ? 'first 5 rows' : 'first row'} of
                  XXXXX, taken from underlying data
                </h3>
                <a
                  href="#"
                  onClick={e => {
                    e.preventDefault();
                    setFullTable(!fullTable);
                  }}
                >
                  {fullTable ? 'Show first row only' : 'Show more rows'}
                </a>
              </div>
              <div style={{ maxWidth: '100%', overflow: 'auto' }}>
                <table className="govuk-table" style={{ width: '3500px' }}>
                  <caption
                    className="govuk-!-margin-bottom-3 govuk-visually-hidden"
                    style={{ fontWeight: 'normal' }}
                  >
                    Snapshot showing {fullTable ? 'first 5 rows' : 'first row'}{' '}
                    of XXXXX, taken from underlying data
                  </caption>
                  <thead>
                    <tr>
                      <th>time_period</th>
                      <th>time_identifier</th>
                      <th>geographic_level</th>
                      <th>country_code</th>
                      <th>country_name</th>
                      <th>group</th>
                      <th>standard</th>
                      <th>age</th>
                      <th>apprenticeship level</th>
                      <th>demographic</th>
                      <th>sector_subject_area</th>
                      <th>overall_leavers</th>
                      <th>overall_achievers</th>
                      <th>overall_completers</th>
                      <th>overall_acheievement_rate</th>
                      <th>overall_retention_rate</th>
                      <th>overall_pass_rate</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr>
                      <td>201819</td>
                      <td>Academic year</td>
                      <td>National</td>
                      <td>E92000001</td>
                      <td>England</td>
                      <td>Ethnicity group</td>
                      <td>Framework</td>
                      <td>16-18</td>
                      <td>Advanced</td>
                      <td>Ethnic minorities (excluding white minorities)</td>
                      <td>Agriculture, Horticulture and Animal Care</td>
                      <td>10</td>
                      <td>~</td>
                      <td>~</td>
                      <td>~</td>
                      <td>~</td>
                      <td>~</td>
                    </tr>
                    {fullTable && (
                      <>
                        <tr>
                          <td>201819</td>
                          <td>Academic year</td>
                          <td>National</td>
                          <td>E92000001</td>
                          <td>England</td>
                          <td>Ethnicity group</td>
                          <td>Framework</td>
                          <td>16-18</td>
                          <td>Advanced</td>
                          <td>
                            Ethnic minorities (excluding white minorities)
                          </td>
                          <td>Arts, Media and Publishing</td>
                          <td>10</td>
                          <td>10</td>
                          <td>10</td>
                          <td>58.3</td>
                          <td>75</td>
                          <td>77.8</td>
                        </tr>
                        <tr>
                          <td>201819</td>
                          <td>Academic year</td>
                          <td>National</td>
                          <td>E92000001</td>
                          <td>England</td>
                          <td>Ethnicity group</td>
                          <td>Framework</td>
                          <td>16-18</td>
                          <td>Advanced</td>
                          <td>
                            Ethnic minorities (excluding white minorities)
                          </td>
                          <td>Business, Administration and Law</td>
                          <td>440</td>
                          <td>300</td>
                          <td>300</td>
                          <td>67.6</td>
                          <td>68.2</td>
                          <td>99</td>
                        </tr>
                        <tr>
                          <td>201819</td>
                          <td>Academic year</td>
                          <td>National</td>
                          <td>E92000001</td>
                          <td>England</td>
                          <td>Ethnicity group</td>
                          <td>Framework</td>
                          <td>16-18</td>
                          <td>Advanced</td>
                          <td>
                            Ethnic minorities (excluding white minorities)
                          </td>
                          <td>
                            Construction, Planning and the Built Environment
                          </td>
                          <td>60</td>
                          <td>40</td>
                          <td>40</td>
                          <td>74.5</td>
                          <td>76.4</td>
                          <td>97.6</td>
                        </tr>
                        <tr>
                          <td>201819</td>
                          <td>Academic year</td>
                          <td>National</td>
                          <td>E92000001</td>
                          <td>England</td>
                          <td>Ethnicity group</td>
                          <td>Framework</td>
                          <td>16-18</td>
                          <td>Advanced</td>
                          <td>
                            Ethnic minorities (excluding white minorities)
                          </td>
                          <td>Education and Training</td>
                          <td>50</td>
                          <td>30</td>
                          <td>30</td>
                          <td>62.5</td>
                          <td>70.8</td>
                          <td>88.2</td>
                        </tr>
                      </>
                    )}
                  </tbody>
                </table>
              </div>
              <div className="govuk-!-margin-bottom-6 govuk-!-margin-top-6">
                <a
                  href="#"
                  onClick={e => {
                    setFullWidth(!fullWidth);
                    e.preventDefault();
                  }}
                >
                  {fullWidth
                    ? 'Back to standard page view'
                    : 'View preview as full screen'}
                </a>
              </div>

              {!sectionShowAll && (
                <PrototypePrevNextNav
                  changeSectionState={changeSectionState}
                  nextId="changelog"
                  nextTitle="Changelog"
                  prevId="dataSummary"
                  prevTitle="Data set summary"
                />
              )}
              {sectionShowAll && <hr className="govuk-!-margin-bottom-9" />}
            </section>
          )}
          {(sectionShowAll || sectionSelected === 'changelog') && (
            <section id="changelog" className={pageStyles.sectionScroll}>
              <h2 className="govuk-heading-l">Changelog</h2>
              <h4>Version 1.0</h4>

              <h4 className="govuk-!-margin-bottom-0">New locations</h4>
              <ul className="govuk-!-margin-bottom-6">
                <li>Northumberland (new)</li>
                <li>Kingston upon Hull, City of (new)</li>
                <li>Leeds</li>
                <li>Doncaster</li>
              </ul>
              <h4 className="govuk-!-margin-bottom-0">Mapped locations</h4>
              <ul className="govuk-!-margin-bottom-6">
                <li>
                  Darlington <strong>maps to</strong> Darlington
                </li>
                <li>
                  Sheffield <strong>maps to</strong> Sheffield
                </li>
              </ul>
              <h4 className="govuk-!-margin-bottom-0">Unmapped locations</h4>
              <ul className="govuk-!-margin-bottom-6">
                <li>Chester</li>
                <li>Kingston upon Hull, City of</li>
                <li>Northumberland</li>
              </ul>
              <h4 className="govuk-!-margin-botttom-0">New filters</h4>
              <ul className="govuk-!-margin-bottom-6">
                <li>Age 10 (new)</li>
                <li>Ethnicity Major Asian Total (new)</li>
                <li>Age 11</li>
                <li>Age 12</li>
              </ul>
              <h4 className="govuk-!-margin-botttom-0">Mapped filters</h4>
              <ul className="govuk-!-margin-bottom-6">
                <li>
                  Ethnicity Major Black Total <strong>maps to</strong> Ethnicity
                  Major Black Total
                </li>
                <li>
                  Age 4 and under <strong>maps to</strong> Age 4 and under
                </li>
                <li>Unmapped filters</li>
                <li>Ethnicity Major Asian Total</li>
                <li>Age 10</li>
              </ul>
              <h4 className="govuk-!-margin-botttom-0">New indicators</h4>
              <ul className="govuk-!-margin-bottom-6">
                <li>Number of authorised holiday sessions (new)</li>
                <li>Authorised absence rate (new)</li>
                <li>Number of authorised other sessions</li>
                <li>Number of authorised reasons sessions</li>
              </ul>
              <h4 className="govuk-!-margin-botttom-0">Mapped indicators</h4>
              <ul className="govuk-!-margin-bottom-6">
                <li>
                  Number of excluded sessions <strong>maps to</strong> Number of
                  excluded sessions
                </li>
                <li>
                  Number of extended authorised holiday sessions{' '}
                  <strong>maps to</strong> Number of extended authorised holiday
                  sessions
                </li>
              </ul>
              <h4 className="govuk-!-margin-botttom-0">Unmapped indicators</h4>
              <ul className="govuk-!-margin-bottom-6">
                <li>
                  Authorised absence rate Number of authorised holiday sessions
                </li>
              </ul>
              {!sectionShowAll && (
                <PrototypePrevNextNav
                  changeSectionState={changeSectionState}
                  prevId="dataPreview"
                  prevTitle="Data preview"
                  nextId="versionHistory"
                  nextTitle="Version history"
                />
              )}
              {sectionShowAll && <hr className="govuk-!-margin-bottom-9" />}
            </section>
          )}
          {(sectionShowAll || sectionSelected === 'versionHistory') && (
            <section id="versionHistory" className={pageStyles.sectionScroll}>
              <h2 className="govuk-heading-l" id="versionHistory">
                Version history
              </h2>
              <table className="govuk-!-margin-bottom-9">
                <caption className="govuk-visually-hidden">
                  <h3>API data set version history</h3>
                </caption>
                <thead>
                  <tr>
                    <th>Version</th>
                    <th>Related release</th>
                    <th>Status</th>
                  </tr>
                </thead>
                <tbody>
                  <tr>
                    <td>
                      <a
                        href="#"
                        onClick={_ => {
                          setSectionSelected('dataSummary');
                        }}
                      >
                        2.0
                      </a>
                    </td>
                    <td>Academic year 2021/22</td>
                    <td>
                      <span className="govuk-tag">Active</span>
                    </td>
                  </tr>
                  <tr>
                    <td>
                      <a
                        href="#"
                        onClick={_ => {
                          setSectionSelected('dataSummary');
                        }}
                      >
                        1.1
                      </a>
                    </td>
                    <td>Academic year 2020/21</td>
                    <td>
                      <span className="govuk-tag">Active</span>
                    </td>
                  </tr>
                  <tr>
                    <td>
                      <a
                        href="#"
                        onClick={_ => {
                          setSectionSelected('dataSummary');
                        }}
                      >
                        1.0
                      </a>
                    </td>
                    <td>Academic year 2019/20</td>
                    <td>
                      <span className="govuk-tag govuk-tag--red">
                        Deprecated
                      </span>
                    </td>
                  </tr>
                </tbody>
              </table>
              {!sectionShowAll && (
                <PrototypePrevNextNav
                  changeSectionState={changeSectionState}
                  nextId="endPoints"
                  nextTitle="API endpoints quick start"
                  prevId="changelog"
                  prevTitle="Changelog"
                />
              )}
            </section>
          )}
          {(sectionShowAll || sectionSelected === 'endPoints') && (
            <section id="endPoints" className={pageStyles.sectionScroll}>
              <h2 className="govuk-heading-l">API endpoints quick start</h2>
              <div className="govuk-inset-text">
                <p className="govuk-hint">
                  If you are unfamiliar with APIs, we suggest you first read our{' '}
                  <Link
                    to="https://dfe-analytical-services.github.io/explore-education-statistics-api-docs/"
                    target="blank"
                  >
                    API documentation
                  </Link>
                </p>
                <p className="govuk-hint">
                  The documentation provides full guidance and examples on how
                  to make the most from our data sets
                </p>
              </div>
              <h3>Data set summary</h3>
              <div className="dfe-flex dfe-align-items--baseline">
                <div className="govuk-!-margin-right-3">GET</div>
                <UrlContainer
                  className="govuk-!-margin-bottom-2"
                  id="data-set-summary-endpoint"
                  url="https://ees-api-mock.ambitiousocean-cb084d07.uksouth.azurecontainerapps.io/api/v1/data-sets/9eee125b-5538-49b8-aa49-4fda877b5e57"
                />
              </div>
              <a href="https://dfe-analytical-services.github.io/explore-education-statistics-api-docs/endpoints/GetDataSet/">
                Guidance, get data set summary
              </a>
              <h3 className="govuk-!-margin-top-9">Data set metadata</h3>
              <div className="dfe-flex dfe-align-items--baseline">
                <div className="govuk-!-margin-right-3">GET</div>
                <UrlContainer
                  className="govuk-!-margin-bottom-2"
                  id="data-set-meta-endpoint"
                  url="https://ees-api-mock.ambitiousocean-cb084d07.uksouth.azurecontainerapps.io/api/v1/data-sets/9eee125b-5538-49b8-aa49-4fda877b5e57/meta"
                />
              </div>
              <a href="https://dfe-analytical-services.github.io/explore-education-statistics-api-docs/endpoints/GetDataSetMeta/">
                Guidance, get data set metadata
              </a>
              <h3 className="govuk-!-margin-top-9">Query data set</h3>
              <h4>Using GET</h4>
              <div className="dfe-flex dfe-align-items--baseline">
                <div className="govuk-!-margin-right-3">GET</div>
                <UrlContainer
                  className="govuk-!-margin-bottom-2"
                  id="data-set-get-query-endpoint"
                  url="https://ees-api-mock.ambitiousocean-cb084d07.uksouth.azurecontainerapps.io/api/v1/data-sets/9eee125b-5538-49b8-aa49-4fda877b5e57/query"
                />
              </div>
              <a href="https://dfe-analytical-services.github.io/explore-education-statistics-api-docs/endpoints/QueryDataSetGet/">
                Guidance, query data set (GET)
              </a>
              <h4 className="govuk-!-margin-top-6">Using POST</h4>
              <div className="dfe-flex dfe-align-items--baseline">
                <div className="govuk-!-margin-right-3">POST</div>
                <UrlContainer
                  className="govuk-!-margin-bottom-2"
                  id="data-post-summary-endpoint"
                  url="https://ees-api-mock.ambitiousocean-cb084d07.uksouth.azurecontainerapps.io/api/v1/data-sets/9eee125b-5538-49b8-aa49-4fda877b5e57/query"
                />
              </div>
              <a href="https://dfe-analytical-services.github.io/explore-education-statistics-api-docs/endpoints/QueryDataSetPost/">
                Guidance, query data set (POST)
              </a>
              {!sectionShowAll && (
                <PrototypePrevNextNav
                  changeSectionState={changeSectionState}
                  nextId="download"
                  nextTitle="Download data or create tables"
                  prevId="versionHistory"
                  prevTitle="Version history"
                />
              )}
              {sectionShowAll && <hr className="govuk-!-margin-bottom-9" />}
            </section>
          )}
          {(sectionShowAll || sectionSelected === 'download') && (
            <section id="download" className={pageStyles.sectionScroll}>
              <h2 className="govuk-heading-l" id="download">
                Download data or create tables
              </h2>
              <div className={pageStyles.prototypeCardContainer}>
                <div
                  className={classNames(pageStyles.prototypeCardChevronOneHalf)}
                >
                  <h2
                    className={classNames(
                      'govuk-heading-m',
                      'govuk-!-margin-bottom-2',
                    )}
                  >
                    <a
                      href="#"
                      className={classNames(
                        pageStyles.prototypeCardChevronLink,
                        'govuk-link--no-visited-state',
                      )}
                    >
                      Download file (csv, 2 Mb)
                    </a>
                  </h2>
                  <p className="govuk-body govuk-!-margin-bottom-0">
                    Individual open data file from our data catlogue
                  </p>
                </div>

                <div
                  className={classNames(pageStyles.prototypeCardChevronOneHalf)}
                >
                  <h2
                    className={classNames(
                      'govuk-heading-m',
                      'govuk-!-margin-bottom-2',
                    )}
                  >
                    <a
                      href="/prototypes/table-highlights-2?source=dataCat&dataset=ratesDetailed"
                      className={classNames(
                        pageStyles.prototypeCardChevronLink,
                        'govuk-link--no-visited-state',
                      )}
                    >
                      Create your own table
                    </a>
                  </h2>
                  <p className="govuk-body govuk-!-margin-bottom-0">
                    Explore this dataset with our table tool
                  </p>
                </div>
              </div>
              {!sectionShowAll && (
                <PrototypePrevNextNav
                  changeSectionState={changeSectionState}
                  prevId="endPoints"
                  prevTitle="API endpoints quick start"
                />
              )}
              <div style={{ marginBottom: '90vh' }} />
            </section>
          )}
        </div>
      </div>
      <div
        className={classNames(
          pageStyles.prototypeCardContainer,
          pageStyles.prototypeCardBg,
          'govuk-visually-hidden',
        )}
      >
        <div
          className={classNames(
            pageStyles.prototypeCardChevronOneThird,
            'govuk-!-padding-right-0',
          )}
        >
          <h2
            className={classNames('govuk-heading-m', 'govuk-!-margin-bottom-3')}
          >
            <a
              href="/prototypes/find-statistics6"
              className={classNames(
                'govuk-heading-m',
                'govuk-!-margin-bottom-2',
              )}
            >
              Download file (csv, 2 Mb)
            </a>
          </h2>
          <p className="govuk-body govuk-!-margin-bottom-0">
            Individual open data file from our data catalogue
          </p>
        </div>
        <div className={classNames(pageStyles.prototypeCardChevronOneThird)}>
          <h2
            className={classNames('govuk-heading-m', 'govuk-!-margin-bottom-2')}
          >
            <a
              href="/prototypes/table-tool"
              className={classNames(
                pageStyles.prototypeCardChevronLink,
                'govuk-link--no-visited-state',
              )}
            >
              View featured tables
            </a>
          </h2>
          <p className="govuk-body govuk-!-margin-bottom-0">
            Ready made tables created using this dataset
          </p>
        </div>
        <div className={classNames(pageStyles.prototypeCardChevronOneThird)}>
          <h2
            className={classNames('govuk-heading-m', 'govuk-!-margin-bottom-2')}
          >
            <a
              href="/prototypes/table-tool"
              className={classNames(
                pageStyles.prototypeCardChevronLink,
                'govuk-link--no-visited-state',
              )}
            >
              Create your own table
            </a>
          </h2>
          <p className="govuk-body govuk-!-margin-bottom-0">
            Explore this data in our table tool
          </p>
        </div>
      </div>
    </div>
  );
};

export default PrototypeAPIDataSetPreview;
