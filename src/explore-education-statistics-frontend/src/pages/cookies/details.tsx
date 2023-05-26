import Page from '@frontend/components/Page';
import React from 'react';
import { allowedCookies } from '@frontend/hooks/useCookies';
import Link from '@frontend/components/Link';
import { NextPage } from 'next';

const CookiesPage: NextPage = () => {
  return (
    <Page
      title="Details about cookies"
      breadcrumbs={[{ name: 'Cookies', link: '/cookies' }]}
    >
      <p>
        The Explore education statistics service puts small files (known as
        ‘cookies’) onto your computer to collect information about how you
        browse the site.
      </p>
      <p>Cookies are used to:</p>
      <ul>
        <li>
          measure how you use the website so it can be updated and improved
          based on your needs
        </li>
        <li>
          remember the notifications you’ve seen so that we don’t show them to
          you again
        </li>
        <li>
          store various settings you have selected so they will be used when you
          return to the site.
        </li>
      </ul>
      <div className="govuk-inset-text">
        <p>GOV.UK cookies aren’t used to identify you personally.</p>
      </div>
      <p>
        You’ll normally see a message on the site before we store a cookie on
        your computer.
      </p>
      <p>
        Find out more about{' '}
        <a rel="external" href="http://www.aboutcookies.org/">
          how to manage cookies.
        </a>
      </p>

      <h2>How cookies are used on this service</h2>
      <section>
        <h3>Measuring website usage (Google Analytics)</h3>
        <p>
          We use Google Analytics software to collect information about how you
          use GOV.UK. We do this to help make sure the site is meeting the needs
          of its users and to help us make improvements.
        </p>
        <p>Google Analytics stores information about:</p>
        <ul>
          <li>the pages you visit on this service</li>
          <li>how long you spend on each page</li>
          <li>how you got to the site</li>
          <li>what you click on while you’re visiting the site</li>
        </ul>
        <p>
          We don’t collect or store your personal information (eg your name or
          address) so this information can’t be used to identify who you are.
        </p>
        <p>We don’t allow Google to use or share our analytics data.</p>
        <p>Google Analytics sets the following cookies:</p>
        <table>
          <thead>
            <tr>
              <th scope="col">Name</th>
              <th scope="col">Purpose</th>
              <th scope="col">Expires</th>
            </tr>
          </thead>
          <tbody>
            <tr>
              <td>_ga</td>
              <td>
                This helps us count how many people visit GOV.UK by tracking if
                you’ve visited before
              </td>
              <td>2 years</td>
            </tr>
            <tr>
              <td>_gid</td>
              <td>
                This helps us count how many people visit GOV.UK by tracking if
                you’ve visited before
              </td>
              <td>24 hours</td>
            </tr>
            <tr>
              <td>_gat</td>
              <td>
                Used to manage the rate at which page view requests are made
              </td>
              <td>1 minute</td>
            </tr>
          </tbody>
        </table>
        <div className="govuk-inset-text">
          <p>
            You can{' '}
            <Link to="https://tools.google.com/dlpage/gaoptout">
              opt out of Google Analytics cookies.
            </Link>
          </p>
        </div>
      </section>
      <section>
        <h3>Our introductory message</h3>
        <p>
          You may see a pop-up welcome message when you first visit GOV.UK.
          We’ll store a cookie so that your computer knows you’ve seen it and
          knows not to show it again.
        </p>

        <table>
          <thead>
            <tr>
              <th scope="col">Name</th>
              <th scope="col">Purpose</th>
              <th scope="col">Expires</th>
            </tr>
          </thead>
          <tbody>
            <tr>
              <td>{allowedCookies.bannerSeen.name}</td>
              <td>
                Saves a message to let us know that you’ve seen our cookie
                message
              </td>
              <td>{allowedCookies.bannerSeen.duration}</td>
            </tr>
          </tbody>
          <tbody>
            <tr>
              <td>{allowedCookies.userTestingBannerSeen.name}</td>
              <td>
                Saves a message to let us know that you’ve seen our user testing
                invite message
              </td>
              <td>{allowedCookies.userTestingBannerSeen.duration}</td>
            </tr>
          </tbody>
        </table>
      </section>
      <section>
        <h3>Your cookie settings</h3>
        <p>
          We will store your cookie settings as a cookie, so that they remain if
          you return to GOV.UK: Explore education statistics.
        </p>

        <table>
          <thead>
            <tr>
              <th scope="col">Name</th>
              <th scope="col">Purpose</th>
              <th scope="col">Expires</th>
            </tr>
          </thead>

          <tbody>
            <tr>
              <td>{allowedCookies.disableGA.name}</td>
              <td>Disables Google Analytics tracking</td>
              <td>{allowedCookies.disableGA.duration}</td>
            </tr>
          </tbody>
        </table>
        <h4>Change your settings</h4>
        <p>
          You can{' '}
          <Link to="/cookies">
            change which cookies you're happy for us to use
          </Link>
          .
        </p>
      </section>
      <section>
        <h3>Microsoft Application Insights</h3>
        <p>Microsoft Application Insights collects telemetry information.</p>

        <table>
          <thead>
            <tr>
              <th scope="col">Name</th>
              <th scope="col">Purpose</th>
              <th scope="col">Expires</th>
            </tr>
          </thead>
          <tbody>
            <tr>
              <td>ai_user</td>
              <td>Used to identify returning users</td>
              <td>1 year</td>
            </tr>
            <tr>
              <td>ai_session</td>
              <td>Anonymous session identifer to group a user's activities</td>
              <td>15 minutes</td>
            </tr>
          </tbody>
        </table>
      </section>
    </Page>
  );
};

export default CookiesPage;
