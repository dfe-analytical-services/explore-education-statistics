import classNames from 'classnames';
import React, { ReactNode, useEffect, useRef } from 'react';
import { InjectedWizardProps } from './Wizard';
import styles from './WizardStep.module.scss';

export interface WizardStepProps {
  children: ((props: InjectedWizardProps) => ReactNode) | ReactNode;
  id?: string;
}

const WizardStep = ({ children, id, ...restProps }: WizardStepProps) => {
  // Hide injected props from public API
  const injectedWizardProps = restProps as InjectedWizardProps;

  const {
    stepNumber,
    currentStep,
    shouldScroll,
    isActive,
  } = injectedWizardProps;

  const ref = useRef<HTMLLIElement>(null);

  useEffect(() => {
    if (isActive && shouldScroll && ref.current) {
      ref.current.scrollIntoView({
        block: 'start',
        behavior: 'smooth',
      });
      ref.current.focus();
    }
  }, [currentStep, isActive, shouldScroll]);

  return (
    <li
      className={classNames(styles.step, {
        [styles.stepActive]: isActive,
        [styles.stepHidden]: stepNumber > currentStep,
      })}
      id={id}
      ref={ref}
      tabIndex={-1}
      hidden={stepNumber > currentStep}
    >
      <div className={styles.content}>
        <span className={styles.number} aria-hidden>
          <span className={styles.numberInner}>{stepNumber}</span>
        </span>

        {typeof children === 'function'
          ? children({
              ...injectedWizardProps,
            })
          : children}
      </div>
    </li>
  );
};

export default WizardStep;
