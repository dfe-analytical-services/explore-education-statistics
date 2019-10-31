import { FullTableMeta } from '@common/modules/full-table/types/fullTable';
import Footnote from '@common/modules/table-tool/components/Footnote';
import React, { forwardRef, ReactNode, Ref, useEffect, useRef } from 'react';
import styles from './FixedMultiHeaderDataTable.module.scss';
import MultiHeaderTable, { RowHeaderType } from './MultiHeaderTable';

const mobileWidth = 1024;

interface Props {
  caption: ReactNode;
  captionId?: string;
  innerRef?: Ref<HTMLElement>;
  columnHeaders: string[][];
  rowHeaders: RowHeaderType[][];
  rowHeaderIsGroup?: boolean[];
  rows: string[][];
  footnotes?: FullTableMeta['footnotes'];
}

const FixedMultiHeaderDataTable = forwardRef<HTMLElement, Props>(
  (props, ref) => {
    const { caption, captionId = 'dataTableCaption', footnotes = [] } = props;

    const containerRef = useRef<HTMLDivElement>(null);
    const mainTableRef = useRef<HTMLTableElement>(null);

    useEffect(() => {
      const listener = () => {
        if (
          containerRef.current &&
          mainTableRef.current &&
          window.innerWidth <= mobileWidth
        ) {
          mainTableRef.current
            .querySelectorAll<HTMLTableHeaderCellElement>('thead td')
            .forEach(el => {
              // eslint-disable-next-line no-param-reassign
              el.style.transform = '';
            });

          mainTableRef.current
            .querySelectorAll<HTMLTableHeaderCellElement>('thead th')
            .forEach(el => {
              // eslint-disable-next-line no-param-reassign
              el.style.transform = '';
            });

          mainTableRef.current
            .querySelectorAll<HTMLTableHeaderCellElement>('tbody th')
            .forEach(el => {
              // eslint-disable-next-line no-param-reassign
              el.style.transform = '';
            });
        }
      };

      window.addEventListener('resize', listener);

      return () => window.removeEventListener('resize', listener);
    }, []);

    return (
      <figure className={styles.figure} ref={ref}>
        <figcaption>{caption}</figcaption>

        <div
          className={styles.container}
          ref={containerRef}
          role="region"
          tabIndex={-1}
          onScroll={event => {
            const { scrollLeft, scrollTop } = event.currentTarget;

            if (mainTableRef.current && window.innerWidth >= mobileWidth) {
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
        {footnotes.length > 0 && (
          <>
            <h3>Footnotes</h3>
            <Footnote content={footnotes} />
          </>
        )}
      </figure>
    );
  },
);

FixedMultiHeaderDataTable.displayName = 'FixedMultiHeaderDataTable';

export default FixedMultiHeaderDataTable;
