import ButtonText from '@common/components/ButtonText';
import ChevronIcon from '@common/components/ChevronIcon';
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

        <ChevronIcon
          className={styles.icon}
          direction={sortedDirection}
          width="1.2em"
        />
      </ButtonText>
    </th>
  );
}
