import React, { Component } from 'react';
import { Helmet } from 'react-helmet';

class CookiesPage extends Component {
  public render() {
    return (
      <>
        <Helmet>
          <title>Cookies - GOV.UK</title>
        </Helmet>
        <h1 className="govuk-heading-xl">Cookies</h1>
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
        </ul>

        <p>GOV.UK cookies aren’t used to identify you personally.</p>
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

        <h2 className="govuk-heading-m">
          How cookies are used on this service
        </h2>
        <h3>Our introductory message</h3>
        <p>
          You may see a pop-up welcome message when you first visit GOV.UK.
          We’ll store a cookie so that your computer knows you’ve seen it and
          knows not to show it again.
        </p>

        <table className="govuk-table">
          <thead className="govuk-table__head">
            <tr className="govuk-table__row">
              <th className="govuk-table__header" scope="col">
                Name
              </th>
              <th className="govuk-table__header" scope="col">
                Purpose
              </th>
              <th className="govuk-table__header" scope="col">
                Expires
              </th>
            </tr>
          </thead>
          <tbody className="govuk-table__body">
            <tr className="govuk-table__row">
              <td className="govuk-table__cell">seen_cookie_message</td>
              <td className="govuk-table__cell">
                Saves a message to let us know that you’ve seen our cookie
                message
              </td>
              <td className="govuk-table__cell">1 month</td>
            </tr>
          </tbody>
        </table>
      </>
    );
  }
}

export default CookiesPage;
