import React from 'react';
import { Link, Route } from 'react-router-dom';

interface Props {
  current: string;
}

const Breadcrumbs = ({ current }: Props) => (
  <Route path="*">
    <div className="govuk-breadcrumbs">
      <ol className="govuk-breadcrumbs__list">
        <li className="govuk-breadcrumbs__list-item">
          <Link className="govuk-breadcrumbs__link" to="/">
            Home
          </Link>
        </li>
        {window.location.pathname
          .split('/')
          .slice(1)
          .map((elem, index) => (
            <li key={index} className="govuk-breadcrumbs__list-item">
              <Link className="govuk-breadcrumbs__link" to={`/${elem}`}>
                {elem}
              </Link>
            </li>
          ))}
      </ol>
    </div>
  </Route>
);

export default Breadcrumbs;
