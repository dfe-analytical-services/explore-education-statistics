import { Form, FormFieldRadioGroup } from '@common/components/form';
import Button from '@common/components/Button';
import {
  FullTable,
  FullTableMeta,
} from '@common/modules/table-tool/types/fullTable';
import {
  appendColumnWidths,
  appendTitle,
  appendFootnotes,
  getCsvData,
} from '@common/modules/table-tool/components/utils/downloadTableUtils';
import { generateTableTitle } from '@common/modules/table-tool/components/DataTableCaption';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import React, { createElement, RefObject } from 'react';
import { utils, writeFile } from 'xlsx';

const handleCsvDownload = (fileName: string, fullTable: FullTable) => {
  const workBook = utils.book_new();
  workBook.Sheets.Sheet1 = utils.aoa_to_sheet(getCsvData(fullTable));
  workBook.SheetNames[0] = 'Sheet1';

  writeFile(workBook, `${fileName}.csv`, {
    type: 'binary',
  });
};

const handleOdsDownload = (
  fileName: string,
  subjectMeta: FullTableMeta,
  tableRef: RefObject<HTMLElement>,
) => {
  const { footnotes } = subjectMeta;

  let tableEl: HTMLTableElement | null = null;

  if (tableRef.current) {
    if (tableRef.current.tagName.toLowerCase() === 'table') {
      tableEl = tableRef.current as HTMLTableElement;
    } else {
      tableEl = tableRef.current.querySelector('table');
    }
  }

  if (!tableEl) {
    return;
  }

  const workBook = utils.table_to_book(tableEl, {
    raw: true,
  });
  const sheet = workBook.Sheets[workBook.SheetNames[0]];

  appendColumnWidths(sheet);
  appendTitle(sheet, generateTableTitle(subjectMeta));
  appendFootnotes(sheet, footnotes);

  writeFile(workBook, `${fileName}.ods`, {
    type: 'binary',
    bookType: 'ods',
  });
};

export type FileFormat = 'ods' | 'csv' | undefined;

interface FormValues {
  fileFormat: FileFormat;
}

interface Props {
  fileName: string;
  fullTable: FullTable;
  headingTag?: 'h2' | 'h3' | 'h4';
  headingSize?: 's' | 'm' | 'l';
  tableRef: RefObject<HTMLElement>;
  onSubmit?: (type: FileFormat) => void;
}

const DownloadTable = ({
  fileName,
  fullTable,
  headingTag = 'h3',
  headingSize = 's',
  tableRef,
  onSubmit,
}: Props) => {
  return (
    <Formik<FormValues>
      initialValues={{
        fileFormat: undefined,
      }}
      onSubmit={values => {
        if (onSubmit) {
          onSubmit(values.fileFormat);
        }
        return values.fileFormat === 'csv'
          ? handleCsvDownload(fileName, fullTable)
          : handleOdsDownload(fileName, fullTable.subjectMeta, tableRef);
      }}
      validationSchema={Yup.object<FormValues>({
        fileFormat: Yup.mixed().required('Choose a file format'),
      })}
    >
      {() => {
        return (
          <Form id="downloadTableForm">
            <>
              {createElement(
                headingTag,
                {
                  className: `govuk-heading-${headingSize}`,
                },
                'Download Table',
              )}
              <FormFieldRadioGroup<FormValues>
                legend="Select file format:"
                legendSize="s"
                legendWeight="regular"
                name="fileFormat"
                small
                order={[]}
                options={[
                  {
                    label:
                      'Table in ODS format (spreadsheet, with title and footnotes)',
                    value: 'ods',
                  },
                  {
                    label:
                      'Table in CSV format (flat file, with location codes)',
                    value: 'csv',
                  },
                ]}
              />
              <Button type="submit">Download table</Button>
            </>
          </Form>
        );
      }}
    </Formik>
  );
};

export default DownloadTable;
