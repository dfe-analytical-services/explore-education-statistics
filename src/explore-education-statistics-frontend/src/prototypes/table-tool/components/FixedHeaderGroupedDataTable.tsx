import classNames from 'classnames';
import throttle from 'lodash/throttle';
import React, { Component, createRef, forwardRef } from 'react';
import styles from 'src/prototypes/table-tool/components/FixedHeaderGroupedDataTable.module.scss';

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
      caption,
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
        className={classNames('govuk-table', className)}
        ref={ref}
        aria-hidden={ariaHidden}
      >
        <caption>{caption}</caption>

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
              headers.map((group, groupIndex) => (
                <th
                  className={classNames(
                    'govuk-table__header--center',
                    styles.borderLeft,
                  )}
                  colSpan={group.columns.length || 1}
                  scope="colgroup"
                  key={`${group.label}-${groupIndex}`}
                >
                  {group.label}
                </th>
              ))}
          </tr>
          <tr>
            <th
              colSpan={2}
              className={classNames(styles.borderBottom, styles.borderRight)}
            />
            {!isStickyColumn &&
              headers.flatMap(group =>
                group.columns.map((column, columnIndex) => (
                  <th
                    className={classNames(
                      'govuk-table__header--numeric',
                      styles.borderBottom,
                      {
                        [styles.borderLeft]: columnIndex === 0,
                      },
                    )}
                    scope="col"
                    key={`${group.label}-${column}-${columnIndex}`}
                  >
                    {column}
                  </th>
                )),
              )}
          </tr>
        </thead>
        {rowGroups.map((group, groupIndex) => {
          const groupKey = `${group.label}-${groupIndex}`;

          return (
            !isStickyHeader && (
              <tbody key={groupKey} className={styles.borderBottom}>
                {group.rows.map((row, rowIndex) => {
                  const rowKey = `${groupKey}-${row.label}-${rowIndex}`;

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
                            return (
                              <td
                                className={classNames(
                                  'govuk-table__cell--numeric',
                                  {
                                    [styles.borderLeft]:
                                      columnIndex === 0 && colGroupIndex > 0,
                                  },
                                )}
                                key={`${rowKey}-${colGroupIndex}-${column}-${columnIndex}`}
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

class FixedHeaderGroupedDataTable extends Component<Props> {
  private containerRef = createRef<HTMLDivElement>();
  private mainTableRef = createRef<HTMLTableElement>();
  private headerTableRef = createRef<HTMLTableElement>();
  private columnTableRef = createRef<HTMLTableElement>();
  private intersectionTableRef = createRef<HTMLTableElement>();

  public componentDidMount() {
    setTimeout(() => {
      this.setStickyElementSizes();

      window.addEventListener('resize', this.setStickyElementSizes);
    });
  }

  public componentDidUpdate(): void {
    setTimeout(() => {
      this.setStickyElementSizes();
    });
  }

  public componentWillUnmount(): void {
    window.removeEventListener('resize', this.setStickyElementSizes);
  }

  private setStickyElementSizes = throttle(() => {
    if (this.mainTableRef.current) {
      const mainTableEl = this.mainTableRef.current;

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

      if (this.headerTableRef.current) {
        this.headerTableRef.current.style.width = `${
          mainTableEl.offsetWidth
        }px`;

        cloneCellHeightWidth('thead th', this.headerTableRef.current);
      }

      if (this.columnTableRef.current) {
        cloneCellHeightWidth(
          'thead tr th:first-child',
          this.columnTableRef.current,
        );
        cloneCellHeightWidth('tbody th', this.columnTableRef.current);
      }

      if (this.intersectionTableRef.current) {
        cloneCellHeightWidth(
          'thead tr th:first-child',
          this.intersectionTableRef.current,
        );
      }
    }
  }, 200);

  public render() {
    return (
      <div
        className={styles.container}
        ref={this.containerRef}
        role="region"
        tabIndex={0}
        onScroll={event => {
          const { scrollLeft, scrollTop } = event.currentTarget;

          if (this.headerTableRef.current) {
            this.headerTableRef.current.style.top = `${scrollTop}px`;
          }

          if (this.columnTableRef.current) {
            this.columnTableRef.current.style.left = `${scrollLeft}px`;
          }

          if (this.intersectionTableRef.current) {
            this.intersectionTableRef.current.style.top = `${scrollTop}px`;
            this.intersectionTableRef.current.style.left = `${scrollLeft}px`;
          }
        }}
      >
        <GroupedDataTable
          {...this.props}
          className={styles.stickyIntersectionTable}
          ref={this.intersectionTableRef}
          ariaHidden
          isStickyColumn
          isStickyHeader
        />

        <GroupedDataTable
          {...this.props}
          className={styles.stickyColumnTable}
          ref={this.columnTableRef}
          ariaHidden
          isStickyColumn
        />
        <GroupedDataTable
          {...this.props}
          className={styles.stickyHeaderTable}
          ref={this.headerTableRef}
          ariaHidden
          isStickyHeader
        />
        <GroupedDataTable ref={this.mainTableRef} {...this.props} />
      </div>
    );
  }
}

export default FixedHeaderGroupedDataTable;
