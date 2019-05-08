import { InjectedWizardProps } from '@frontend/modules/table-tool/components/Wizard';
import classNames from 'classnames';
import React, { ReactNode, useEffect, useRef } from 'react';
import styles from './WizardStep.module.scss';

export interface WizardStepProps {
  children: ((props: InjectedWizardProps) => ReactNode) | ReactNode;
  id?: string;
}

const WizardStep = ({ children, id, ...restProps }: WizardStepProps) => {
  // Hide injected props from public API
  const injectedWizardProps = restProps as InjectedWizardProps;

  const { stepNumber, currentStep, isActive } = injectedWizardProps;

  const ref = useRef<HTMLLIElement>(null);

  useEffect(() => {
    if (isActive && ref.current) {
      ref.current.scrollIntoView({
        block: 'start',
        behavior: 'smooth',
      });
      ref.current.focus();
    }
  }, [isActive]);

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
          ? children({
              ...injectedWizardProps,
            })
          : children}
      </div>
    </li>
  );
};

export default WizardStep;
