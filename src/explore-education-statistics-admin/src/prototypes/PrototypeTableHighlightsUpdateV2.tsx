import classNames from 'classnames';
import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import PageSearchForm from '@common/components/PageSearchForm';
import PageTitle from '@admin/components/PageTitle';
import PrototypePage from '@admin/prototypes/components/PrototypePage';
import PrototypeGridView from '@admin/prototypes/components/PrototypeGridView';
import PrototypeChevronCard from '@admin/prototypes/components/PrototypeChevronCard';
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
                        <a href="#">Apprenticeships and traineeships</a>
                      </div>
                      <div>
                        <ButtonText
                          className={classNames(
                            styles.goToButton,
                            'govuk-!-margin-0',
                            'govuk-!-margin-right-6',
                          )}
                        >
                          View publication
                        </ButtonText>
                        <ButtonText
                          className={classNames(
                            styles.goToButton,
                            'govuk-!-margin-0',
                          )}
                        >
                          Change
                        </ButtonText>
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
                                  Apprenticeship Achievement Rates Demographics
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
                                      'Apprenticeship Achievement Rates  DetailedSeries',
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
                                  Apprenticeship Achievement Rates Detailed
                                  Series
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
                                  Apprenticeship Achievement Rates Headlines
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
                                  Duration, planned length of stay and length of
                                  employment
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
                                2014/15 to 2020/21
                              </SummaryListItem>
                              <SummaryListItem term="Content">
                                <p>
                                  Time series of headline apprenticeship figures
                                  <a href="#" className="govuk-!-margin-left-2">
                                    View more
                                  </a>
                                </p>
                              </SummaryListItem>
                            </SummaryList>

                            <PrototypeStep2Options />

                            <PrototypeGridView>
                              <PrototypeChevronCard
                                title="Starts by age and level"
                                description="Annual apprenticeship starts by age and level"
                              />
                            </PrototypeGridView>
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
                                  <a href="#" className="govuk-!-margin-left-2">
                                    View more
                                  </a>
                                </p>
                              </SummaryListItem>
                            </SummaryList>

                            <PrototypeStep2Options />

                            <PrototypeGridView>
                              <PrototypeChevronCard
                                title="Rates by sector subject area and Ethnicity group learners"
                                description="Apprenticeship achievement rates by sector subject area and Ethnicity group learners"
                              />
                              <PrototypeChevronCard
                                title="Rates by sector subject area and gender"
                                description="All age apprenticeships overall achievement rates demographic summary"
                              />
                              <PrototypeChevronCard
                                title="Rates by sector subject area and learners with learning difficulty and or disability"
                                description="Apprenticeship achievement rates by sector subject area and learners with learning difficulty and or disability"
                              />
                              <PrototypeChevronCard
                                title="Rates demographic summary"
                                description="All age apprenticeships overall achievement rates demographic summary"
                              />
                            </PrototypeGridView>
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
                                  <a href="#" className="govuk-!-margin-left-2">
                                    View more
                                  </a>
                                </p>
                              </SummaryListItem>
                            </SummaryList>

                            <PrototypeStep2Options />

                            <PrototypeGridView>
                              <PrototypeChevronCard
                                title="Achievements by level"
                                description="Apprenticeship achievements by level, 2015/16 to 2020/21"
                              />
                              <PrototypeChevronCard
                                title="Rates demographic summary"
                                description="All age apprenticeships overall achievement rates demographic summary"
                              />
                              <PrototypeChevronCard
                                title="Participation and achievements by age and level"
                                description="Apprenticeship participation and achievements by age and level, reported to date for 2021/22 with equivalent figures for 2018/19 to 2020/21"
                              />
                            </PrototypeGridView>
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
                                  <a href="#" className="govuk-!-margin-left-2">
                                    View more
                                  </a>
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

                            <PrototypeGridView>
                              <PrototypeChevronCard
                                title="Starts by age and level"
                                description="Annual apprenticeship starts by age and level"
                              />
                              <PrototypeChevronCard
                                title="Rates by sector subject area and Ethnicity group learners"
                                description="Apprenticeship achievement rates by sector subject area and Ethnicity group learners"
                              />
                              <PrototypeChevronCard
                                title="Rates by sector subject area and gender"
                                description="All age apprenticeships overall achievement rates demographic summary"
                              />
                              <PrototypeChevronCard
                                title="Rates by sector subject area and learners with learning difficulty and or disability"
                                description="Apprenticeship achievement rates by sector subject area and learners with learning difficulty and or disability"
                              />
                              <PrototypeChevronCard
                                title="Rates demographic summary"
                                description="All age apprenticeships overall achievement rates demographic summary"
                              />
                              <PrototypeChevronCard
                                title="Achievements by level"
                                description="Apprenticeship achievements by level, 2015/16 to 2020/21"
                              />
                              <PrototypeChevronCard
                                title="Rates demographic summary"
                                description="All age apprenticeships overall achievement rates demographic summary"
                              />
                              <PrototypeChevronCard
                                title="Participation and achievements by age and level"
                                description="Apprenticeship participation and achievements by age and level, reported to date for 2021/22 with equivalent figures for 2018/19 to 2020/21"
                              />
                            </PrototypeGridView>
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
