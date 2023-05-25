import FigureFootnotes from '@common/components/FigureFootnotes';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import { OmitStrict } from '@common/types';
import React, { forwardRef, ReactNode, Ref, useEffect, useRef } from 'react';
import styles from './FixedMultiHeaderDataTable.module.scss';
import multiHeaderStyles from './MultiHeaderTable.module.scss';
import MultiHeaderTable, { MultiHeaderTableProps } from './MultiHeaderTable';

const mobileWidth = 1024;

interface Props extends OmitStrict<MultiHeaderTableProps, 'ariaLabelledBy'> {
  caption: ReactNode;
  captionId: string;
  innerRef?: Ref<HTMLElement>;
  footnotes?: FullTableMeta['footnotes'];
  footnotesClassName?: string;
  footnotesId: string;
  source?: string;
  footnotesHeadingHiddenText?: string;
}

const FixedMultiHeaderDataTable = forwardRef<HTMLElement, Props>(
  (props, ref) => {
    const {
      caption,
      captionId,
      footnotes = [],
      footnotesClassName,
      footnotesId,
      source,
      footnotesHeadingHiddenText,
    } = props;

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
            .querySelectorAll<HTMLTableCellElement>('thead td')
            .forEach(el => {
              // eslint-disable-next-line no-param-reassign
              el.style.transform = '';
            });

          mainTableRef.current
            .querySelectorAll<HTMLTableCellElement>('thead th')
            .forEach(el => {
              // eslint-disable-next-line no-param-reassign
              el.style.transform = '';
            });

          mainTableRef.current
            .querySelectorAll<HTMLTableCellElement>('tbody th')
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
          // eslint-disable-next-line jsx-a11y/no-noninteractive-tabindex
          tabIndex={0}
          className={styles.container}
          ref={containerRef}
          role="region"
          onScroll={event => {
            const { scrollLeft, scrollTop } = event.currentTarget;

            if (mainTableRef.current && window.innerWidth >= mobileWidth) {
              mainTableRef.current
                .querySelectorAll<HTMLTableCellElement>('thead td')
                .forEach(el => {
                  // eslint-disable-next-line no-param-reassign
                  el.style.transform = `translate(${scrollLeft}px, ${scrollTop}px)`;
                });

              mainTableRef.current
                .querySelectorAll<HTMLTableCellElement>('thead th')
                .forEach(el => {
                  // eslint-disable-next-line no-param-reassign
                  el.style.transform = `translate(0, ${scrollTop}px)`;
                });

              mainTableRef.current
                .querySelectorAll<HTMLTableCellElement>('tbody th')
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

        <div className={footnotesClassName}>
          <FigureFootnotes
            footnotes={footnotes}
            headingHiddenText={footnotesHeadingHiddenText}
            id={footnotesId}
          />
        </div>

        {source && <p className="govuk-body-s">Source: {source}</p>}
      </figure>
    );
  },
);

FixedMultiHeaderDataTable.displayName = 'FixedMultiHeaderDataTable';

export default FixedMultiHeaderDataTable;
