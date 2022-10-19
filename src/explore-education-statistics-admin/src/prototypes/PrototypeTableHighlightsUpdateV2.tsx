import classNames from 'classnames';
import ButtonText from '@common/components/ButtonText';
import PageTitle from '@admin/components/PageTitle';
import PrototypePage from '@admin/prototypes/components/PrototypePage';
import React from 'react';
import stylesWiz from '@common/modules/table-tool/components/Wizard.module.scss';
import stylesStep from '@common/modules/table-tool/components/WizardStep.module.scss';
import stylesWizSummary from '@common/modules/table-tool/components/WizardStepSummary.module.scss';
import stylesStepHeading from '@common/modules/table-tool/components/WizardStepHeading.module.scss';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import stylesPublicView from './PrototypePublicPage.module.scss';

const styles = {
  ...stylesWizSummary,
  ...stylesPublicView,
  ...stylesWiz,
  ...stylesStep,
  ...stylesStepHeading,
};

const PrototypeTableHighlights = () => {
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
              <div className={classNames(styles.content, styles.contentSmall)}>
                <div className={styles.container}>
                  <div className={styles.content}>
                    <h2 className={styles.stepEnabled}>
                      <span className="govuk-tag govuk-tag govuk-tag--grey">
                        Step 1
                      </span>
                      <span className="govuk-visually-hidden">
                        Choose a publication
                      </span>
                    </h2>
                    <SummaryList noBorder>
                      <SummaryListItem term="Publication">
                        Apprenticeships and traineeships
                      </SummaryListItem>
                    </SummaryList>
                  </div>
                </div>
              </div>
              <div className={styles.goToContainer}>
                <ButtonText className={styles.goToButton}>
                  Change publication
                </ButtonText>
              </div>
            </li>
            <li
              aria-current="step"
              className={classNames(styles.step, styles.stepActive)}
              data-testid="wizardStep-2"
              id="2"
              tabIndex={-1}
            >
              <div className={classNames(styles.content, styles.contentSmall)}>
                <div className={styles.container} style={{ flexWrap: 'wrap' }}>
                  <div className={styles.content}>
                    <h2 className="govuk-heading-l dfe-flex dfe-align-items--center govuk-fieldset__heading">
                      <span className="govuk-tag govuk-tag govuk-tag--turquoise govuk-!-margin-right-2">
                        Step 2
                      </span>
                      <span className="govuk-heading-m govuk-!-margin-0">
                        Choose a dataset
                      </span>
                    </h2>
                  </div>
                  <form
                    className="govuk-!-margin-top-3"
                    style={{ flex: '0 0 100%' }}
                  >
                    <div className="govuk-grid-row">
                      <div className="govuk-grid-column-one-quarter">
                        <div className="govuk-form-group">
                          <fieldset className="govuk-fieldset">
                            <legend
                              className={classNames(
                                'govuk-fieldset__legend',
                                'govuk-fieldset__legend--m',
                                'govuk-!-margin-bottom-6',
                                'govuk-visually-hidden',
                              )}
                            >
                              Choose a dataset
                            </legend>
                            <div className="govuk-radios">
                              <div className="govuk-radios__item">
                                <input
                                  type="radio"
                                  className="govuk-radios__input"
                                  name="subject"
                                  id="subject-1"
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
                            </div>
                          </fieldset>
                        </div>
                      </div>
                      <div className="govuk-grid-column-three-quarters">
                        <h3>
                          Dataset: Apprenticeship Achievement Rates Detailed
                          Series
                        </h3>
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
