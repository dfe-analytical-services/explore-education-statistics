import classNames from 'classnames';
import throttle from 'lodash/throttle';
import React, { forwardRef, Ref, useEffect, useRef } from 'react';
import DataTableKeys from './DataTableKeys';
import styles from './FixedHeaderGroupedDataTable.module.scss';

const dataTableCaption = 'dataTableCaption';

export interface HeaderGroup {
  label: string;
  columns: string[];
}

export interface RowGroup {
  label: string;
  rows: {
    label: string;
    columnGroups: string[][];
  }[];
}

interface Props {
  caption: string;
  headers: HeaderGroup[];
  innerRef?: Ref<HTMLElement>;
  rowGroups: RowGroup[];
}

type InnerTableProps = {
  ariaHidden?: boolean;
  className?: string;
  isStickyHeader?: boolean;
  isStickyColumn?: boolean;
} & Props;

const GroupedDataTable = forwardRef<HTMLTableElement, InnerTableProps>(
  (
    {
      ariaHidden,
      className,
      headers,
      isStickyColumn,
      isStickyHeader,
      rowGroups,
    },
    ref,
  ) => {
    return (
      <table
        aria-hidden={ariaHidden}
        aria-labelledby={dataTableCaption}
        className={classNames('govuk-table', className)}
        ref={ref}
      >
        <thead>
          <tr>
            <th
              colSpan={2}
              className={classNames(
                styles.borderRight,
                styles.intersectionCell,
              )}
            />
            {!isStickyColumn &&
              headers.map((group, groupIndex) => {
                const key = `${group.label}-${groupIndex}`;

                return (
                  <th
                    className={classNames(
                      'govuk-table__header--center',
                      styles.borderLeft,
                    )}
                    colSpan={group.columns.length || 1}
                    scope="colgroup"
                    key={key}
                  >
                    {group.label}
                  </th>
                );
              })}
          </tr>
          <tr>
            <th
              colSpan={2}
              className={classNames(styles.borderBottom, styles.borderRight)}
            />
            {!isStickyColumn &&
              headers.flatMap(group =>
                group.columns.map((column, columnIndex) => {
                  const key = `${group.label}_${column}_${columnIndex}`;

                  return (
                    <th
                      className={classNames(
                        'govuk-table__header--numeric',
                        styles.borderBottom,
                        {
                          [styles.borderLeft]: columnIndex === 0,
                        },
                      )}
                      scope="col"
                      key={key}
                    >
                      {column}
                    </th>
                  );
                }),
              )}
          </tr>
        </thead>
        {rowGroups.map((group, groupIndex) => {
          const groupKey = `${group.label}-${groupIndex}`;

          return (
            !isStickyHeader && (
              <tbody key={groupKey} className={styles.borderBottom}>
                {group.rows.map((row, rowIndex) => {
                  const rowKey = `${groupKey}_${row.label}_${rowIndex}`;

                  return (
                    <tr key={rowKey}>
                      {rowIndex === 0 && (
                        <th
                          scope="rowgroup"
                          rowSpan={group.rows.length || 1}
                          className={classNames(styles.borderBottom)}
                        >
                          {group.label}
                        </th>
                      )}
                      <th
                        scope="row"
                        className={classNames(
                          'govuk-table__cell--numeric',
                          styles.borderRight,
                        )}
                      >
                        {row.label}
                      </th>
                      {!isStickyColumn &&
                        row.columnGroups.flatMap((colGroup, colGroupIndex) =>
                          colGroup.map((column, columnIndex) => {
                            const cellKey = `${rowKey}_${colGroupIndex}_${column}_${columnIndex}`;

                            return (
                              <td
                                className={classNames(
                                  'govuk-table__cell--numeric',
                                  {
                                    [styles.borderLeft]:
                                      columnIndex === 0 && colGroupIndex > 0,
                                  },
                                )}
                                key={cellKey}
                              >
                                {column}
                              </td>
                            );
                          }),
                        )}
                    </tr>
                  );
                })}
              </tbody>
            )
          );
        })}
      </table>
    );
  },
);

GroupedDataTable.displayName = 'GroupedDataTable';

const FixedHeaderGroupedDataTable = forwardRef<HTMLElement, Props>(
  (props, ref) => {
    const { caption } = props;

    const mainTableRef = useRef<HTMLTableElement>(null);
    const headerTableRef = useRef<HTMLTableElement>(null);
    const columnTableRef = useRef<HTMLTableElement>(null);
    const intersectionTableRef = useRef<HTMLTableElement>(null);

    const setStickyElementSizes = throttle(() => {
      if (mainTableRef.current) {
        const mainTableEl = mainTableRef.current;

        const cloneCellHeightWidth = (
          selector: string,
          tableEl: HTMLTableElement,
        ) => {
          const tableCells = tableEl.querySelectorAll<HTMLTableCellElement>(
            selector,
          );

          mainTableEl
            .querySelectorAll<HTMLTableCellElement>(selector)
            .forEach((el, index) => {
              tableCells[index].style.height = `${el.offsetHeight}px`;
              tableCells[index].style.width = `${el.offsetWidth}px`;
            });
        };

        if (headerTableRef.current) {
          headerTableRef.current.style.width = `${mainTableEl.offsetWidth}px`;

          cloneCellHeightWidth('thead th', headerTableRef.current);
        }

        if (columnTableRef.current) {
          cloneCellHeightWidth(
            'thead tr th:first-child',
            columnTableRef.current,
          );
          cloneCellHeightWidth('tbody th', columnTableRef.current);
        }

        if (intersectionTableRef.current) {
          cloneCellHeightWidth(
            'thead tr th:first-child',
            intersectionTableRef.current,
          );
        }
      }
    }, 200);

    useEffect(() => {
      setTimeout(() => {
        setStickyElementSizes();

        window.addEventListener('resize', setStickyElementSizes);
      }, 100);

      return () => {
        window.removeEventListener('resize', setStickyElementSizes);
      };
    });

    return (
      <figure className={styles.figure} ref={ref}>
        <figcaption>
          <strong id={dataTableCaption}>{caption}</strong>

          <DataTableKeys />
        </figcaption>

        <div
          className={styles.container}
          role="region"
          tabIndex={-1}
          onScroll={event => {
            const { scrollLeft, scrollTop } = event.currentTarget;

            if (headerTableRef.current) {
              headerTableRef.current.style.top = `${scrollTop}px`;
            }

            if (columnTableRef.current) {
              columnTableRef.current.style.left = `${scrollLeft}px`;
            }

            if (intersectionTableRef.current) {
              intersectionTableRef.current.style.top = `${scrollTop}px`;
              intersectionTableRef.current.style.left = `${scrollLeft}px`;
            }
          }}
        >
          <GroupedDataTable
            {...props}
            className={styles.intersectionTable}
            ref={intersectionTableRef}
            ariaHidden
            isStickyColumn
            isStickyHeader
          />
          <GroupedDataTable
            {...props}
            className={styles.columnTable}
            ref={columnTableRef}
            ariaHidden
            isStickyColumn
          />
          <GroupedDataTable
            {...props}
            className={styles.headerTable}
            ref={headerTableRef}
            ariaHidden
            isStickyHeader
          />

          <GroupedDataTable ref={mainTableRef} {...props} />
        </div>
      </figure>
    );
  },
);

FixedHeaderGroupedDataTable.displayName = 'FixedHeaderGroupedDataTable';

export default FixedHeaderGroupedDataTable;
