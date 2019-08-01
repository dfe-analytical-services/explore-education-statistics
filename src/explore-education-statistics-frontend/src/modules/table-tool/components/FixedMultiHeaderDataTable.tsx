import React, { forwardRef, ReactNode, Ref, useRef } from 'react';
import { TableData } from '@common/services/tableBuilderService';
import Footnote from '@frontend/modules/table-tool/components/Footnote';
import styles from './FixedMultiHeaderDataTable.module.scss';
import MultiHeaderTable from './MultiHeaderTable';

interface Props {
  caption: ReactNode;
  captionId?: string;
  innerRef?: Ref<HTMLElement>;
  columnHeaders: string[][];
  rowHeaders: string[][];
  rows: string[][];
  footnotes: TableData['footnotes'];
}

const FixedMultiHeaderDataTable = forwardRef<HTMLElement, Props>(
  (props, ref) => {
    const { caption, captionId = 'dataTableCaption', footnotes } = props;

    const mainTableRef = useRef<HTMLTableElement>(null);

    return (
      <figure className={styles.figure} ref={ref}>
        <figcaption>{caption}</figcaption>

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
