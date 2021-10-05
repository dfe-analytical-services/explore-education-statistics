import classNames from 'classnames';
import logo from 'govuk-frontend/govuk/assets/images/govuk-logotype-crown.png';
import React from 'react';

interface Props {
  wide?: boolean;
}

const PageHeader = ({ wide }: Props) => (
  <>
    <a href="#main-content" className="govuk-skip-link">
      Skip to main content
    </a>

    <header className="govuk-header " role="banner" data-module="header">
      <div
        className={classNames(
          'govuk-header__container',
          'govuk-width-container',
          {
            'dfe-width-container--wide': wide,
          },
        )}
      >
        <div className="govuk-header__logo">
          <a
            href="//www.gov.uk"
            className="govuk-header__link govuk-header__link--homepage"
          >
            <span className="govuk-header__logotype">
              <img
                alt="GOV.UK"
                src={logo.src}
                className="govuk-header__logotype-crown-fallback-image"
              />
              <span className="govuk-header__logotype-text">GOV.UK</span>
            </span>
          </a>
        </div>
        <div className="govuk-header__content">
          <a
            href="/"
            className="govuk-header__link govuk-header__link--service-name"
          >
            Explore education statistics
          </a>
        </div>
      </div>
    </header>
  </>
);

export default PageHeader;
