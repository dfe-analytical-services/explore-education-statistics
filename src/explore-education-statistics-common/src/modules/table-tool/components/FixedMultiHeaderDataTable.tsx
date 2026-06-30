import BackToTopLink from '@common/components/BackToTopLink';
import ButtonText from '@common/components/ButtonText';
import DataSymbolsModal from '@common/components/DataSymbolsModal';
import FigureFootnotes from '@common/components/FigureFootnotes';
import Modal from '@common/components/Modal';
import VisuallyHidden from '@common/components/VisuallyHidden';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import { OmitStrict } from '@common/types';
import classNames from 'classnames';
import { ReactNode, Ref, useEffect, useRef } from 'react';
import styles from './FixedMultiHeaderDataTable.module.scss';
import MultiHeaderTable, { MultiHeaderTableProps } from './MultiHeaderTable';

const mobileWidth = 1024;

interface Props
  extends OmitStrict<MultiHeaderTableProps, 'ariaLabelledBy' | 'ref'> {
  capMaxHeight?: boolean;
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
  const { ref, ...tableProps } = props;

  const {
    capMaxHeight,
    caption,
    captionId,
    captionTitle,
    footnotes = [],
    footnotesClassName,
    footnotesHeadingTag,
    footnotesId,
    source,
    footnotesHeadingHiddenText,
    tableHeadersForm,
    tableJson,
  } = tableProps;

  const containerRef = useRef<HTMLDivElement>(null);
  const mainTableRef = useRef<HTMLTableElement>(null);
  const fullScreenTableRef = useRef<HTMLTableElement>(null);

  useEffect(() => {
    const listener = () => {
      const removeTransform = (element: HTMLTableCellElement) => {
        // eslint-disable-next-line no-param-reassign
        element.style.transform = '';
      };

      if (
        containerRef.current &&
        mainTableRef.current &&
        window.innerWidth <= mobileWidth
      ) {
        mainTableRef.current
          .querySelectorAll<HTMLTableCellElement>('thead td')
          .forEach(removeTransform);

        mainTableRef.current
          .querySelectorAll<HTMLTableCellElement>('thead th')
          .forEach(removeTransform);

        mainTableRef.current
          .querySelectorAll<HTMLTableCellElement>('tbody th')
          .forEach(removeTransform);
      }

      if (
        containerRef.current &&
        fullScreenTableRef.current &&
        window.innerWidth <= mobileWidth
      ) {
        fullScreenTableRef.current
          .querySelectorAll<HTMLTableCellElement>('thead td')
          .forEach(removeTransform);

        fullScreenTableRef.current
          .querySelectorAll<HTMLTableCellElement>('thead th')
          .forEach(removeTransform);

        fullScreenTableRef.current
          .querySelectorAll<HTMLTableCellElement>('tbody th')
          .forEach(removeTransform);
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
        className={classNames(
          styles.container,
          capMaxHeight && styles.capMaxHeight,
        )}
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

          if (fullScreenTableRef.current && window.innerWidth >= mobileWidth) {
            fullScreenTableRef.current
              .querySelectorAll<HTMLTableCellElement>('thead td')
              .forEach(el => {
                // eslint-disable-next-line no-param-reassign
                el.style.transform = `translate(${scrollLeft}px, ${scrollTop}px)`;
              });

            fullScreenTableRef.current
              .querySelectorAll<HTMLTableCellElement>('thead th')
              .forEach(el => {
                // eslint-disable-next-line no-param-reassign
                el.style.transform = `translate(0, ${scrollTop}px)`;
              });

            fullScreenTableRef.current
              .querySelectorAll<HTMLTableCellElement>('tbody th')
              .forEach(el => {
                // eslint-disable-next-line no-param-reassign
                el.style.transform = `translate(${scrollLeft}px, 0)`;
              });
          }
        }}
      >
        <MultiHeaderTable
          {...tableProps}
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
          ref={fullScreenTableRef}
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
