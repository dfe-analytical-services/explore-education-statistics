import { useAuthContext } from '@admin/contexts/AuthContext';
import classNames from 'classnames';
import React from 'react';
import Link from './Link';

interface Props {
  wide?: boolean;
}

const PageFooter = ({ wide }: Props) => {
  const { user } = useAuthContext();

  return (
    <footer className="govuk-footer" role="contentinfo">
      <div
        className={classNames('govuk-width-container', {
          'dfe-width-container--wide': wide,
        })}
      >
        <div className="govuk-footer__meta">
          <div className="govuk-footer__meta-item govuk-footer__meta-item--grow">
            {user && (
              <>
                <h2 className="govuk-visually-hidden">Support links</h2>
                <ul className="govuk-footer__inline-list">
                  {user?.permissions.isBauUser && (
                    <>
                      <li className="govuk-footer__inline-list-item">
                        <a className="govuk-footer__link" href="/docs">
                          API Documentation
                        </a>
                      </li>
                      <li className="govuk-footer__inline-list-item">
                        <Link className="govuk-footer__link" to="/prototypes">
                          Prototypes
                        </Link>
                      </li>
                    </>
                  )}
                  <li className="govuk-footer__inline-list-item">
                    <Link className="govuk-footer__link" to="/contact-us">
                      Contact us
                    </Link>
                  </li>
                </ul>
              </>
            )}
            <div className="govuk-footer__meta-custom">
              This service is maintained by the{' '}
              <Link
                className="govuk-footer__link"
                to="https://www.gov.uk/government/organisations/department-for-education"
              >
                Department for Education
              </Link>
            </div>
            <div className="govuk-footer__meta-custom">
              Our statistical practice is regulated by the{' '}
              <Link
                className="govuk-footer__link"
                to="https://osr.statisticsauthority.gov.uk/what-we-do/"
              >
                Office for Statistics Regulation
              </Link>{' '}
              (OSR)
            </div>
            <svg
              role="presentation"
              focusable="false"
              className="govuk-footer__licence-logo"
              xmlns="http://www.w3.org/2000/svg"
              viewBox="0 0 483.2 195.7"
              height="17"
              width="41"
            >
              <path
                fill="currentColor"
                d="M421.5 142.8V.1l-50.7 32.3v161.1h112.4v-50.7zm-122.3-9.6A47.12 47.12 0 0 1 221 97.8c0-26 21.1-47.1 47.1-47.1 16.7 0 31.4 8.7 39.7 21.8l42.7-27.2A97.63 97.63 0 0 0 268.1 0c-36.5 0-68.3 20.1-85.1 49.7A98 98 0 0 0 97.8 0C43.9 0 0 43.9 0 97.8s43.9 97.8 97.8 97.8c36.5 0 68.3-20.1 85.1-49.7a97.76 97.76 0 0 0 149.6 25.4l19.4 22.2h3v-87.8h-80l24.3 27.5zM97.8 145c-26 0-47.1-21.1-47.1-47.1s21.1-47.1 47.1-47.1 47.2 21 47.2 47S123.8 145 97.8 145"
              />
            </svg>
            <span className="govuk-footer__licence-description">
              All content is available under the{' '}
              <Link
                className="govuk-footer__link"
                to="https://www.nationalarchives.gov.uk/doc/open-government-licence/version/3/"
                rel="license"
              >
                Open Government Licence v3.0
              </Link>
              , except where otherwise stated
            </span>

            {process.env.REACT_APP_BUILD_NUMBER && (
              <p
                aria-hidden
                className="govuk-!-margin-top-3 govuk-!-font-size-14"
              >
                Build: {process.env.REACT_APP_BUILD_NUMBER}
              </p>
            )}
          </div>
          <div className="govuk-footer__meta-item">
            <Link
              className="govuk-footer__link govuk-footer__copyright-logo"
              to="https://www.nationalarchives.gov.uk/information-management/re-using-public-sector-information/uk-government-licensing-framework/crown-copyright/"
            >
              © Crown copyright
            </Link>
          </div>
        </div>
      </div>
    </footer>
  );
};
export default PageFooter;
