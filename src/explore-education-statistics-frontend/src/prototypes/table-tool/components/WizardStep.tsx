import { InjectedWizardProps } from '@frontend/prototypes/table-tool/components/Wizard';
import classNames from 'classnames';
import React, { forwardRef, ReactNode } from 'react';
import styles from './WizardStep.module.scss';

export interface WizardStepProps {
  children: ((props: InjectedWizardProps) => ReactNode) | ReactNode;
  id?: string;
}

const WizardStep = forwardRef<HTMLLIElement, WizardStepProps>(
  ({ children, id, ...restProps }: WizardStepProps, ref) => {
    // Hide injected props from public API
    const injectedWizardProps = restProps as InjectedWizardProps;

    const { stepNumber, currentStep, isActive } = injectedWizardProps;

    return (
      <li
        className={classNames(styles.step, {
          [styles.stepActive]: isActive,
          [styles.stepEnabled]: currentStep > stepNumber,
        })}
        id={id}
        ref={ref}
        tabIndex={-1}
      >
        <div className={styles.content}>
          <span className={styles.number} aria-hidden>
            <span className={styles.numberInner}>{stepNumber}</span>
          </span>

          {typeof children === 'function'
            ? children(injectedWizardProps)
            : children}
        </div>
      </li>
    );
  },
);

WizardStep.displayName = 'WizardStep';

export default WizardStep;
