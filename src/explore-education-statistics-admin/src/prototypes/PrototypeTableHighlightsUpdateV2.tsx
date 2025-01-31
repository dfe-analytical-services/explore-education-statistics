import classNames from 'classnames';
import Link from '@admin/components/Link';
import Details from '@common/components/Details';
import PageSearchForm from '@common/components/PageSearchForm';
import PrototypePage from '@admin/prototypes/components/PrototypePage';
import PrototypeFeaturedLinks from '@admin/prototypes/components/PrototypeFeaturedLinks';
import PrototypeStep2Options from '@admin/prototypes/components/PrototypeTableStep2Options';
import React, { useState } from 'react';
import stylesWiz from '@common/modules/table-tool/components/Wizard.module.scss';
import stylesStep from '@common/modules/table-tool/components/WizardStep.module.scss';
import stylesWizSummary from '@common/modules/table-tool/components/WizardStepSummary.module.scss';
import stylesStepHeading from '@common/modules/table-tool/components/WizardStepHeading.module.scss';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import stylesPublicView from './PrototypePublicPage.module.scss';

const styles = {
  ...stylesPublicView,
  ...stylesWiz,
  ...stylesStep,
  ...stylesStepHeading,
  ...stylesWizSummary,
};

const PrototypeTableHighlights = () => {
  const params = new URLSearchParams(window.location.search);
  const urlSource = params.get('source');
  const urlDataset = params.get('dataset');

  const [dataset, setDataset] = useState(
    urlDataset === 'ratesDetailed' ? 'subject-3' : '',
  );
  const [datasetValue, setDatasetValue] = useState(
    urlDataset === 'ratesDetailed'
      ? 'Apprenticeship Achievement Rates  Detailed series'
      : '',
  );
  const publication = 'pub-1';

  const [sourcePublication] = useState(urlSource === 'publicationPage');

  const [sourceDataCat] = useState(urlSource === 'dataCat');

  return (
    <div
      className={classNames(
        styles.prototypePublicPage,
        styles.prototypeTableTool,
        sourcePublication && styles.prototypeHideBreadcrumb,
      )}
    >
      <PrototypePage
        wide
        breadcrumbs={[
          {
            name: 'Explore our datasets',
            link: '#',
          },
          { name: 'Further education', link: '#' },
          { name: 'Apprenticeships and traineeships', link: '#' },
          { name: datasetValue, link: '#' },
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
            <h1 className={classNames('govuk-heading-xl')}>
              <span className="govuk-caption-xl">Table tool</span>
              Explore our datasets
            </h1>

            {sourceDataCat && (
              <div className="govuk-!-margin-bottom-4">
                <Link to="./data-selected" back>
                  Back to data catalogue
                </Link>
              </div>
            )}
          </div>
        </div>
        <div className="govuk-!-margin-bottom-9">
          <ol className={styles.stepNav} id="tableToolWizard">
            <li
              aria-current="step"
              className={classNames(styles.step)}
              data-testid="wizardStep-1"
              id="1"
              tabIndex={-1}
            >
              <div
                className={classNames(styles.content, styles.contentSmall)}
                style={{ maxWidth: '1800px' }}
              >
                <div
                  className={styles.container}
                  style={{ borderBottom: '2px solid #f3f2f1' }}
                >
                  <div className="govuk-grid-row govuk-!-width-full">
                    <div className="govuk-grid-column-one-quarter">
                      <h2 className="dfe-flex">
                        <div className="govuk-tag govuk-tag govuk-tag--grey">
                          Step 1
                        </div>
                        <div className="govuk-heading-s govuk-!-margin-bottom-0 govuk-!-margin-left-3">
                          {' '}
                          Choose a publication
                        </div>
                      </h2>
                    </div>
                    <div className="dfe-flex dfe-justify-content--space-between  govuk-grid-column-three-quarters">
                      <div>Apprenticeships and traineeships</div>
                      <div>
                        <a
                          href="./releaseData"
                          className="govuk-!-margin-right-6"
                        >
                          View this release
                        </a>

                        <a
                          href="https://explore-education-statistics.service.gov.uk/data-tables"
                          className="govuk-!-margin-right-6"
                        >
                          Change publication
                        </a>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </li>
            <li
              aria-current="step"
              className={classNames(styles.step, styles.stepActive)}
              data-testid="wizardStep-2"
              id="2"
              tabIndex={-1}
            >
              <div
                className={classNames(styles.content, styles.contentSmall)}
                style={{ maxWidth: '1800px' }}
              >
                <div className={styles.container} style={{ flexWrap: 'wrap' }}>
                  <div
                    className={classNames(styles.content, styles.contentSmall)}
                    style={{ maxWidth: '1800px' }}
                  >
                    <div className="govuk-grid-row govuk-!-width-full">
                      <div className="govuk-grid-column-one-quarter">
                        <h2 className="dfe-flex">
                          <div className="govuk-tag govuk-tag govuk-tag--turquoise">
                            Step 2
                          </div>
                          <div className="govuk-heading-s govuk-!-margin-bottom-0 govuk-!-margin-left-3">
                            {' '}
                            Dataset or featured table
                          </div>
                        </h2>
                      </div>
                      <div className="dfe-flex dfe-justify-content--space-between  govuk-grid-column-three-quarters">
                        {!datasetValue && <h3>No dataset selected</h3>}
                      </div>
                    </div>
                  </div>
                  <form>
                    <div className="govuk-grid-row govuk-!-width-full">
                      <div className="govuk-grid-column-one-quarter">
                        <div>
                          <a
                            href="#"
                            className="govuk-!-padding-left-3"
                            onClick={e => {
                              setDatasetValue('All featured tables');
                              setDataset('subject-all');
                              e.preventDefault();
                            }}
                          >
                            View all featured tables
                          </a>

                          <fieldset className="govuk-fieldset">
                            <legend
                              className={classNames(
                                'govuk-fieldset__legend',
                                'govuk-fieldset__legend--s',
                                'govuk-!-margin-bottom-6',
                                'govuk-visually-hidden',
                              )}
                            >
                              Select a dataset
                            </legend>

                            <div
                              className={classNames(
                                'govuk-radios',
                                'govuk-radios--small',
                                'govuk-form-group',
                                'govuk-!-padding-left-3',
                                'govuk-!-padding-right-5',
                              )}
                            >
                              {/* <fieldset className="govuk-fieldset govuk-!-margin-top-3">
                                <div className="govuk-radios__item">
                                  <input
                                    type="radio"
                                    className="govuk-radios__input"
                                    name="subject"
                                    id="subject-all"
                                    onClick={() => {
                                      setDatasetValue('All featured tables');
                                      setDataset('subject-all');
                                    }}
                                  />
                                  <label
                                    className={classNames(
                                      'govuk-label',
                                      'govuk-radios__label',
                                    )}
                                    htmlFor="subject-all"
                                  >
                                    All featured tables
                                  </label>
                                </div>
                                    </fieldset> */}

                              <fieldset className="govuk-fieldset govuk-!-margin-top-6">
                                <legend className="govuk-heading-s govuk-!-margin-bottom-0 ">
                                  <h3>Datasets</h3>
                                  Headlines
                                </legend>
                                <div className="govuk-radios__item">
                                  <input
                                    type="radio"
                                    className="govuk-radios__input"
                                    name="subject"
                                    id="subject-1"
                                    onClick={() => {
                                      setDatasetValue(
                                        'Annual Headlines - detailed series',
                                      );
                                      setDataset('subject-1');
                                    }}
                                  />
                                  <label
                                    className={classNames(
                                      'govuk-label',
                                      'govuk-radios__label',
                                    )}
                                    htmlFor="subject-1"
                                  >
                                    Annual Headlines - detailed series
                                  </label>
                                </div>
                              </fieldset>

                              <fieldset className="govuk-fieldset govuk-!-margin-top-6">
                                <legend className="govuk-heading-s govuk-!-margin-bottom-0 ">
                                  Apprenticeship Achievement Rates
                                </legend>
                                <div className="govuk-radios__item">
                                  <input
                                    type="radio"
                                    className="govuk-radios__input"
                                    name="subject"
                                    id="subject-2"
                                    onClick={() => {
                                      setDatasetValue(
                                        'Apprenticeship Achievement Rates Demographics',
                                      );
                                      setDataset('subject-2');
                                    }}
                                  />
                                  <label
                                    className={classNames(
                                      'govuk-label',
                                      'govuk-radios__label',
                                    )}
                                    htmlFor="subject-2"
                                  >
                                    Rates Demographics
                                  </label>
                                </div>
                                <div className="govuk-radios__item">
                                  <input
                                    type="radio"
                                    className="govuk-radios__input"
                                    name="subject"
                                    id="subject-3"
                                    checked={dataset === 'subject-3'}
                                    onClick={() => {
                                      setDatasetValue(
                                        'Apprenticeship Achievement Rates  Detailed series',
                                      );
                                      setDataset('subject-3');
                                    }}
                                  />
                                  <label
                                    className={classNames(
                                      'govuk-label',
                                      'govuk-radios__label',
                                    )}
                                    htmlFor="subject-3"
                                  >
                                    {' '}
                                    Rates Detailed Series
                                  </label>
                                </div>
                                <div className="govuk-radios__item">
                                  <input
                                    type="radio"
                                    className="govuk-radios__input"
                                    name="subject"
                                    id="subject-4"
                                    onClick={() => {
                                      setDatasetValue(
                                        'Apprenticeship Achievement Rates Headliness',
                                      );
                                      setDataset('subject-4');
                                    }}
                                  />
                                  <label
                                    className={classNames(
                                      'govuk-label',
                                      'govuk-radios__label',
                                    )}
                                    htmlFor="subject-4"
                                  >
                                    Rates Headlines
                                  </label>
                                </div>
                              </fieldset>
                              <fieldset className="govuk-fieldset govuk-!-margin-top-6">
                                <legend className="govuk-heading-s govuk-!-margin-bottom-0 ">
                                  Other heading example
                                </legend>

                                <div className="govuk-radios__item">
                                  <input
                                    type="radio"
                                    className="govuk-radios__input"
                                    name="subject"
                                    id="subject-5"
                                  />
                                  <label
                                    className={classNames(
                                      'govuk-label',
                                      'govuk-radios__label',
                                    )}
                                    htmlFor="subject-5"
                                  >
                                    Apprenticeship Service - incentives
                                  </label>
                                </div>
                                <div className="govuk-radios__item">
                                  <input
                                    type="radio"
                                    className="govuk-radios__input"
                                    name="subject"
                                    id="subject-6"
                                  />
                                  <label
                                    className={classNames(
                                      'govuk-label',
                                      'govuk-radios__label',
                                    )}
                                    htmlFor="subject-6"
                                  >
                                    Charts data
                                  </label>
                                </div>
                                <div className="govuk-radios__item">
                                  <input
                                    type="radio"
                                    className="govuk-radios__input"
                                    name="subject"
                                    id="subject-7"
                                  />
                                  <label
                                    className={classNames(
                                      'govuk-label',
                                      'govuk-radios__label',
                                    )}
                                    htmlFor="subject-7"
                                  >
                                    Duration, planned length of stay and length
                                    of employment
                                  </label>
                                </div>
                                <div className="govuk-radios__item">
                                  <input
                                    type="radio"
                                    className="govuk-radios__input"
                                    name="subject"
                                    id="subject-8"
                                  />
                                  <label
                                    className={classNames(
                                      'govuk-label',
                                      'govuk-radios__label',
                                    )}
                                    htmlFor="subject-8"
                                  >
                                    Find an apprenticeship adverts and vacancies
                                  </label>
                                </div>
                                <div className="govuk-radios__item">
                                  <input
                                    type="radio"
                                    className="govuk-radios__input"
                                    name="subject"
                                    id="subject-9"
                                  />
                                  <label
                                    className={classNames(
                                      'govuk-label',
                                      'govuk-radios__label',
                                    )}
                                    htmlFor="subject-9"
                                  >
                                    Geographical breakdowns - detailed (reported
                                    to date)
                                  </label>
                                </div>
                              </fieldset>
                            </div>
                          </fieldset>
                          <Details
                            summary="Filter datasets"
                            className="govuk-!-margin-top-6 govuk-!-margin-left-5"
                          >
                            <div
                              className="govuk-form-group govuk-!-margin-bottom-6"
                              style={{ position: 'relative' }}
                            >
                              <h2 className="govuk-label-wrapper">
                                <label
                                  className="govuk-label govuk-label--s"
                                  htmlFor="search"
                                >
                                  Search
                                </label>
                              </h2>

                              <input
                                type="search"
                                id="search"
                                className="govuk-input"
                                value=""
                                style={{ width: 'calc(100% - 36px)' }}
                              />
                            </div>
                            <div
                              className="govuk-form-group govuk-!-margin-bottom-6"
                              style={{ position: 'relative' }}
                            >
                              <h2 className="govuk-label-wrapper">
                                <label
                                  className="govuk-label govuk-label--s"
                                  htmlFor="search"
                                >
                                  Geographic level including
                                </label>
                              </h2>

                              <select className="govuk-select">
                                <option value="">Any geographic level</option>
                                <option value="">National</option>
                                <option value="">Regional</option>
                                <option value="">Local authority</option>
                              </select>
                            </div>
                            <div
                              className="govuk-form-group govuk-!-margin-bottom-6"
                              style={{ position: 'relative' }}
                            >
                              <h2 className="govuk-label-wrapper">
                                <label
                                  className="govuk-label govuk-label--s"
                                  htmlFor="search"
                                >
                                  Indicators including
                                </label>
                              </h2>

                              <select className="govuk-select">
                                <option value="">Any indicator</option>
                                <option value="">Achievements</option>
                                <option value="">Learner participation</option>
                                <option value="">
                                  Percentage Achievements
                                </option>
                                <option value="">
                                  Percentage Learner participations
                                </option>
                                <option value="">Starts</option>
                              </select>
                              <button
                                type="submit"
                                className="govuk-button govuk-!-margin-top-6"
                              >
                                Apply filters
                              </button>
                            </div>
                          </Details>
                        </div>
                      </div>
                      <div className="govuk-grid-column-three-quarters">
                        <h3>
                          {datasetValue &&
                            (datasetValue === 'All featured tables'
                              ? 'All featured tables for this publication'
                              : 'Dataset details')}
                        </h3>
                        {!dataset && (
                          <div className="govuk-!-margin-0">
                            <p className="govuk-body-l govuk-!-width-three-quarters">
                              Please select a dataset, you will then be able to
                              see a summary of the data, create your own tables,
                              view featured tables, or download the entire data
                              file.
                            </p>
                            <p className="govuk-body-l govuk-!-width-three-quarters">
                              Alternatively you can browse{' '}
                              <a
                                href="#"
                                onClick={e => {
                                  setDatasetValue('All featured tables');
                                  setDataset('subject-all');
                                  e.preventDefault();
                                }}
                              >
                                featured tables
                              </a>{' '}
                              from across all the datasets for this publication
                              .
                            </p>
                            <p className="govuk-!-width-three-quarters">
                              Featured tables have been created by our
                              publication team, highlighting popular tables
                              built from datasets available in this publication.
                              These tables can be viewed, shared and customised
                              to the data that you're interested in.
                            </p>
                          </div>
                        )}
                        {dataset === 'subject-1' && (
                          <div className="govuk-!-margin-0">
                            <SummaryList noBorder className="govuk-!-margin-0">
                              <SummaryListItem term="Selected dataset">
                                <strong>{datasetValue}</strong>
                              </SummaryListItem>
                              <SummaryListItem term="Geographic levels">
                                National
                              </SummaryListItem>
                              <SummaryListItem term="Time period">
                                2014/15 to 2020/21
                              </SummaryListItem>
                              <SummaryListItem term="Content">
                                <p>
                                  Time series of headline apprenticeship figures
                                </p>

                                <h4 className="govuk-!-margin-bottom-0">
                                  Indicators
                                </h4>
                                <p>
                                  Starts, Achievements, Learner participation,
                                  Percentage Starts, Percentage Achievements,
                                  Percentage Learner participation
                                </p>
                                <h4 className="govuk-!-margin-bottom-0">
                                  Filters
                                </h4>
                                <p>
                                  Apprenticeship level, Funding type, Age group
                                </p>
                              </SummaryListItem>
                            </SummaryList>

                            <PrototypeStep2Options />

                            <PrototypeFeaturedLinks
                              dataset={dataset}
                              publication={publication}
                            />
                          </div>
                        )}
                        {dataset === 'subject-2' && (
                          <div className="govuk-!-margin-0">
                            <SummaryList noBorder className="govuk-!-margin-0">
                              <SummaryListItem term="Selected dataset">
                                <strong>{datasetValue}</strong>
                              </SummaryListItem>
                              <SummaryListItem term="Geographic levels">
                                National
                              </SummaryListItem>
                              <SummaryListItem term="Time period">
                                2018/19 to 2020/21
                              </SummaryListItem>
                              <SummaryListItem term="Content">
                                <p>
                                  Apprenticeship national achievement rate
                                  tables
                                </p>
                                <h4 className="govuk-!-margin-bottom-0">
                                  Indicators
                                </h4>
                                <p>
                                  Achievement rate, Achievers, Completers,
                                  Leavers, Pass rate, Retention rate
                                </p>
                                <h4 className="govuk-!-margin-bottom-0">
                                  Filters
                                </h4>
                                <p>
                                  Age, Level, demographic - ethnicity, gender
                                  and lldd, Standard /Framework flag
                                </p>
                              </SummaryListItem>
                            </SummaryList>

                            <PrototypeStep2Options />

                            <PrototypeFeaturedLinks
                              dataset={dataset}
                              publication={publication}
                            />
                          </div>
                        )}
                        {dataset === 'subject-3' && (
                          <div className="govuk-!-margin-0">
                            <SummaryList noBorder className="govuk-!-margin-0">
                              <SummaryListItem term="Selected dataset">
                                <strong>{datasetValue}</strong>
                              </SummaryListItem>
                              <SummaryListItem term="Geographic levels">
                                National
                              </SummaryListItem>
                              <SummaryListItem term="Time period">
                                2018/19 to 2020/21
                              </SummaryListItem>
                              <SummaryListItem term="Content">
                                <p>
                                  Apprenticeship national achievement rate
                                  tables{' '}
                                </p>
                              </SummaryListItem>
                              <SummaryListItem term="Indicators">
                                Achievement rate, Achievers, Completers,
                                Leavers, Pass rate, Retention rate
                              </SummaryListItem>
                              <SummaryListItem term="filters">
                                Age, Level, demographic - ethnicity, gender and
                                lldd, Sector Subject Area, Standard /Framework
                                flag
                              </SummaryListItem>
                            </SummaryList>

                            <PrototypeStep2Options />

                            <PrototypeFeaturedLinks
                              dataset={dataset}
                              publication={publication}
                            />
                          </div>
                        )}
                        {dataset === 'subject-4' && (
                          <div className="govuk-!-margin-0">
                            <SummaryList noBorder className="govuk-!-margin-0">
                              <SummaryListItem term="Selected dataset">
                                <strong>{datasetValue}</strong>
                              </SummaryListItem>
                              <SummaryListItem term="Geographic levels">
                                National
                              </SummaryListItem>
                              <SummaryListItem term="Time period">
                                2018/19 to 2020/21
                              </SummaryListItem>
                              <SummaryListItem term="Content">
                                <p>
                                  Apprenticeship national achievement rate
                                  tables
                                  <Details
                                    summary="View more details"
                                    className="govuk-!-margin-bottom-0"
                                  >
                                    <p>
                                      Indicators: Achievement rate, Leavers,
                                      Pass rate, Retention rate
                                    </p>
                                    <p>
                                      Filters: Level, Detailed Level, Sector
                                      Subject Area, Standard /Framework flag
                                    </p>
                                  </Details>
                                </p>
                              </SummaryListItem>
                            </SummaryList>

                            <PrototypeStep2Options noFeatured />
                          </div>
                        )}
                        {dataset === 'subject-all' && (
                          <div className="govuk-!-margin-0">
                            <div className="govuk-width-container govuk-!-margin-0">
                              <p className="govuk-body-l">
                                View featured tables from across all our
                                datasets for this publication. If you can't find
                                what you are looking for please select a
                                specific dataset, and then you can create your
                                own table.
                              </p>
                              <PageSearchForm inputLabel="Search featured tables" />
                            </div>

                            <PrototypeFeaturedLinks
                              dataset={dataset}
                              publication={publication}
                            />
                          </div>
                        )}
                      </div>
                    </div>
                  </form>
                </div>
              </div>
            </li>
          </ol>
        </div>
      </PrototypePage>
    </div>
  );
};

export default PrototypeTableHighlights;
