import Link from '@admin/components/Link';
import { useAuthContext } from '@admin/contexts/AuthContext';
import loginService from '@admin/services/sign-in/service';
import { Authentication } from '@admin/services/sign-in/types';
import classNames from 'classnames';
import logo from 'govuk-frontend/govuk/assets/images/govuk-logotype-crown.png';
import React from 'react';
import styles from './PageHeader.module.scss';

interface Props {
  wide?: boolean;
}

const PageHeader = ({ wide }: Props) => {
  const { user } = useAuthContext();

  const envs = {
    localhost: 'dfeEnv--local',
    'admin.dev': 'dfeEnv--dev',
    'admin.test': 'dfeEnv--test',
    'admin.pre-production': 'dfeEnv--pre-prod',
    'admin.explore': 'dfeEnv--prod',
  };

  const matchingEnv = Object.keys(envs).find(env =>
    window.location.href.includes(env),
  );

  const envClassName = envs[matchingEnv as keyof typeof envs] ?? envs.localhost;

  return (
    <>
      <a href="#main-content" className="govuk-skip-link">
        Skip to main content
      </a>

      <header
        className={classNames('govuk-header', styles[envClassName])}
        role="banner"
        data-module="header"
      >
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
              href={user && user.permissions.canAccessAnalystPages ? '/' : '#'}
              className="govuk-header__link govuk-header__link--service-name"
            >
              Explore education statistics
            </a>

            <button
              type="button"
              className="govuk-header__menu-button govuk-js-header-toggle"
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
                {user && user.validToken ? (
                  <LoggedInLinks user={user} />
                ) : (
                  <NotLoggedInLinks />
                )}
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
    {user && user.permissions.canAccessAnalystPages && (
      <li className="govuk-header__navigation-item">
        <a className="govuk-header__link" href="/documentation">
          Administrators' guide
        </a>
      </li>
    )}

    {user &&
      (user.permissions.canAccessUserAdministrationPages ||
        user.permissions.canAccessMethodologyAdministrationPages) && (
        <li className="govuk-header__navigation-item">
          <a className="govuk-header__link" href="/administration">
            Platform administration
          </a>
        </li>
      )}
    <li className="govuk-header__navigation-item">
      <Link className="govuk-header__link" to={loginService.getSignOutLink()}>
        Sign out
      </Link>
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
