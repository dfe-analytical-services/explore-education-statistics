import FigureFootnotes from '@common/components/FigureFootnotes';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import { OmitStrict } from '@common/types';
import React, { ReactNode, Ref, useEffect, useRef } from 'react';
import DataSymbolsModal from '@common/components/DataSymbolsModal';
import ButtonText from '@common/components/ButtonText';
import Modal from '@common/components/Modal';
import VisuallyHidden from '@common/components/VisuallyHidden';
import BackToTopLink from '@common/components/BackToTopLink';
import styles from './FixedMultiHeaderDataTable.module.scss';
import MultiHeaderTable, { MultiHeaderTableProps } from './MultiHeaderTable';

const mobileWidth = 1024;

interface Props
  extends OmitStrict<MultiHeaderTableProps, 'ariaLabelledBy' | 'ref'> {
  caption: ReactNode;
  captionId: string;
  captionTitle?: string;
  innerRef?: Ref<HTMLElement>;
  footnotes?: FullTableMeta['footnotes'];
  footnotesClassName?: string;
  footnotesHeadingTag?: 'h2' | 'h3';
  footnotesId: string;
  ref?: Ref<HTMLElement>;
  source?: string;
  footnotesHeadingHiddenText?: string;
  tableHeadersForm?: ReactNode;
}

const FixedMultiHeaderDataTable = (props: Props) => {
  const {
    caption,
    captionId,
    captionTitle,
    footnotes = [],
    footnotesClassName,
    footnotesHeadingTag,
    footnotesId,
    ref,
    source,
    footnotesHeadingHiddenText,
    tableHeadersForm,
    tableJson,
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

      {tableHeadersForm}

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
      <Modal
        className={styles.fullScreen}
        closeText="Close full screen table"
        fullScreen
        showClose
        title={captionTitle ?? ''}
        titleId={`modal-${captionId}`}
        triggerButton={
          <ButtonText className="govuk-!-margin-bottom-2">
            Show full screen table
            <VisuallyHidden>{`for ${captionTitle}`}</VisuallyHidden>
          </ButtonText>
        }
      >
        <MultiHeaderTable
          {...props}
          ariaLabelledBy={`modal-${captionId}`}
          ref={mainTableRef}
        />
      </Modal>
      <p>
        <DataSymbolsModal />
      </p>
      {tableJson.tbody.length > 10 && (
        <BackToTopLink className="govuk-!-margin-top-4 govuk-!-margin-bottom-4" />
      )}
      <div className={footnotesClassName}>
        <FigureFootnotes
          footnotes={footnotes}
          headingHiddenText={footnotesHeadingHiddenText}
          headingTag={footnotesHeadingTag}
          id={footnotesId}
        />
      </div>

      {source && <p className="govuk-body-s">Source: {source}</p>}
    </figure>
  );
};

FixedMultiHeaderDataTable.displayName = 'FixedMultiHeaderDataTable';

export default FixedMultiHeaderDataTable;
