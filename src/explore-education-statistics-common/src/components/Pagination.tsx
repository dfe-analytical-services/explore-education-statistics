import { ArrowLeft, ArrowRight } from '@common/components/ArrowIcons';
import generatePageNumbers from '@common/components/util/generatePageNumbers';
import styles from '@common/components/Pagination.module.scss';
import { useMobileMedia } from '@common/hooks/useMedia';
import appendQuery from '@common/utils/url/appendQuery';
import classNames from 'classnames';
import { ParsedUrlQueryInput } from 'querystring';
import React, { MouseEvent, ReactNode } from 'react';
import VisuallyHidden from './VisuallyHidden';

const paginationLinkClassName = 'govuk-pagination__link';

interface LinkRenderProps {
  'aria-current'?: 'page';
  'aria-label'?: string;
  'data-testid'?: string;
  children: ReactNode;
  className: string;
  rel?: 'next' | 'prev';
  to: string;
  onClick?: (event: MouseEvent<HTMLAnchorElement | HTMLButtonElement>) => void;
}

export interface PaginationProps {
  baseUrl?: string;
  currentPage: number;
  label?: string;
  queryParams?: ParsedUrlQueryInput;
  pageParam?: string;
  renderLink: (props: LinkRenderProps) => ReactNode;
  totalPages: number;
  onClick?: (pageNumber: number) => void;
}

const Pagination = ({
  baseUrl = '',
  currentPage,
  label = 'Pagination',
  pageParam = 'page',
  queryParams,
  renderLink,
  totalPages,
  onClick,
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
    <nav
      className={`govuk-pagination ${styles.pagination}`}
      role="navigation"
      aria-label={label}
    >
      {currentPage !== 1 && (
        <div className="govuk-pagination__prev">
          {renderLink({
            className: paginationLinkClassName,
            rel: 'prev',
            'data-testid': 'pagination-previous',
            to: appendQuery(baseUrl, {
              ...queryParams,
              [pageParam]: currentPage - 1,
            }),
            children: (
              <>
                <ArrowLeft className="govuk-pagination__icon govuk-pagination__icon--prev" />
                <span className="govuk-pagination__link-title">
                  Previous<VisuallyHidden> page</VisuallyHidden>
                </span>
              </>
            ),
            onClick: () => onClick?.(currentPage - 1),
          })}
        </div>
      )}

      <ul className="govuk-pagination__list">
        {pageNumbers.map((pageNumber, index) => {
          if (pageNumber === null) {
            return (
              <li
                // eslint-disable-next-line react/no-array-index-key
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
              // eslint-disable-next-line react/no-array-index-key
              key={index.toString()}
              className={classNames('govuk-pagination__item', {
                'govuk-pagination__item--current': pageNumber === currentPage,
              })}
            >
              {renderLink({
                'aria-current': currentPage === pageNumber ? 'page' : undefined,
                'aria-label': `Page ${pageNumber}`,
                className: paginationLinkClassName,
                to: appendQuery(baseUrl, {
                  ...queryParams,
                  [pageParam]: pageNumber,
                }),
                children: <>{pageNumber}</>,
                onClick: () => onClick?.(pageNumber),
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
            to: appendQuery(baseUrl, {
              ...queryParams,
              [pageParam]: currentPage + 1,
            }),
            children: (
              <>
                <span className="govuk-pagination__link-title">
                  Next<VisuallyHidden> page</VisuallyHidden>
                </span>
                <ArrowRight className="govuk-pagination__icon govuk-pagination__icon--next" />
              </>
            ),
            onClick: () => onClick?.(currentPage + 1),
          })}
        </div>
      )}
    </nav>
  );
};
export default Pagination;
