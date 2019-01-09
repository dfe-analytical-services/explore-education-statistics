import React from 'react';
import withBreadcrumbs, { InjectedProps } from 'react-router-breadcrumbs-hoc';
import Link from './Link';

const Breadcrumbs = ({ breadcrumbs }: InjectedProps<{}>) => {
  return (
    <div className="govuk-breadcrumbs">
      <ol className="govuk-breadcrumbs__list">
        {breadcrumbs.map((breadcrumb, index) => (
          <li key={index} className="govuk-breadcrumbs__list-item">
            <Link
              className="govuk-breadcrumbs__link"
              to={breadcrumb.props.match.url}
            >
              {breadcrumb}
            </Link>
          </li>
        ))}
      </ol>
    </div>
  );
};

export default withBreadcrumbs([])(Breadcrumbs);
