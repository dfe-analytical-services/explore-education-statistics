import classNames from 'classnames';
import React, { ReactNode, useState } from 'react';
import styles from './StepByStep.module.scss';

interface Props {
  children: ReactNode;
  open?: boolean;
  stepHeading?: string;
  stepNumber?: number;
}

const StepByStepItem = ({ children, stepHeading, stepNumber, open }: Props) => {
  const [viewStep, setViewStep] = useState(open);

  return (
    <>
      <li
        className={classNames(styles.step, { [styles.stepActive]: viewStep })}
        id={`step${stepNumber}`}
      >
        <div className={styles.content}>
          <span className={styles.number} aria-hidden>
            <span className={styles.numberInner}>{stepNumber}</span>
          </span>
          {!viewStep && (
            <>
              <a
                href="#"
                className={styles.toggleLink}
                onClick={event => {
                  event.preventDefault();
                  setViewStep(true);
                }}
              >
                <h2 className="govuk-heading-m govuk-!-margin-bottom-0">
                  {stepHeading}
                </h2>
                <span className="govuk-!-font-size-16">Show</span>
              </a>
            </>
          )}
          {viewStep && (
            <>
              <a
                href="#"
                className={styles.toggleLink}
                onClick={event => {
                  event.preventDefault();
                  setViewStep(false);
                }}
              >
                <h2 className="govuk-heading-m govuk-!-margin-bottom-0">
                  {stepHeading}
                </h2>
                <span className="govuk-!-font-size-16">Hide</span>
              </a>
              <div className="govuk-!-margin-top-3">{children}</div>
            </>
          )}
        </div>
      </li>
    </>
  );
};

export default StepByStepItem;
