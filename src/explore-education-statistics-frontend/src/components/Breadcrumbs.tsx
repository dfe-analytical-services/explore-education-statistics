import React from 'react';
import Link from './Link';

export interface BreadcrumbsProps {
  breadcrumbs?: {
    link?: string;
    name: string;
  }[];
}

const Breadcrumbs = ({ breadcrumbs = [] }: BreadcrumbsProps) => {
  const currentBreadcrumbIndex = breadcrumbs.length - 1;

  return (
    <nav className="govuk-breadcrumbs" aria-label="Breadcrumb">
      <ol className="govuk-breadcrumbs__list" data-testid="breadcrumbs--list">
        <li className="govuk-breadcrumbs__list-item">
          <Link className="govuk-breadcrumbs__link" to="/">
            Home
          </Link>
        </li>
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
    </nav>
  );
};

export default Breadcrumbs;
