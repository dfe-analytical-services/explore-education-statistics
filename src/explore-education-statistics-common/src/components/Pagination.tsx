import { ArrowLeft, ArrowRight } from '@common/components/ArrowIcons';
import generatePageNumbers from '@common/components/util/generatePageNumbers';
import { useDesktopMedia } from '@common/hooks/useMedia';
import classNames from 'classnames';
import React, { ReactNode } from 'react';

interface Props {
  currentPage: number;
  nextPrevLinkRenderer: ({
    children,
    className,
    pageNumber,
    rel,
  }: {
    children: ReactNode;
    className: string;
    pageNumber: number;
    rel: 'next' | 'prev';
  }) => ReactNode;
  pageLinkRenderer: ({
    ariaCurrent,
    ariaLabel,
    className,
    pageNumber,
  }: {
    ariaCurrent?: 'page' | undefined;
    ariaLabel?: string;
    className: string;
    pageNumber: number;
  }) => ReactNode;
  totalPages: number;
}

const Pagination = ({
  currentPage,
  nextPrevLinkRenderer,
  pageLinkRenderer,
  totalPages,
}: Props) => {
  const { isMedia: isDesktopMedia } = useDesktopMedia();
  const pageNumbers = generatePageNumbers({
    currentPage,
    totalPages,
    offset: isDesktopMedia ? 2 : 1,
  });

  if (pageNumbers.length <= 1) {
    return null;
  }

  return (
    <nav
      className="govuk-pagination"
      role="navigation"
      aria-label="Pagination navigation"
    >
      {currentPage !== 1 && (
        <div className="govuk-pagination__prev">
          {nextPrevLinkRenderer({
            className: 'govuk-link govuk-pagination__link',
            pageNumber: currentPage - 1,
            rel: 'prev',
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
          const key = `paginationKey-${index}`;
          if (pageNumber === null) {
            return (
              <li
                key={key}
                className="govuk-pagination__item govuk-pagination__item--ellipses"
              >
                …
              </li>
            );
          }
          return (
            <li
              key={key}
              className={classNames('govuk-pagination__item', {
                'govuk-pagination__item--current': pageNumber === currentPage,
              })}
            >
              {pageLinkRenderer({
                ariaCurrent: currentPage === pageNumber ? 'page' : undefined,
                ariaLabel: `Page ${pageNumber}`,
                className: 'govuk-link govuk-pagination__link',
                pageNumber,
              })}
            </li>
          );
        })}
      </ul>

      {currentPage !== totalPages && (
        <div className="govuk-pagination__next">
          {nextPrevLinkRenderer({
            className: 'govuk-link govuk-pagination__link',
            pageNumber: currentPage + 1,
            rel: 'next',
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
