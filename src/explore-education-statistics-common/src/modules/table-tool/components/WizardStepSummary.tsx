import ButtonText from '@common/components/ButtonText';
import LoadingSpinner from '@common/components/LoadingSpinner';
import { InjectedWizardProps } from '@common/modules/table-tool/components/Wizard';
import React, { ReactElement, ReactNode } from 'react';
import styles from './WizardStepSummary.module.scss';

interface Props extends InjectedWizardProps {
  children: ReactNode;
  goToButtonText: string;
}

const WizardStepSummary = ({
  children,
  goToButtonText,
  isEnabled,
  isLoading,
  loadingStep,
  setCurrentStep,
  stepNumber,
}: Props): ReactElement => {
  return (
    <div className={styles.container}>
      <div className={styles.content}>{children}</div>

      {isEnabled && (
        <div className={styles.goToContainer}>
          <ButtonText
            className={styles.goToButton}
            testId={`wizardStep-${stepNumber}-goToButton`}
            disabled={isLoading || typeof loadingStep !== 'undefined'}
            onClick={() => setCurrentStep(stepNumber)}
          >
            {goToButtonText}{' '}
            <span className="govuk-visually-hidden">{`on step ${stepNumber}`}</span>
          </ButtonText>

          <LoadingSpinner
            alert
            inline
            hideText
            loading={isLoading}
            size="sm"
            text={`Loading step ${stepNumber}`}
            className="govuk-!-margin-left-2"
          />
        </div>
      )}
    </div>
  );
};

export default WizardStepSummary;
