import { dashboardRoute } from '@admin/routes/routes';
import React from 'react';
import Link from './Link';

export interface BreadcrumbsProps {
  breadcrumbs?: {
    link?: string;
    name: string;
  }[];
  homePath?: string;
}

const Breadcrumbs = ({
  breadcrumbs = [],
  homePath = dashboardRoute.path,
}: BreadcrumbsProps) => {
  const currentBreadcrumbIndex = breadcrumbs.length - 1;

  return (
    <div className="govuk-breadcrumbs">
      <ol className="govuk-breadcrumbs__list" data-testid="breadcrumbs--list">
        {homePath && (
          <li className="govuk-breadcrumbs__list-item">
            <Link className="govuk-breadcrumbs__link" to={homePath}>
              Home
            </Link>
          </li>
        )}
        {breadcrumbs.map((breadcrumb, index) => {
          return (
            <li
              key={`${breadcrumb.name}_${breadcrumb.link}`}
              className="govuk-breadcrumbs__list-item"
            >
              {index < currentBreadcrumbIndex && breadcrumb.link ? (
                <Link className="govuk-breadcrumbs__link" to={breadcrumb.link}>
                  {breadcrumb.name}
                </Link>
              ) : (
                breadcrumb.name
              )}
            </li>
          );
        })}
      </ol>
    </div>
  );
};

export default Breadcrumbs;
