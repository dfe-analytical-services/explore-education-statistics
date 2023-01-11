import PrototypePage from '@admin/prototypes/components/PrototypePage';
import React, { useState } from 'react';
import classNames from 'classnames';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import RelatedInformation from '@common/components/RelatedInformation';
import Link from '../components/Link';
import styles from './PrototypePublicPage.module.scss';
import imageDataPreview from './images/dataDownload.png';

const PrototypeHomepage = () => {
  const latestRelease = 'Academic year 2021/22';
  const [fullWidth, setFullWidth] = useState(false);
  const [selectedRelease, setSelectedRelease] = useState(latestRelease);
  return (
    <div
      className={classNames(
        styles.prototypePublicPage,
        fullWidth
          ? styles.prototypeFullWidthOveride
          : styles.prototypeStandardWidth,
      )}
    >
      <PrototypePage
        wide={fullWidth}
        breadcrumbs={[
          { name: 'Data catalogue', link: '/prototypes/data-catalog' },
          {
            name: 'Further education',
            link: '/prototypes/data-catalog?theme=fe',
          },
          {
            name: 'Apprenticeships and traineeships',
            link: '/prototypes/data-catalog?theme=fe&publication=traineeships',
          },
          {
            name: selectedRelease,
            link: '/prototypes/data-catalog?theme=fe&publication=traineeships',
          },
        ]}
      >
        <>
          <h1 className="govuk-heading-xl govuk-!-margin-bottom-2">
            <span className="govuk-caption-xl ">Data catalogue</span>{' '}
            Apprenticeship Achievement Rates Detailed Series
          </h1>
          <Link
            to="./data-catalog?theme=fe&publication=traineeships"
            back
            className="govuk-!-margin-bottom-6"
          >
            Back to apprenticeships and traineeships data catalogue
          </Link>
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
              {selectedRelease !== latestRelease && (
                <p className="govuk-!-margin-top-6">
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
                  <SummaryListItem term="Theme">
                    Further education
                  </SummaryListItem>
                  <SummaryListItem term="Publication">
                    Apprenticeships and traineeships
                  </SummaryListItem>
                  <SummaryListItem term="Release">
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
              <RelatedInformation heading="Download options">
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
                {!fullWidth && (
                  <>
                    <ul className="govuk-list govuk-list--spaced">
                      <li>
                        <Link
                          to="./table-highlights-2?source=dataCat&dataset=ratesDetailed"
                          target="_blank"
                        >
                          Create your own tables
                        </Link>
                      </li>
                      <li>
                        <Link
                          to="./table-highlights-2?source=dataCat&dataset=ratesDetailed"
                          target="_blank"
                        >
                          Pre-built featured tables
                        </Link>
                      </li>
                    </ul>

                    <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
                      Publication
                    </h3>
                    <ul className="govuk-list">
                      <li>
                        <Link to="./releaseData">
                          Apprenticeships and traineeships, Academic Year
                          2021/22
                        </Link>
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
                          All releases
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
                        <option value={latestRelease}>{latestRelease}</option>
                        <option value="Academic year 2020/21">
                          Academic year 2020/21
                        </option>
                        <option value="Academic year 2019/20">
                          Academic year 2019/20
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

          <h2 className="govuk-heading-l">Data preview</h2>

          <div style={{ maxWidth: '100%', overflow: 'auto' }}>
            <table className="govuk-table">
              <caption
                className="govuk-!-margin-bottom-3"
                style={{ fontWeight: 'normal' }}
              >
                Snapshot showing first 5 rows of XXXXX, taken from CSV file
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
                  <td>Ethnic minorities (excluding white minorities)</td>
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
                  <td>Ethnic minorities (excluding white minorities)</td>
                  <td>Construction, Planning and the Built Environment</td>
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
                  <td>Ethnic minorities (excluding white minorities)</td>
                  <td>Education and Training</td>
                  <td>50</td>
                  <td>30</td>
                  <td>30</td>
                  <td>62.5</td>
                  <td>70.8</td>
                  <td>88.2</td>
                </tr>
              </tbody>
            </table>
          </div>

          <div className="govuk-!-margin-top-3 govuk-!-margin-bottom-9">
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
