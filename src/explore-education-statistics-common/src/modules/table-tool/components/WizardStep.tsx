import classNames from 'classnames';
import React, { ReactNode, useEffect, useRef } from 'react';
import { InjectedWizardProps } from './Wizard';
import styles from './WizardStep.module.scss';

export interface WizardStepProps {
  children: ((props: InjectedWizardProps) => ReactNode) | ReactNode;
  onBack?: () => void;
  id?: string;
  size?: 's' | 'l';
}

const WizardStep = ({
  children,
  id,
  size = 's',
  ...restProps
}: WizardStepProps) => {
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
    // Don't re-focus on initial page render as this is confusing
    // for screen reader users (initial focus should be on the body)
    if (isActive && shouldScroll && document.activeElement !== document.body) {
      setTimeout(() => {
        if (ref.current) {
          ref.current.scrollIntoView({
            block: 'start',
            behavior: 'smooth',
          });
          ref.current.focus();
        }
      }, 300);
    }
  }, [currentStep, isActive, shouldScroll]);

  return (
    <li
      aria-current={isActive ? 'step' : undefined}
      className={classNames(styles.step, {
        [styles.stepActive]: isActive,
        [styles.stepHidden]: stepNumber > currentStep,
      })}
      data-testid={`wizardStep-${stepNumber}`}
      id={id}
      ref={ref}
      tabIndex={-1}
      hidden={stepNumber > currentStep}
    >
      <div
        className={classNames(styles.content, {
          [styles.contentSmall]: size === 's',
        })}
      >
        {typeof children === 'function'
          ? children(injectedWizardProps)
          : children}
      </div>
    </li>
  );
};

export default WizardStep;
