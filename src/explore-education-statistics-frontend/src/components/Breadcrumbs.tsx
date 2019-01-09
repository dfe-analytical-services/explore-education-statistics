import React from 'react';
import withBreadcrumbs, { InjectedProps } from 'react-router-breadcrumbs-hoc';
import Link from './Link';

const Breadcrumbs = ({ breadcrumbs }: InjectedProps<{}>) => {
  const currentBreadcrumbIndex = breadcrumbs.length - 1;

  return (
    <div className="govuk-breadcrumbs">
      <ol className="govuk-breadcrumbs__list">
        {breadcrumbs.map((breadcrumb, index) =>
          index < currentBreadcrumbIndex ? (
            <li key={index} className="govuk-breadcrumbs__list-item">
              <Link
                className="govuk-breadcrumbs__link"
                to={breadcrumb.props.match.url}
              >
                {breadcrumb}
              </Link>
            </li>
          ) : (
            <li
              aria-current="page"
              className="govuk-breadcrumbs__list-item"
              key={index}
            >
              {breadcrumb}
            </li>
          ),
        )}
      </ol>
    </div>
  );
};

export default withBreadcrumbs([])(Breadcrumbs);
