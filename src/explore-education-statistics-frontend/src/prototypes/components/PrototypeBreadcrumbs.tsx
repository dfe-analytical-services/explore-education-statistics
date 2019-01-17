import React from 'react';
import Link from '../../components/Link';

export interface Breadcrumb {
  link?: string;
  text: string;
}

interface Props {
  items: Breadcrumb[];
}

const PrototypeBreadcrumbs = ({ items }: Props) => {
  return (
    <div className="govuk-breadcrumbs">
      <ol className="govuk-breadcrumbs__list">
        <li className="govuk-breadcrumbs__list-item">
          <Link className="govuk-breadcrumbs__link" to="/prototypes/home">
            Home
          </Link>
        </li>
        {items.map(({ link, text }) => (
          <li className="govuk-breadcrumbs__list-item" key={text}>
            <Link
              className="govuk-breadcrumbs__link"
              to={link || '/prototypes'}
            >
              {text}
            </Link>
          </li>
        ))}
      </ol>
    </div>
  );
};

export default PrototypeBreadcrumbs;
