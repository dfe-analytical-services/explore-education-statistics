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
  const publication = 'pub-2';

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
                        <a href="https://explore-education-statistics.service.gov.uk/find-statistics/school-workforce-in-england">
                          School workforce in England
                        </a>
                      </div>
                      <div>
                        <a
                          href="https://explore-education-statistics.service.gov.uk/find-statistics/school-workforce-in-england"
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
                                  id="subject-1"
                                  onClick={() => {
                                    setDatasetValue('Entrants');
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
                                  Entrants
                                </label>
                              </div>
                              <div className="govuk-radios__item">
                                <input
                                  type="radio"
                                  className="govuk-radios__input"
                                  name="subject"
                                  id="subject-2"
                                  onClick={() => {
                                    setDatasetValue('Leavers');
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
                                  Leavers
                                </label>
                              </div>
                              <div className="govuk-radios__item">
                                <input
                                  type="radio"
                                  className="govuk-radios__input"
                                  name="subject"
                                  id="subject-3"
                                  onClick={() => {
                                    setDatasetValue(
                                      'Specialist teachers in state funded secondary schools',
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
                                  Specialist teachers in state funded secondary
                                  schools
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
                                      'Subjects taught in state funded secondary schools',
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
                                  Subjects taught in state funded secondary
                                  schools
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
                                  Support staff characteristics
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
                                  Teacher absences
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
                                  Teacher pay
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
                                  Teacher qualifications
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
                                  Teacher retention
                                </label>
                              </div>
                              <div className="govuk-radios__item">
                                <input
                                  type="radio"
                                  className="govuk-radios__input"
                                  name="subject"
                                  id="subject-10"
                                />
                                <label
                                  className={classNames(
                                    'govuk-label',
                                    'govuk-radios__label',
                                  )}
                                  htmlFor="subject-10"
                                >
                                  Teacher retirements
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
                                2011/12 to 2020/21
                              </SummaryListItem>
                              <SummaryListItem term="Content">
                                <p>
                                  Full time equivalent (FTE) qualified teacher
                                  entrants by category including school type,
                                  gender, age group and type of entrant.
                                </p>
                              </SummaryListItem>
                            </SummaryList>

                            <PrototypeStep2Options noFeatured />

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
                                2010/11 to 2020/21
                              </SummaryListItem>
                              <SummaryListItem term="Content">
                                <p>
                                  Full time equivalent (FTE) qualified teacher
                                  leavers by category including school type,
                                  gender, age group and type of entrant.
                                </p>
                              </SummaryListItem>
                            </SummaryList>

                            <PrototypeStep2Options noFeatured />

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
                                2014/15 to 2020/21
                              </SummaryListItem>
                              <SummaryListItem term="Content">
                                <p>
                                  % of hours taught by a teacher with
                                  qualification in relevant subject and the % of
                                  teachers holding qualification in relevant
                                  subject by highest qualification of teacher
                                  and subject taught.
                                </p>
                              </SummaryListItem>
                            </SummaryList>

                            <PrototypeStep2Options noFeatured />

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
                                2011/12 to 2020/21
                              </SummaryListItem>
                              <SummaryListItem term="Content">
                                <p>
                                  Headcount of teachers and the number of hours
                                  taught by subject to year groups 7-13 in all
                                  state funded secondary schools.
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
