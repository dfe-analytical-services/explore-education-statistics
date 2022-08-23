import { ArrowLeft, ArrowRight } from '@common/components/ArrowIcons';
import generatePageNumbers from '@common/components/util/generatePageNumbers';
import classNames from 'classnames';
import React, { ReactNode } from 'react';

const paginationLinkClassName = 'govuk-link govuk-pagination__link';

interface LinkRenderProps {
  'aria-current'?: 'page' | undefined;
  'aria-label'?: string;
  children: ReactNode;
  className: string;
  rel?: 'next' | 'prev';
  to: string;
}

export interface PaginationProps {
  baseUrl: string;
  currentPage: number;
  label?: string;
  queryParams?: Record<string, unknown>;
  renderLink: (props: LinkRenderProps) => ReactNode;
  totalPages: number;
}

const Pagination = ({
  baseUrl,
  currentPage,
  label = 'Pagination',
  queryParams,
  renderLink,
  totalPages,
}: PaginationProps) => {
  const pageNumbers = generatePageNumbers({
    currentPage,
    totalPages,
  });

  if (!pageNumbers.length) {
    return null;
  }

  const queryString = queryParams
    ? `&${Object.keys(queryParams)
        .map(key => `${key}=${queryParams[key]}`)
        .join('&')}`
    : '';

  return (
    <nav className="govuk-pagination" role="navigation" aria-label={label}>
      {currentPage !== 1 && (
        <div className="govuk-pagination__prev">
          {renderLink({
            className: paginationLinkClassName,
            rel: 'prev',
            to: `${baseUrl}?page=${currentPage - 1}${queryString}`,
            children: (
              <>
                <ArrowLeft className="govuk-pagination__icon govuk-pagination__icon--prev" />
                <span className="govuk-pagination__link-title">Previous</span>
              </>
            ),
          })}
        </div>
      )}

      <ul className="govuk-pagination__list">
        {pageNumbers.map((pageNumber, index) => {
          if (pageNumber === null) {
            return (
              <li
                key={index.toString()}
                className="govuk-pagination__item govuk-pagination__item--ellipses"
              >
                …
              </li>
            );
          }
          return (
            <li
              key={index.toString()}
              className={classNames('govuk-pagination__item', {
                'govuk-pagination__item--current': pageNumber === currentPage,
              })}
            >
              {renderLink({
                'aria-current': currentPage === pageNumber ? 'page' : undefined,
                'aria-label': `Page ${pageNumber}`,
                className: paginationLinkClassName,
                to: `${baseUrl}?page=${pageNumber}${queryString}`,
                children: <>{pageNumber}</>,
              })}
            </li>
          );
        })}
      </ul>

      {currentPage !== totalPages && (
        <div className="govuk-pagination__next">
          {renderLink({
            className: paginationLinkClassName,
            rel: 'next',
            to: `${baseUrl}?page=${currentPage + 1}${queryString}`,
            children: (
              <>
                <span className="govuk-pagination__link-title">Next</span>
                <ArrowRight className="govuk-pagination__icon govuk-pagination__icon--next" />
              </>
            ),
          })}
        </div>
      )}
    </nav>
  );
};
export default Pagination;
