import VisuallyHidden from '@common/components/VisuallyHidden';
import classNames from 'classnames';
import React from 'react';
import Link from './Link';
import { PageWidth } from './Page';

interface Props {
  width?: PageWidth;
}

const PageFooter = ({ width }: Props) => (
  <footer className="govuk-footer" role="contentinfo">
    <div
      className={classNames('govuk-width-container', {
        'dfe-width-container--wide': width === 'wide',
        'dfe-width-container--full': width === 'full',
      })}
    >
      <svg
        focusable="false"
        role="presentation"
        xmlns="http://www.w3.org/2000/svg"
        viewBox="0 0 64 60"
        height="30"
        width="32"
        fill="currentcolor"
        className="govuk-footer__crown"
      >
        <g>
          <circle cx="20" cy="17.6" r="3.7" />
          <circle cx="10.2" cy="23.5" r="3.7" />
          <circle cx="3.7" cy="33.2" r="3.7" />
          <circle cx="31.7" cy="30.6" r="3.7" />
          <circle cx="43.3" cy="17.6" r="3.7" />
          <circle cx="53.2" cy="23.5" r="3.7" />
          <circle cx="59.7" cy="33.2" r="3.7" />
          <circle cx="31.7" cy="30.6" r="3.7" />
          <path d="M33.1,9.8c.2-.1.3-.3.5-.5l4.6,2.4v-6.8l-4.6,1.5c-.1-.2-.3-.3-.5-.5l1.9-5.9h-6.7l1.9,5.9c-.2.1-.3.3-.5.5l-4.6-1.5v6.8l4.6-2.4c.1.2.3.3.5.5l-2.6,8c-.9,2.8,1.2,5.7,4.1,5.7h0c3,0,5.1-2.9,4.1-5.7l-2.6-8ZM37,37.9s-3.4,3.8-4.1,6.1c2.2,0,4.2-.5,6.4-2.8l-.7,8.5c-2-2.8-4.4-4.1-5.7-3.8.1,3.1.5,6.7,5.8,7.2,3.7.3,6.7-1.5,7-3.8.4-2.6-2-4.3-3.7-1.6-1.4-4.5,2.4-6.1,4.9-3.2-1.9-4.5-1.8-7.7,2.4-10.9,3,4,2.6,7.3-1.2,11.1,2.4-1.3,6.2,0,4,4.6-1.2-2.8-3.7-2.2-4.2.2-.3,1.7.7,3.7,3,4.2,1.9.3,4.7-.9,7-5.9-1.3,0-2.4.7-3.9,1.7l2.4-8c.6,2.3,1.4,3.7,2.2,4.5.6-1.6.5-2.8,0-5.3l5,1.8c-2.6,3.6-5.2,8.7-7.3,17.5-7.4-1.1-15.7-1.7-24.5-1.7h0c-8.8,0-17.1.6-24.5,1.7-2.1-8.9-4.7-13.9-7.3-17.5l5-1.8c-.5,2.5-.6,3.7,0,5.3.8-.8,1.6-2.3,2.2-4.5l2.4,8c-1.5-1-2.6-1.7-3.9-1.7,2.3,5,5.2,6.2,7,5.9,2.3-.4,3.3-2.4,3-4.2-.5-2.4-3-3.1-4.2-.2-2.2-4.6,1.6-6,4-4.6-3.7-3.7-4.2-7.1-1.2-11.1,4.2,3.2,4.3,6.4,2.4,10.9,2.5-2.8,6.3-1.3,4.9,3.2-1.8-2.7-4.1-1-3.7,1.6.3,2.3,3.3,4.1,7,3.8,5.4-.5,5.7-4.2,5.8-7.2-1.3-.2-3.7,1-5.7,3.8l-.7-8.5c2.2,2.3,4.2,2.7,6.4,2.8-.7-2.3-4.1-6.1-4.1-6.1h10.6,0Z" />
        </g>
      </svg>
      <div className="govuk-footer__meta">
        <div className="govuk-footer__meta-item govuk-footer__meta-item--grow">
          <h2 className="govuk-visually-hidden">Support links</h2>
          <ul className="govuk-footer__inline-list">
            <li className="govuk-footer__inline-list-item">
              <Link
                className="govuk-footer__link"
                to="/cookies"
                data-testid="footer--cookies-link"
              >
                Cookies
              </Link>
            </li>
            <li className="govuk-footer__inline-list-item">
              <Link
                className="govuk-footer__link"
                to="https://www.gov.uk/government/organisations/department-for-education/about/personal-information-charter"
                rel="noopener noreferrer nofollow"
                target="_blank"
                data-testid="footer--privacy-notice-link"
              >
                Privacy notice{' '}
                <VisuallyHidden>(opens in new tab)</VisuallyHidden>
              </Link>
            </li>
            <li className="govuk-footer__inline-list-item">
              <Link
                className="govuk-footer__link"
                to="/contact-us"
                data-testid="footer--contact-link"
              >
                Contact us
                <VisuallyHidden> about general enquiries</VisuallyHidden>
              </Link>
            </li>
            <li className="govuk-footer__inline-list-item">
              <Link
                className="govuk-footer__link"
                to="/accessibility-statement"
                data-testid="footer--contact-link"
              >
                Accessibility statement
              </Link>
            </li>
            <li className="govuk-footer__inline-list-item">
              <Link
                className="govuk-footer__link"
                to="/glossary"
                data-testid="footer--glossary-link"
              >
                Glossary
              </Link>
            </li>
            <li className="govuk-footer__inline-list-item">
              <Link
                className="govuk-footer__link"
                to="/help-support"
                data-testid="footer--help-support-link"
              >
                Help and support
              </Link>
            </li>
          </ul>
          <div className="govuk-footer__meta-custom">
            This service is maintained by the{' '}
            <Link
              className="govuk-footer__link"
              to="https://www.gov.uk/government/organisations/department-for-education"
              rel="noopener noreferrer nofollow"
              target="_blank"
            >
              Department for Education{' '}
              <VisuallyHidden>(opens in new tab)</VisuallyHidden>
            </Link>
          </div>
          <div className="govuk-footer__meta-custom">
            Our statistical practice is regulated by the{' '}
            <Link
              className="govuk-footer__link"
              to="https://osr.statisticsauthority.gov.uk/what-we-do/"
              rel="noopener noreferrer nofollow"
              target="_blank"
            >
              Office for Statistics Regulation{' '}
              <VisuallyHidden>(opens in new tab)</VisuallyHidden>
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
              rel="license noopener noreferrer nofollow"
              target="_blank"
            >
              Open Government Licence v3.0{' '}
              <VisuallyHidden>(opens in new tab)</VisuallyHidden>
            </Link>
            , except where otherwise stated
          </span>

          {process.env.BUILD_NUMBER && (
            <p
              aria-hidden
              className="govuk-!-margin-top-3 govuk-!-font-size-14"
            >
              Build: {process.env.BUILD_NUMBER}
            </p>
          )}
        </div>
        <div className="govuk-footer__meta-item">
          <Link
            className="govuk-footer__link govuk-footer__copyright-logo"
            to="https://www.nationalarchives.gov.uk/information-management/re-using-public-sector-information/uk-government-licensing-framework/crown-copyright/"
            rel="noopener noreferrer nofollow"
            target="_blank"
          >
            © Crown copyright{' '}
            <VisuallyHidden>(opens in new tab)</VisuallyHidden>
          </Link>
        </div>
      </div>
    </div>
  </footer>
);

export default PageFooter;
