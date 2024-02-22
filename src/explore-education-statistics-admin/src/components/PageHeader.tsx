import Link from '@admin/components/Link';
import { handleLogout } from '@admin/auth/msal';
import { signInRoute } from '@admin/routes/routes';
import { useAuthContext, User } from '@admin/contexts/AuthContext';
import ButtonText from '@common/components/ButtonText';
import { useMobileMedia } from '@common/hooks/useMedia';
import classNames from 'classnames';
import React, { useState } from 'react';
import styles from './PageHeader.module.scss';

interface Props {
  wide?: boolean;
}

const PageHeader = ({ wide }: Props) => {
  const { user } = useAuthContext();
  const { isMedia: isMobileMedia } = useMobileMedia();
  const [menuOpen, setMenuOpen] = useState(false);

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
                <svg
                  aria-hidden="true"
                  focusable="false"
                  className="govuk-header__logotype-crown"
                  xmlns="http://www.w3.org/2000/svg"
                  viewBox="0 0 32 30"
                  height="30"
                  width="32"
                >
                  <path
                    fill="currentColor"
                    fillRule="evenodd"
                    d="M22.6 10.4c-1 .4-2-.1-2.4-1-.4-.9.1-2 1-2.4.9-.4 2 .1 2.4 1s-.1 2-1 2.4m-5.9 6.7c-.9.4-2-.1-2.4-1-.4-.9.1-2 1-2.4.9-.4 2 .1 2.4 1s-.1 2-1 2.4m10.8-3.7c-1 .4-2-.1-2.4-1-.4-.9.1-2 1-2.4.9-.4 2 .1 2.4 1s0 2-1 2.4m3.3 4.8c-1 .4-2-.1-2.4-1-.4-.9.1-2 1-2.4.9-.4 2 .1 2.4 1s-.1 2-1 2.4M17 4.7l2.3 1.2V2.5l-2.3.7-.2-.2.9-3h-3.4l.9 3-.2.2c-.1.1-2.3-.7-2.3-.7v3.4L15 4.7c.1.1.1.2.2.2l-1.3 4c-.1.2-.1.4-.1.6 0 1.1.8 2 1.9 2.2h.7c1-.2 1.9-1.1 1.9-2.1 0-.2 0-.4-.1-.6l-1.3-4c-.1-.2 0-.2.1-.3m-7.6 5.7c.9.4 2-.1 2.4-1 .4-.9-.1-2-1-2.4-.9-.4-2 .1-2.4 1s0 2 1 2.4m-5 3c.9.4 2-.1 2.4-1 .4-.9-.1-2-1-2.4-.9-.4-2 .1-2.4 1s.1 2 1 2.4m-3.2 4.8c.9.4 2-.1 2.4-1 .4-.9-.1-2-1-2.4-.9-.4-2 .1-2.4 1s0 2 1 2.4m14.8 11c4.4 0 8.6.3 12.3.8 1.1-4.5 2.4-7 3.7-8.8l-2.5-.9c.2 1.3.3 1.9 0 2.7-.4-.4-.8-1.1-1.1-2.3l-1.2 4c.7-.5 1.3-.8 2-.9-1.1 2.5-2.6 3.1-3.5 3-1.1-.2-1.7-1.2-1.5-2.1.3-1.2 1.5-1.5 2.1-.1 1.1-2.3-.8-3-2-2.3 1.9-1.9 2.1-3.5.6-5.6-2.1 1.6-2.1 3.2-1.2 5.5-1.2-1.4-3.2-.6-2.5 1.6.9-1.4 2.1-.5 1.9.8-.2 1.1-1.7 2.1-3.5 1.9-2.7-.2-2.9-2.1-2.9-3.6.7-.1 1.9.5 2.9 1.9l.4-4.3c-1.1 1.1-2.1 1.4-3.2 1.4.4-1.2 2.1-3 2.1-3h-5.4s1.7 1.9 2.1 3c-1.1 0-2.1-.2-3.2-1.4l.4 4.3c1-1.4 2.2-2 2.9-1.9-.1 1.5-.2 3.4-2.9 3.6-1.9.2-3.4-.8-3.5-1.9-.2-1.3 1-2.2 1.9-.8.7-2.3-1.2-3-2.5-1.6.9-2.2.9-3.9-1.2-5.5-1.5 2-1.3 3.7.6 5.6-1.2-.7-3.1 0-2 2.3.6-1.4 1.8-1.1 2.1.1.2.9-.3 1.9-1.5 2.1-.9.2-2.4-.5-3.5-3 .6 0 1.2.3 2 .9l-1.2-4c-.3 1.1-.7 1.9-1.1 2.3-.3-.8-.2-1.4 0-2.7l-2.9.9C1.3 23 2.6 25.5 3.7 30c3.7-.5 7.9-.8 12.3-.8"
                  />
                </svg>
                <span className="govuk-header__logotype-text">GOV.UK</span>
              </span>
            </a>
          </div>
          <div className="govuk-header__content">
            <Link
              to="/"
              className="govuk-header__link govuk-header__link--service-name"
            >
              Explore education statistics
            </Link>

            <nav
              aria-label="Top Level Navigation"
              className="govuk-header__navigation"
            >
              <button
                type="button"
                className={classNames(
                  'govuk-header__menu-button govuk-js-header-toggle',
                  { 'govuk-header__menu-button--open': menuOpen },
                )}
                aria-controls="navigation"
                aria-expanded={menuOpen}
                hidden={!isMobileMedia}
                aria-label="Show or hide Top Level Navigation"
                onClick={() => setMenuOpen(!menuOpen)}
              >
                Menu
              </button>
              <ul
                id="navigation"
                className="govuk-header__navigation-list"
                hidden={isMobileMedia && !menuOpen}
              >
                {user ? <LoggedInLinks user={user} /> : <NotLoggedInLinks />}
              </ul>
            </nav>
          </div>
        </div>
      </header>
    </>
  );
};

interface LoggedInLinksProps {
  user: User;
}

const LoggedInLinks = ({ user }: LoggedInLinksProps) => (
  <>
    {/* EES-2464
      {user.permissions.canAccessAnalystPages && (
      <li className="govuk-header__navigation-item">
        <a className="govuk-header__link" href="/documentation">
          Administrators' guide
        </a>
      </li>
    )} */}

    {user.permissions.isBauUser && (
      <li className="govuk-header__navigation-item">
        <a className="govuk-header__link" href="/administration">
          Platform administration
        </a>
      </li>
    )}
    <li className="govuk-header__navigation-item">
      <ButtonText
        className={`govuk-header__link ${styles.signOutLink}`}
        onClick={() => handleLogout()}
      >
        Sign out
      </ButtonText>
    </li>
  </>
);

const NotLoggedInLinks = () => (
  <li className="govuk-header__navigation-item">
    <a className="govuk-header__link" href={signInRoute.path}>
      Sign in
    </a>
  </li>
);

export default PageHeader;
