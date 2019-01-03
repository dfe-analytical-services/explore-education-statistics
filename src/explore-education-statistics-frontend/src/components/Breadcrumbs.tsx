import React from 'react';
import { Link } from 'react-router-dom';

interface Props {
  current: string,
}

const Breadcrumbs = ({ current }: Props) => (
  <div className="govuk-breadcrumbs">
    <ol className="govuk-breadcrumbs__list">
      <li className="govuk-breadcrumbs__list-item">
        <Link className="govuk-breadcrumbs__link" to='/'>Home</Link>
      </li>
      <li className="govuk-breadcrumbs__list-item">
        <Link className="govuk-breadcrumbs__link" to='/'>Education training and skills</Link>
      </li>
      <li className="govuk-breadcrumbs__list-item">
        <Link className="govuk-breadcrumbs__link" to='/themes'>Themes</Link>
      </li>
      {current &&
      <li className="govuk-breadcrumbs__list-item" aria-current="page">{current}</li>
      }
    </ol>
  </div>
);

export default Breadcrumbs;
