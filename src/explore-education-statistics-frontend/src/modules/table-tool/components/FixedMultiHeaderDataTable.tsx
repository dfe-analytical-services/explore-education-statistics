import React, { forwardRef, Ref, useRef } from 'react';
import DataTableKeys from './DataTableKeys';
import styles from './FixedMultiHeaderDataTable.module.scss';
import MultiHeaderTable from './MultiHeaderTable';

interface Props {
  caption: string;
  captionId?: string;
  innerRef?: Ref<HTMLElement>;
  columnHeaders: string[][];
  rowHeaders: string[][];
  rows: string[][];
}

const FixedMultiHeaderDataTable = forwardRef<HTMLElement, Props>(
  (props, ref) => {
    const { caption, captionId = 'dataTableCaption' } = props;

    const mainTableRef = useRef<HTMLTableElement>(null);

    return (
      <figure className={styles.figure} ref={ref}>
        <figcaption>
          <strong id={captionId}>{caption}</strong>

          <DataTableKeys />
        </figcaption>

        <div
          className={styles.container}
          role="region"
          tabIndex={-1}
          onScroll={event => {
            const { scrollLeft, scrollTop } = event.currentTarget;

            if (mainTableRef.current) {
              mainTableRef.current
                .querySelectorAll<HTMLTableHeaderCellElement>('thead td')
                .forEach(el => {
                  // eslint-disable-next-line no-param-reassign
                  el.style.transform = `translate(${scrollLeft}px, ${scrollTop}px)`;
                });

              mainTableRef.current
                .querySelectorAll<HTMLTableHeaderCellElement>('thead th')
                .forEach(el => {
                  // eslint-disable-next-line no-param-reassign
                  el.style.transform = `translate(0, ${scrollTop}px)`;
                });

              mainTableRef.current
                .querySelectorAll<HTMLTableHeaderCellElement>('tbody th')
                .forEach(el => {
                  // eslint-disable-next-line no-param-reassign
                  el.style.transform = `translate(${scrollLeft}px, 0)`;
                });
            }
          }}
        >
          <MultiHeaderTable
            {...props}
            ariaLabelledBy={captionId}
            ref={mainTableRef}
          />
        </div>
      </figure>
    );
  },
);

FixedMultiHeaderDataTable.displayName = 'FixedHeaderGroupedDataTable';

export default FixedMultiHeaderDataTable;
