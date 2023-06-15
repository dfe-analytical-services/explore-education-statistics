import classNames from 'classnames';
import PageTitle from '@admin/components/PageTitle';
import PrototypePage from '@admin/prototypes/components/PrototypePage';
import stylesWiz from '@common/modules/table-tool/components/Wizard.module.scss';
import stylesStep from '@common/modules/table-tool/components/WizardStep.module.scss';
import stylesStepHeading from '@common/modules/table-tool/components/WizardStepHeading.module.scss';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import React from 'react';
import stylesPublicView from './PrototypePublicPage.module.scss';
import HighlightsLinks from './components/PrototypeHighlightsLinks';
import PreviewSubjects from './components/PrototypeTablePreviewSubjects';

const styles = {
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
            <PageTitle title="Create your own tables" caption="Table tool" />
            <p>
              Choose the data and area of interest you want to explore and then
              use filters to create your table.
            </p>
            <p>
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
              <div className={styles.content}>
                <span className={styles.number} aria-hidden>
                  <span className={styles.numberInner}>{1}</span>
                </span>
                <h2
                  className={classNames('govuk-heading-m', styles.stepEnabled)}
                >
                  <button
                    data-testid="wizardStep-1-goToButton"
                    type="button"
                    className={styles.stepButton}
                  >
                    Children looked after in England including adoptions
                    <span className={styles.toggleText} aria-hidden>
                      Change this publication
                    </span>
                  </button>
                </h2>
              </div>
            </li>
            <li
              aria-current="step"
              className={classNames(styles.step, styles.stepActive)}
              data-testid="wizardStep-2"
              id="2"
              tabIndex={-1}
            >
              <div className={styles.content}>
                <span className={styles.number} aria-hidden>
                  <span className={styles.numberInner}>{2}</span>
                </span>
                <h2 className={classNames('govuk-heading-m')}>
                  View a featured table or create your own
                </h2>
                <Tabs id="test">
                  <TabsSection title="Featured tables (30)">
                    <HighlightsLinks />
                  </TabsSection>
                  <TabsSection title="Create your own table">
                    <PreviewSubjects />
                  </TabsSection>
                </Tabs>
              </div>
            </li>
          </ol>
        </div>
      </PrototypePage>
    </div>
  );
};

export default PrototypeTableHighlights;
