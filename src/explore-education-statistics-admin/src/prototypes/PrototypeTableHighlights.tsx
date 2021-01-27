import classNames from 'classnames';
import PageTitle from '@admin/components/PageTitle';
import PrototypePage from '@admin/prototypes/components/PrototypePage';
import React from 'react';
import styles from '@common/modules/table-tool/components/Wizard.module.scss';
import stylesStep from '@common/modules/table-tool/components/WizardStep.module.scss';
import stylesStepHeading from '@common/modules/table-tool/components/WizardStepHeading.module.scss';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import HighlightsLinks from './components/PrototypeHighlightsLinks';
import PreviewSubjects from './components/PrototypeTablePreviewSubjects';

const PrototypeTableHighlights = () => {
  return (
    <PrototypePage wide>
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <PageTitle
            title="Create your own tables online"
            caption="Table tool"
          />
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
      <div>
        <ol className={styles.stepNav} id="tableToolWizard">
          <li
            aria-current="step"
            className={classNames(stylesStep.step)}
            data-testid="wizardStep-1"
            id="1"
            tabIndex={-1}
          >
            <div className={stylesStep.content}>
              <span className={stylesStep.number} aria-hidden>
                <span className={stylesStep.numberInner}>{1}</span>
              </span>
              <h2
                className={classNames(
                  'govuk-heading-m',
                  stylesStepHeading.stepEnabled,
                )}
              >
                <button
                  data-testid="wizardStep-1-goToButton"
                  type="button"
                  className={stylesStepHeading.stepButton}
                >
                  Children looked after in England including adoptions
                  <span className={stylesStepHeading.toggleText} aria-hidden>
                    Change this publication
                  </span>
                </button>
              </h2>
            </div>
          </li>
          <li
            aria-current="step"
            className={classNames(stylesStep.step, stylesStep.stepActive)}
            data-testid="wizardStep-2"
            id="2"
            tabIndex={-1}
          >
            <div className={stylesStep.content}>
              <span className={stylesStep.number} aria-hidden>
                <span className={stylesStep.numberInner}>{2}</span>
              </span>
              <h2 className={classNames('govuk-heading-m')}>
                View a popular table or create your own
              </h2>
              <Tabs id="test">
                <TabsSection title="Popular tables (30)">
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
      <p>Test</p>
    </PrototypePage>
  );
};

export default PrototypeTableHighlights;
