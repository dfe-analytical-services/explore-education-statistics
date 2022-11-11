import classNames from 'classnames';
import PageSearchForm from '@common/components/PageSearchForm';
import PageTitle from '@admin/components/PageTitle';
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
  const [dataset, setDataset] = useState('');
  const [datasetValue, setDatasetValue] = useState('');
  const publication = 'pub-3';

  return (
    <div
      className={classNames(
        styles.prototypePublicPage,
        styles.prototypeTableTool,
      )}
    >
      <PrototypePage wide>
        <div className="govuk-grid-row">
          <div className="govuk-grid-column-two-thirds">
            <PageTitle title="Explore our datasets" caption="Table tool" />
            <p>
              Choose the data and area of interest you want to explore and then
              use filters to create your table.
              <br />
              Once you've created your table, you can download the data it
              contains for your own offline analysis.
            </p>
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
                      <div>
                        <a href="https://explore-education-statistics.service.gov.uk/find-statistics/key-stage-4-performance-revised">
                          Key stage 4 performance
                        </a>
                      </div>
                      <div>
                        <a
                          href="https://explore-education-statistics.service.gov.uk/find-statistics/key-stage-4-performance-revised"
                          className="govuk-!-margin-right-6"
                        >
                          View publication
                        </a>

                        <a
                          href="https://explore-education-statistics.service.gov.uk/data-tables"
                          className="govuk-!-margin-right-6"
                        >
                          Change
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
                            Select a dataset
                          </div>
                        </h2>
                      </div>
                      <div className="dfe-flex dfe-justify-content--space-between  govuk-grid-column-three-quarters">
                        <h3>
                          {datasetValue
                            ? 'Dataset details'
                            : 'No dataset selected'}
                        </h3>
                      </div>
                    </div>
                  </div>
                  <form>
                    <div className="govuk-grid-row govuk-!-width-full">
                      <div className="govuk-grid-column-one-quarter">
                        <div className="govuk-form-group govuk-!-padding-left-3 govuk-!-padding-right-5">
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
                            <div className="govuk-radios govuk-radios--small">
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

                              <div className="govuk-radios__item">
                                <input
                                  type="radio"
                                  className="govuk-radios__input"
                                  name="subject"
                                  id="subject-1"
                                  onClick={() => {
                                    setDatasetValue('KS4 national data');
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
                                  KS4 national data
                                </label>
                              </div>
                              <div className="govuk-radios__item">
                                <input
                                  type="radio"
                                  className="govuk-radios__input"
                                  name="subject"
                                  id="subject-2"
                                  onClick={() => {
                                    setDatasetValue(
                                      'KS4 national characteristics data',
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
                                  KS4 national characteristics data
                                </label>
                              </div>

                              <div className="govuk-radios__item">
                                <input
                                  type="radio"
                                  className="govuk-radios__input"
                                  name="subject"
                                  id="subject-3"
                                  onClick={() => {
                                    setDatasetValue('KS4 local authority data');
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
                                  KS4 local authority data
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
                                      'KS4 local authority characteristics data',
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
                                  KS4 local authority characteristics data
                                </label>
                              </div>
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
                                  KS4 subject pupil level data
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
                                  KS4 subject entry level data
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
                                  KS4 subject year of entry data
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
                                  KS4 national academies data
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
                                  KS4 disadvantage gap index data
                                </label>
                              </div>
                            </div>
                          </fieldset>
                        </div>
                      </div>
                      <div className="govuk-grid-column-three-quarters">
                        {!dataset && (
                          <div className="govuk-!-margin-0">
                            <p className="govuk-body-l govuk-!-width-three-quarters">
                              Please select a dataset, you will then be able to
                              see a summary of the data, create your own tables,
                              view featured tables, or download the entire data
                              file.
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
                                2009/10 to 2020/21
                              </SummaryListItem>
                              <SummaryListItem term="Content">
                                <p>
                                  National level headline entry and attainment
                                  measures broken down by year, detailed school
                                  type, gender, prior attainment, school
                                  admission type and school religious
                                  denomination.
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
                                  National level headline entry and attainment
                                  measures broken down by year, detailed school
                                  type, gender, major and minor ethnicity, free
                                  school meal eligibility, special educational
                                  need status and description, primary need,
                                  disadvantaged status, first language, school
                                  admission type and school religious
                                  denomination.
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
                                Local Authority; National; Regional
                              </SummaryListItem>
                              <SummaryListItem term="Time period">
                                2018/19 to 2021/22
                              </SummaryListItem>
                              <SummaryListItem term="Content">
                                <p>
                                  Local authority and regional level headline
                                  entry and attainment measures for the latest
                                  year broken down gender. State-funded schools
                                  only.
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
                        {dataset === 'subject-4' && (
                          <div className="govuk-!-margin-0">
                            <SummaryList noBorder className="govuk-!-margin-0">
                              <SummaryListItem term="Selected dataset">
                                <strong>{datasetValue}</strong>
                              </SummaryListItem>
                              <SummaryListItem term="Geographic levels">
                                Local Authority; National; Regional
                              </SummaryListItem>
                              <SummaryListItem term="Time period">
                                2018/19 to 2021/22
                              </SummaryListItem>
                              <SummaryListItem term="Content">
                                <p>
                                  Local authority and regional level headline
                                  entry and attainment measures for the latest
                                  year broken down by gender, major ethnicity,
                                  free school meal eligibility, special
                                  educational need status and description,
                                  disadvantaged status and first language.
                                  State-funded schools only.
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
