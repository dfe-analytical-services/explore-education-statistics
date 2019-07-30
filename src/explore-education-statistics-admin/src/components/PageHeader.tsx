import {LoginContext} from '@admin/components/Login';
import loginService from '@admin/services/sign-in/service';
import {Authentication} from '@admin/services/sign-in/types';
import classNames from 'classnames';
import logo from 'govuk-frontend/assets/images/govuk-logotype-crown.png';
import React, {useContext} from 'react';

interface Props {
  wide?: boolean;
}

const PageHeader = ({ wide }: Props) => {

  const authentication = useContext(LoginContext);

  return (
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
                  src={logo}
                  className="govuk-header__logotype-crown-fallback-image"
                />
                <span className="govuk-header__logotype-text"> GOV.UK</span>
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

            <button
              type="button"
              className="govuk-header__menu-button js-header-toggle"
              aria-controls="navigation"
              aria-label="Show or hide Top Level Navigation"
            >
              Menu
            </button>
            <nav>
              <ul
                id="navigation"
                className="govuk-header__navigation "
                aria-label="Top Level Navigation"
              >
                <li className="govuk-header__navigation-item">
                  <a
                    className="govuk-header__link"
                    href="/prototypes/documentation"
                  >
                    Administrators' guide
                  </a>
                </li>
                {authentication.user
                  ? <LoggedInLinks user={authentication.user} />
                  : <NotLoggedInLinks />}
              </ul>
            </nav>
          </div>
        </div>
      </header>
    </>
  );
};

const LoggedInLinks = ({ user }: Authentication) => (
  <>
    <li className="govuk-header__navigation-item">
      <a className="govuk-header__link" href="#">
        {user ? user.name : ''}
      </a>
    </li>
    <li className="govuk-header__navigation-item">
      <a className="govuk-header__link" href={loginService.getSignOutLink()}>
        Sign out
      </a>
    </li>
  </>
);

const NotLoggedInLinks = () => (
  <>
    <li className="govuk-header__navigation-item">
      <a className="govuk-header__link" href={loginService.getSignInLink()}>
        Sign in
      </a>
    </li>
  </>
);

export default PageHeader;
