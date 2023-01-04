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
  const [fullWidth, setFullWidth] = useState(false);
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
            name: 'Academic Year 2021/22',
            link: '/prototypes/data-catalog?theme=fe&publication=traineeships',
          },
        ]}
      >
        <>
          <h1 className="govuk-heading-xl">
            <span className="govuk-caption-xl">Data catalogue</span>{' '}
            Apprenticeship Achievement Rates Detailed Series
          </h1>
          <div className="govuk-grid-row">
            <div className="govuk-grid-column-two-thirds">
              <span className="govuk-tag">National statistics</span>{' '}
              <span className="govuk-tag">latest data</span>
              <p className="govuk-body-l govuk-!-margin-top-3">
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
                    <ul className="govuk-list">
                      <li>
                        <Link to="#" target="_blank">
                          Create your own tables
                        </Link>
                      </li>
                      <li>
                        <Link to="#" target="_blank">
                          Pre-built featured tables
                        </Link>
                      </li>
                    </ul>

                    <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
                      Publication
                    </h3>
                    <ul className="govuk-list">
                      <li>
                        <Link to="#" target="_blank">
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
                          Other releases
                        </label>
                      </h2>

                      <select className="govuk-select" id="release">
                        <option value="Latest release">Latest release</option>
                        <option value="Academic year 2021/22">
                          Academic year 2021/22
                        </option>
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

          <p className="govuk-hint">
            Snapshot image showing first 10 rows of XXXXX, taken from CSV file.{' '}
          </p>

          <div
            style={{ maxWidth: '100%', maxHeight: '220px', overflow: 'auto' }}
          >
            <img src={imageDataPreview} alt="Data preview snapshot" />
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

          {/*
          <table className="govuk-table">
            <thead>
              <tr>
                <td colSpan={2} rowSpan={1}></td>
                <th
                  colSpan={1}
                  rowSpan={1}
                  scope="col"
                  className="govuk-table__cell--numeric"
                >
                  2018/19
                </th>
                <th
                  colSpan={1}
                  rowSpan={1}
                  scope="col"
                  className="govuk-table__cell--numeric"
                >
                  2019/20
                </th>
                <th
                  colSpan={1}
                  rowSpan={1}
                  scope="col"
                  className="govuk-table__cell--numeric"
                >
                  2020/21
                </th>
              </tr>
            </thead>
            <tbody>
              <tr>
                <th rowSpan={6} colSpan={1} scope="rowgroup">
                  Ethnicity group
                </th>
                <th rowSpan={1} colSpan={1} scope="row">
                  Achievement Rate
                </th>
                <td className="govuk-table__cell--numeric">65.1%</td>
                <td className="govuk-table__cell--numeric">57.5%</td>
                <td className="govuk-table__cell--numeric">57.7%</td>
              </tr>
              <tr>
                <th rowSpan={1} colSpan={1} scope="row">
                  Achievers
                </th>
                <td className="govuk-table__cell--numeric">192,130 </td>
                <td className="govuk-table__cell--numeric">155,440</td>
                <td className="govuk-table__cell--numeric">158,780</td>
              </tr>
              <tr>
                <th rowSpan={1} colSpan={1} scope="row">
                  Completers
                </th>
                <td className="govuk-table__cell--numeric">195,040 </td>
                <td className="govuk-table__cell--numeric">158,740</td>
                <td className="govuk-table__cell--numeric">161,900</td>
              </tr>
              <tr>
                <th rowSpan={1} colSpan={1} scope="row">
                  Pass Rate
                </th>
                <td className="govuk-table__cell--numeric">98.5% </td>
                <td className="govuk-table__cell--numeric">97.9% </td>
                <td className="govuk-table__cell--numeric">98.1%</td>
              </tr>
              <tr>
                <th rowSpan={1} colSpan={1} scope="row">
                  Retention Rate
                </th>
                <td className="govuk-table__cell--numeric">66.1% </td>
                <td className="govuk-table__cell--numeric">58.7% </td>
                <td className="govuk-table__cell--numeric">58.8%</td>
              </tr>
            </tbody>
          </table>

                  

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
