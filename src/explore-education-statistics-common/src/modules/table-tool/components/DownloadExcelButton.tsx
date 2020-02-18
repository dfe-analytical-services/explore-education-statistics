import ButtonText from '@common/components/ButtonText';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import { Dictionary } from '@common/types';
import sum from 'lodash/sum';
import React, { RefObject } from 'react';
import { CellObject, utils, WorkSheet, writeFile } from 'xlsx';

interface Props {
  publicationSlug: string;
  footnotes: FullTableMeta['footnotes'];
  tableRef: RefObject<HTMLElement>;
}

/**
 * Explicitly set column widths for a {@param sheet} using
 * the width of the cell with the most content in it.
 * SheetJs doesn't automatically generate our column widths
 * so the spreadsheet would look quite squished without this.
 */
export function appendColumnWidths(sheet: WorkSheet): WorkSheet {
  const columnWidths = Object.entries(sheet)
    .filter(([key]) => !key.startsWith('!'))
    .reduce<Dictionary<number>>((acc, [key, value]: [string, CellObject]) => {
      const [column] = key.split(/[0-9]+/);

      if (typeof acc[column] === 'undefined') {
        acc[column] = 0;
      }

      let length = 0;

      if (typeof value.v === 'string') {
        // eslint-disable-next-line prefer-destructuring
        length = value.v.length;
      } else if (typeof value.v === 'number') {
        // eslint-disable-next-line prefer-destructuring
        length = value.v.toString().length;
      }

      if (length > acc[column]) {
        acc[column] = length;
      }

      return acc;
    }, {});

  // eslint-disable-next-line no-param-reassign
  sheet['!cols'] = Object.entries(columnWidths)
    .map(([key, value]) => {
      // Convert column letters to a single number so they
      // can be sorted in the way that Excel expects
      const keyHash = sum(key.split('').map(char => char.charCodeAt(0)));

      return [keyHash, value];
    })
    .sort(([a], [b]) => a - b)
    .map(([, value]) => {
      return value ? { wch: value } : {};
    });

  return sheet;
}

/**
 * Append {@param footnotes} to the bottom of a {@param sheet}.
 */
export function appendFootnotes(
  sheet: WorkSheet,
  footnotes: FullTableMeta['footnotes'],
): WorkSheet {
  const [, lastCell] = sheet['!ref']?.split(':') ?? [];

  if (!lastCell) {
    return sheet;
  }

  const [lastColumn, lastRow] = lastCell.split(/([0-9]+)/);
  const footnoteStartRow = parseInt(lastRow, 10) + 2;

  footnotes.forEach((footnote, index) => {
    // eslint-disable-next-line no-param-reassign
    sheet[`A${footnoteStartRow + index}`] = {
      t: 's',
      v: `(${index + 1}) ${footnote.label}`,
    } as CellObject;
  });

  // eslint-disable-next-line no-param-reassign
  sheet['!ref'] = `A1:${lastColumn}${footnoteStartRow + footnotes.length - 1}`;

  return sheet;
}

const DownloadExcelButton = ({
  publicationSlug,
  footnotes,
  tableRef,
}: Props) => {
  // Try to find table element within the ref
  let tableEl: HTMLTableElement | null = null;

  if (tableRef.current) {
    if (tableRef.current.tagName.toLowerCase() === 'table') {
      tableEl = tableRef.current as HTMLTableElement;
    } else {
      tableEl = tableRef.current.querySelector('table');
    }

    // Don't render anything if no table could be found
    if (!tableEl) {
      return null;
    }
  }

  return (
    <ButtonText
      onClick={() => {
        const workBook = utils.table_to_book(tableEl, {
          raw: true,
        });
        const sheet = workBook.Sheets[workBook.SheetNames[0]];

        appendColumnWidths(sheet);
        appendFootnotes(sheet, footnotes);

        writeFile(workBook, `data-${publicationSlug}.xlsx`, {
          type: 'binary',
        });
      }}
    >
      Download table as Excel spreadsheet (XLSX)
    </ButtonText>
  );
};

export default DownloadExcelButton;
