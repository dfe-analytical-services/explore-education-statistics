import FormProvider from '@common/components/form/FormProvider';
import Form from '@common/components/form/Form';
import FormFieldRadioGroup from '@common/components/form/FormFieldRadioGroup';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import downloadTableOdsFile from '@common/modules/table-tool/components/utils/downloadTableOdsFile';
import generateTableTitle from '@common/modules/table-tool/utils/generateTableTitle';
import { Footnote } from '@common/services/types/footnotes';
import downloadFile from '@common/utils/file/downloadFile';
import Yup from '@common/validation/yup';
import LoadingSpinner from '@common/components/LoadingSpinner';
import copyElementToClipboard from '@common/utils/copyElementToClipboard';
import React, { createElement, RefObject } from 'react';

export type FileFormat = 'ods' | 'csv';

interface FormValues {
  fileFormat: FileFormat;
}

interface Props {
  fileName: string;
  footnotes?: Footnote[];
  fullTable?: FullTable;
  headingTag?: 'h2' | 'h3' | 'h4';
  headingSize?: 's' | 'm' | 'l';
  tableRef: RefObject<HTMLElement | null>;
  tableTitle?: string;
  hideOdsDownload?: boolean;
  onCsvDownload: () => Blob | Promise<Blob>;
  onSubmit?: (type: FileFormat) => void;
}

const DownloadTable = ({
  fileName,
  footnotes = [],
  fullTable,
  headingTag = 'h3',
  headingSize = 's',
  tableRef,
  tableTitle = '',
  hideOdsDownload = false,
  onCsvDownload,
  onSubmit,
}: Props) => {
  const tableFootnotes = fullTable
    ? fullTable.subjectMeta.footnotes
    : footnotes;
  const handleCsvDownload = async () => {
    const csv = await onCsvDownload();
    downloadFile({ file: csv, fileName });
  };

  const handleOdsDownload = () => {
    const title = fullTable
      ? generateTableTitle(fullTable.subjectMeta)
      : tableTitle;

    downloadTableOdsFile(fileName, tableFootnotes, tableRef, title);
  };

  const options = [
    {
      label: 'Table in ODS format (spreadsheet, with title and footnotes)',
      value: 'ods',
    },
    {
      label: 'Table in CSV format (flat file, with location codes)',
      value: 'csv',
    },
  ];

  return (
    <FormProvider
      initialValues={{
        fileFormat: undefined as never,
      }}
      validationSchema={Yup.object<FormValues>({
        fileFormat: Yup.string()
          .oneOf<FileFormat>(['ods', 'csv'])
          .required('Choose a file format'),
      })}
    >
      {({ formState }) => {
        return (
          <Form
            id="downloadTableForm"
            onSubmit={async ({ fileFormat }) => {
              // TODO EES-5852 analytics for table tool/permalink csv/ods downloads
              await onSubmit?.(fileFormat);

              if (fileFormat === 'csv') {
                await handleCsvDownload();
              } else {
                await handleOdsDownload();
              }
            }}
          >
            <>
              {createElement(
                headingTag,
                {
                  className: `govuk-heading-${headingSize}`,
                },
                'Download Table',
              )}
              <FormFieldRadioGroup<FormValues>
                legend="Select a file format to download:"
                legendSize="s"
                legendWeight="regular"
                name="fileFormat"
                small
                order={[]}
                options={
                  hideOdsDownload
                    ? options.filter(o => o.value !== 'ods')
                    : options
                }
              />
              <ButtonGroup>
                <Button type="submit" disabled={formState.isSubmitting}>
                  Download table
                </Button>
                <Button
                  variant="secondary"
                  onClick={() => copyElementToClipboard(tableRef)}
                >
                  Copy table to clipboard
                </Button>
                <LoadingSpinner
                  alert
                  className="govuk-!-margin-left-2"
                  inline
                  hideText
                  loading={formState.isSubmitting}
                  size="md"
                  text="Preparing download"
                />
              </ButtonGroup>
            </>
          </Form>
        );
      }}
    </FormProvider>
  );
};

export default DownloadTable;
