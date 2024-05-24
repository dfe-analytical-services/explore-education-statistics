import PrototypePage from '@admin/prototypes/components/PrototypePage';
import React, { useState } from 'react';
import classNames from 'classnames';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import RelatedInformation from '@common/components/RelatedInformation';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import UrlContainer from '@common/components/UrlContainer';
import Link from '../components/Link';
import styles from './PrototypePublicPage.module.scss';

const PrototypeHomepage = () => {
  const params = new URLSearchParams(window.location.search);
  const urlDataType = params.get('dataType');
  const latestRelease = 'Academic year 2021/22';
  const [fullWidth, setFullWidth] = useState(false);
  const [fullTable, setFullTable] = useState(false);
  const [selectedRelease, setSelectedRelease] = useState(latestRelease);
  const [dataType, setDataType] = useState(urlDataType || 'api');
  return (
    <div
      className={classNames(
        styles.prototypePublicPage,
        styles.prototypeHideBreadcrumb2,
        fullWidth
          ? styles.prototypeFullWidthOveride
          : styles.prototypeStandardWidth,
      )}
    >
      <PrototypePage
        wide={fullWidth}
        backLink={`./data-catalog?theme=fe&publication=traineeships&dataType=${dataType}`}
        backLinkText=" Back to apprenticeships and traineeships data catalogue"
      >
        <>
          <h1 className="govuk-heading-xl">
            <span className="govuk-caption-xl ">Data catalogue v3</span>{' '}
            Apprenticeship Achievement Rates Detailed Series {dataType}
          </h1>

          <div className="govuk-grid-row">
            <div className="govuk-grid-column-two-thirds">
              <span className="govuk-tag">National statistics</span>{' '}
              {selectedRelease === latestRelease && (
                <span className="govuk-tag">latest data</span>
              )}
              {selectedRelease !== latestRelease && (
                <span className="govuk-tag govuk-tag--red">
                  Not the latest data
                </span>
              )}
              {selectedRelease === 'Academic year 2019/20' && (
                <div className="govuk-inset-text">
                  <div className="govuk-warning-text">
                    <span
                      className="govuk-warning-text__icon"
                      aria-hidden="true"
                    >
                      !
                    </span>
                    <strong className="govuk-warning-text__text">
                      <span className="govuk-warning-text__assistive">
                        Warning
                      </span>
                      <p>
                        <strong>
                          This version of the API data set has been deprecated.
                        </strong>
                      </p>
                      <p>
                        <a
                          href="#"
                          onClick={_ => {
                            setSelectedRelease(latestRelease);
                          }}
                        >
                          View the latest available data set: {latestRelease}
                        </a>
                      </p>
                    </strong>
                  </div>
                </div>
              )}
              {selectedRelease !== latestRelease &&
                selectedRelease !== 'Academic year 2019/20' && (
                  <p className="govuk-!-margin-top-6">
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
              <p className="govuk-body-l govuk-!-margin-top-3 govuk-!-margin-bottom-3">
                Apprenticeship national achievement rate tables
              </p>
              {/*
              <a
                href="/prototypes/find-statistics6"
                className={classNames(
                  'govuk-button',
                  'govuk-link--no-visited-state',
                )}
              >
                Download file (csv, 2 Mb)
              </a>
                */}
              {!fullWidth && (
                <SummaryList noBorder className="govuk-!-margin-bottom-9">
                  {dataType === 'api' && (
                    <SummaryListItem term="API status">
                      {selectedRelease === latestRelease && (
                        <>
                          <span className="govuk-tag govuk-tag--turquoise">
                            ACTIVE
                          </span>{' '}
                          Version 2.0
                        </>
                      )}
                      {selectedRelease !== latestRelease && (
                        <>
                          {selectedRelease === 'Academic year 2020/21' && (
                            <>
                              <span className="govuk-tag govuk-tag--turquoise">
                                ACTIVE
                              </span>{' '}
                              Version 1.1
                            </>
                          )}
                          {selectedRelease === 'Academic year 2019/20' && (
                            <>
                              <span className="govuk-tag govuk-tag--red">
                                DEPRECATED
                              </span>{' '}
                              Version 1.0
                            </>
                          )}
                        </>
                      )}
                    </SummaryListItem>
                  )}

                  <SummaryListItem term="Theme">
                    Further education
                  </SummaryListItem>
                  <SummaryListItem term="Publication">
                    Apprenticeships and traineeships
                  </SummaryListItem>
                  <SummaryListItem term="Related release">
                    {selectedRelease}
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
                    Age, Level, demographic - ethnicity, gender and lldd,
                    Standard /Framework flag
                  </SummaryListItem>
                </SummaryList>
              )}
            </div>
            <div className="govuk-grid-column-one-third">
              <RelatedInformation heading="Quick links">
                {dataType === 'api' && (
                  <>
                    <ul className="govuk-list">
                      <li>
                        <Link
                          to="https://dfe-analytical-services.github.io/explore-education-statistics-api-docs/"
                          className={classNames(
                            'govuk-button',
                            'govuk-link--no-visited-state',
                            'govuk-!-margin-bottom-0',
                          )}
                        >
                          API documentatiom
                        </Link>
                      </li>
                    </ul>
                    <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
                      On this page
                    </h3>
                    <ul className="govuk-list ">
                      <li>
                        <a href="#api-preview-tabs-3">Changelog</a>
                      </li>
                      <li>
                        <a href="#api-preview-tabs-2">
                          API endpoint quick start
                        </a>
                      </li>
                    </ul>
                  </>
                )}
                {dataType === 'csv' && (
                  <ul className="govuk-list govuk-list--spaced">
                    <li>
                      <a
                        href="/prototypes/find-statistics6"
                        className={classNames(
                          'govuk-button',
                          'govuk-link--no-visited-state',
                          'govuk-!-margin-bottom-3',
                        )}
                      >
                        Download file (csv, 2 Mb)
                      </a>
                    </li>
                    <li>
                      <a
                        href="#"
                        onClick={_ => {
                          setDataType('api');
                        }}
                      >
                        API available for this data set
                      </a>
                    </li>
                    <li>
                      <a
                        href="./table-highlights-2?source=dataCat&dataset=ratesDetailed"
                        target="_blank"
                      >
                        View or create your own tables
                      </a>
                    </li>
                  </ul>
                )}
                {!fullWidth && (
                  <>
                    <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
                      Publication
                    </h3>
                    <ul className="govuk-list">
                      <li>
                        <a href="./releaseData">
                          Apprenticeships and traineeships
                        </a>
                      </li>
                    </ul>
                    <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
                      Methodologies
                    </h3>
                    <ul className="govuk-list">
                      <li>
                        <Link to="#" target="_blank">
                          Further education and skills statistics: methodology
                        </Link>
                      </li>
                    </ul>
                    <div
                      className="govuk-form-group govuk-!-margin-bottom-6"
                      style={{ position: 'relative' }}
                    >
                      <h2 className="govuk-label-wrapper">
                        <label
                          className="govuk-label govuk-label--s"
                          htmlFor="pubilication"
                        >
                          All related releases
                        </label>
                      </h2>

                      <select
                        className="govuk-select"
                        id="release"
                        onBlur={e => {
                          setSelectedRelease(e.target.value);
                        }}
                      >
                        <option value={latestRelease}>Latest release</option>
                        <option value={latestRelease}>
                          v2.0 {latestRelease}
                        </option>
                        <option value="Academic year 2020/21">
                          v1.1 Academic year 2020/21
                        </option>
                        <option value="Academic year 2019/20">
                          v1.0 Academic year 2019/20
                        </option>
                      </select>
                    </div>
                  </>
                )}
              </RelatedInformation>
            </div>
          </div>

          <div
            className={classNames(
              styles.prototypeCardContainer,
              styles.prototypeCardBg,
              'govuk-visually-hidden',
            )}
          >
            <div
              className={classNames(
                styles.prototypeCardChevronOneThird,
                'govuk-!-padding-right-0',
              )}
            >
              <h2
                className={classNames(
                  'govuk-heading-m',
                  'govuk-!-margin-bottom-3',
                )}
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
            <div className={classNames(styles.prototypeCardChevronOneThird)}>
              <h2
                className={classNames(
                  'govuk-heading-m',
                  'govuk-!-margin-bottom-2',
                )}
              >
                <a
                  href="/prototypes/table-tool"
                  className={classNames(
                    styles.prototypeCardChevronLink,
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
            <div className={classNames(styles.prototypeCardChevronOneThird)}>
              <h2
                className={classNames(
                  'govuk-heading-m',
                  'govuk-!-margin-bottom-2',
                )}
              >
                <a
                  href="/prototypes/table-tool"
                  className={classNames(
                    styles.prototypeCardChevronLink,
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

          <h2 className="govuk-heading-l">
            {dataType === 'api'
              ? 'API data preview, endpoints and changelog'
              : 'Data preview'}
          </h2>

          <Tabs id="api-preview-tabs">
            <TabsSection title="Data preview">
              <h3>Data preview</h3>
              <div style={{ maxWidth: '100%', overflow: 'auto' }}>
                <table className="govuk-table" style={{ width: '3500px' }}>
                  <caption
                    className="govuk-!-margin-bottom-3"
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
                    e.preventDefault();
                    setFullTable(!fullTable);
                  }}
                >
                  {fullTable ? 'Show first row only' : 'View more rows'}
                </a>
                {` `}
                <a
                  href="#"
                  className="govuk-!-margin-left-6"
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
            </TabsSection>
            {dataType === 'api' && (
              <TabsSection title="API endpoint quick start">
                <div className="govuk-!-margin-bottom-9">
                  <h3>API endpoints</h3>
                  <div className="govuk-inset-text">
                    <p className="govuk-hint">
                      If you are unfamiliar using APIs, we suggest you first
                      read our{' '}
                      <Link
                        to="https://dfe-analytical-services.github.io/explore-education-statistics-api-docs/"
                        target="blank"
                      >
                        API guidance documentation
                      </Link>
                    </p>
                    <p className="govuk-hint">
                      The documentation provides full details and examples on
                      how to make the most from our data sets
                    </p>
                  </div>

                  <h4>Data set summary</h4>
                  <UrlContainer
                    className="govuk-!-margin-bottom-2"
                    id="data-set-summary-endpoint"
                    url="GET https://ees-api-mock.ambitiousocean-cb084d07.uksouth.azurecontainerapps.io/api/v1/data-sets/{dataSetId}"
                  />
                  <a href="https://dfe-analytical-services.github.io/explore-education-statistics-api-docs/endpoints/GetDataSet/">
                    View endpoint summary guidance
                  </a>

                  <h4 className="govuk-!-margin-top-9">Query data set</h4>
                  <h5>Using GET</h5>
                  <UrlContainer
                    className="govuk-!-margin-bottom-6"
                    id="data-set-get-query-endpoint"
                    url="GET https://ees-api-mock.ambitiousocean-cb084d07.uksouth.azurecontainerapps.io/api/v1/data-sets/{dataSetId}/query"
                  />
                  <h5>Using POST</h5>
                  <UrlContainer
                    className="govuk-!-margin-bottom-2"
                    id="data-set-post-query-endpoint"
                    url="POST https://ees-api-mock.ambitiousocean-cb084d07.uksouth.azurecontainerapps.io/api/v1/data-sets/{dataSetId}/query"
                  />
                  <a href="https://dfe-analytical-services.github.io/explore-education-statistics-api-docs/endpoints/QueryDataSetGet/">
                    View endpoint query guidance
                  </a>
                </div>
              </TabsSection>
            )}
            {dataType === 'api' && (
              <TabsSection title="Changelog">
                <h3>Changelog</h3>
                <h4>
                  {selectedRelease === latestRelease && <>Version 2.0</>}
                  {selectedRelease !== latestRelease && (
                    <>
                      {selectedRelease === 'Academic year 2020/21' && (
                        <>Version 1.1</>
                      )}
                      {selectedRelease === 'Academic year 2019/20' && (
                        <>Version 1.0</>
                      )}
                    </>
                  )}
                </h4>
                {selectedRelease === latestRelease && (
                  <div className="govuk-inset-text">
                    This version includes some structural changes, these are
                    highlighted in the changelog, this is likely to cause
                    breaking changes to any existing queries you may have set
                    up.
                  </div>
                )}
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
                    Ethnicity Major Black Total <strong>maps to</strong>{' '}
                    Ethnicity Major Black Total
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
                    Number of excluded sessions <strong>maps to</strong> Number
                    of excluded sessions
                  </li>
                  <li>
                    Number of extended authorised holiday sessions{' '}
                    <strong>maps to</strong> Number of extended authorised
                    holiday sessions
                  </li>
                </ul>
                <h4 className="govuk-!-margin-botttom-0">
                  Unmapped indicators
                </h4>
                <ul className="govuk-!-margin-bottom-6">
                  <li>
                    Authorised absence rate Number of authorised holiday
                    sessions
                  </li>
                </ul>
              </TabsSection>
            )}
            {dataType === 'api' && (
              <TabsSection title="Version history">
                <table>
                  <caption>API data set version history</caption>
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
                            setSelectedRelease('Academic year 2021/22');
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
                            setSelectedRelease('Academic year 2020/21');
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
                            setSelectedRelease('Academic year 2019/20');
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
              </TabsSection>
            )}
          </Tabs>

          <h2 className="govuk-heading-l">Access and view data</h2>

          <div className={styles.prototypeCardContainer}>
            <div className={classNames(styles.prototypeCardChevronOneThird)}>
              <h2
                className={classNames(
                  'govuk-heading-m',
                  'govuk-!-margin-bottom-2',
                )}
              >
                <a
                  href="#"
                  className={classNames(
                    styles.prototypeCardChevronLink,
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
            <div className={classNames(styles.prototypeCardChevronOneThird)}>
              <h2
                className={classNames(
                  'govuk-heading-m',
                  'govuk-!-margin-bottom-2',
                )}
              >
                <a
                  href="/prototypes/table-highlights-2?source=dataCat&dataset=ratesDetailed"
                  className={classNames(
                    styles.prototypeCardChevronLink,
                    'govuk-link--no-visited-state',
                  )}
                >
                  View featured tables
                </a>
              </h2>
              <p className="govuk-body govuk-!-margin-bottom-0">
                Pre-built ready made tables created using this dataset
              </p>
            </div>
            <div className={classNames(styles.prototypeCardChevronOneThird)}>
              <h2
                className={classNames(
                  'govuk-heading-m',
                  'govuk-!-margin-bottom-2',
                )}
              >
                <a
                  href="/prototypes/table-highlights-2?source=dataCat&dataset=ratesDetailed"
                  className={classNames(
                    styles.prototypeCardChevronLink,
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

          {/*
          <h2 className="govuk-heading-l govuk-!-margin-top-9">Meta data</h2>

          <SummaryList noBorder>
            <SummaryListItem term="Indicators">
              Starts, Achievements, Learner participation, Percentage Starts,
              Percentage Achievements, Percentage Learner participation
            </SummaryListItem>
            <SummaryListItem term="Filters">
              Apprenticeship level, Funding type, Age group
            </SummaryListItem>
            <SummaryListItem term="Geographic level">National</SummaryListItem>
            <SummaryListItem term="Time period">
              Academic years 2018/19 to 2020/21
            </SummaryListItem>
          </SummaryList>

                  */}
        </>
      </PrototypePage>
    </div>
  );
};

export default PrototypeHomepage;
