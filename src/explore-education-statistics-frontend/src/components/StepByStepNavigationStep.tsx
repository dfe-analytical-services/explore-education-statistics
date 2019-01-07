import React, { HTMLAttributes, ReactNode } from 'react';

interface Props {
  title: string;
  caption?: string;
  children?: ReactNode;
}

const StepByStepNavigationStep = ({ children, title, caption }: Props) => (
  <li className="app-step-nav__step js-step">
    <div className="app-step-nav__header js-toggle-panel" data-position="4">
      <h2 className="app-step-nav__title">
        <span className="app-step-nav__circle app-step-nav__circle--number">
          <span className="app-step-nav__circle-inner">
            <span className="app-step-nav__circle-background">
              <span className="govuk-visually-hidden">Step</span>
            </span>
          </span>
        </span>

        <span className="js-step-title">
          <button
            className="app-step-nav__button app-step-nav__button--title js-step-title-button"
            aria-expanded="true"
            aria-controls="step-panel"
          >
            <span className="js-step-title-text">
              <button
                className="app-step-nav__button app-step-nav__button--title js-step-title-button"
                aria-expanded="true"
                aria-controls="step-panel"
              >
                <span className="js-step-title-text">
                  {title}
                  <span className="govuk-caption-m">{caption}</span>
                </span>
                <span
                  className="app-step-nav__toggle-link js-toggle-link"
                  aria-hidden="false"
                >
                  Show
                </span>
              </button>
            </span>
          </button>
        </span>
      </h2>
    </div>
    <div className="app-step-nav__panel js-panel" id="step-panel">
      {children}
    </div>
  </li>
);

export default StepByStepNavigationStep;
