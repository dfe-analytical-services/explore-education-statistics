import ButtonText from '@common/components/ButtonText';
import styles from '@common/components/SortedTableHeader.module.scss';
import classNames from 'classnames';
import React from 'react';

export type TableSortDirection = 'ascending' | 'descending';

export interface TableSort<TColumn extends string = string> {
  column: TColumn;
  direction: TableSortDirection;
}

interface Props<TColumn extends string> {
  className?: string;
  column: TColumn;
  label: string;
  sort: TableSort<TColumn>;
  onClick: (column: TColumn) => void;
}

/**
 * A table header cell with a control for sorting the table by its column.
 *
 * Shows a single chevron for the direction the table is currently sorted in,
 * or both chevrons when the table is sorted by another column.
 */
export default function SortedTableHeader<TColumn extends string>({
  className,
  column,
  label,
  sort,
  onClick,
}: Props<TColumn>) {
  const sortedDirection = sort.column === column ? sort.direction : undefined;

  return (
    <th aria-sort={sortedDirection ?? 'none'} className={className} scope="col">
      <ButtonText
        className={classNames(styles.button, 'govuk-!-font-weight-bold')}
        underline={false}
        onClick={() => onClick(column)}
      >
        {label}

        <svg
          aria-hidden
          className={styles.icon}
          focusable="false"
          viewBox="0 0 22 22"
          width="1.2em"
          fill="currentColor"
        >
          {sortedDirection === 'descending' && (
            <path d="M15.4375 7L11 15.8687L6.5625 7L15.4375 7Z" />
          )}
          {sortedDirection === 'ascending' && (
            <path d="M6.5625 15L11 6.1313L15.4375 15L6.5625 15Z" />
          )}
          {!sortedDirection && (
            <>
              <path d="M8.1875 9.5L10.9609 3.95703L13.7344 9.5H8.1875Z" />
              <path d="M13.7344 12.0781L10.9609 17.6211L8.1875 12.0781H13.7344Z" />
            </>
          )}
        </svg>
      </ButtonText>
    </th>
  );
}
