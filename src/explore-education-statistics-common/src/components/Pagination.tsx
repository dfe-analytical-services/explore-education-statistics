/* eslint-disable react/no-array-index-key */
import { ArrowLeft, ArrowRight } from '@common/components/ArrowIcons';
import generatePageNumbers from '@common/components/util/generatePageNumbers';
import { useMobileMedia } from '@common/hooks/useMedia';
import appendQuery from '@common/utils/url/appendQuery';
import { PublicationSortOption } from '@common/services/publicationService';
import classNames from 'classnames';
import React, { ReactNode } from 'react';

const paginationLinkClassName = 'govuk-link govuk-pagination__link';

type Params = {
  newDesign?: boolean;
  page?: number;
  sortBy?: PublicationSortOption;
};

interface LinkRenderProps {
  'aria-current'?: 'page' | undefined;
  'aria-label'?: string;
  'data-testid'?: string;
  children: ReactNode;
  className: string;
  rel?: 'next' | 'prev';
  to: string;
}

export interface PaginationProps {
  baseUrl?: string;
  currentPage: number;
  label?: string;
  queryParams?: Params;
  renderLink: (props: LinkRenderProps) => ReactNode;
  totalPages: number;
}

const Pagination = ({
  baseUrl = '',
  currentPage,
  label = 'Pagination',
  queryParams,
  renderLink,
  totalPages,
}: PaginationProps) => {
  const { isMedia: isMobileMedia } = useMobileMedia();

  const pageNumbers = generatePageNumbers({
    currentPage,
    totalPages,
    windowSize: isMobileMedia ? 's' : 'm',
  });

  if (!pageNumbers.length) {
    return null;
  }

  return (
    <nav className="govuk-pagination" role="navigation" aria-label={label}>
      {currentPage !== 1 && (
        <div className="govuk-pagination__prev">
          {renderLink({
            className: paginationLinkClassName,
            rel: 'prev',
            'data-testid': 'pagination-previous',
            to: appendQuery<Params>(baseUrl, {
              ...queryParams,
              page: currentPage - 1,
            }),
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
                data-testid="pagination-ellipsis"
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
                to: appendQuery<Params>(baseUrl, {
                  ...queryParams,
                  page: pageNumber,
                }),
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
            'data-testid': 'pagination-next',
            to: appendQuery<Params>(baseUrl, {
              ...queryParams,
              page: currentPage + 1,
            }),
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
