import FormProvider from '@common/components/form/rhf/FormProvider';
import RHFForm from '@common/components/form/rhf/RHFForm';
import RHFFormFieldRadioGroup from '@common/components/form/rhf/RHFFormFieldRadioGroup';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import downloadTableOdsFile from '@common/modules/table-tool/components/utils/downloadTableOdsFile';
import generateTableTitle from '@common/modules/table-tool/utils/generateTableTitle';
import { Footnote } from '@common/services/types/footnotes';
import downloadFile from '@common/utils/file/downloadFile';
import Yup from '@common/validation/yup';
import LoadingSpinner from '@common/components/LoadingSpinner';
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
  tableRef: RefObject<HTMLElement>;
  tableTitle?: string;
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
  onCsvDownload,
  onSubmit,
}: Props) => {
  const tableFootnotes = fullTable
    ? fullTable.subjectMeta.footnotes
    : footnotes;
  const handleCsvDownload = async () => {
    const csv = await onCsvDownload();
    downloadFile(csv, fileName);
  };

  const handleOdsDownload = () => {
    const title = fullTable
      ? generateTableTitle(fullTable.subjectMeta)
      : tableTitle;

    downloadTableOdsFile(fileName, tableFootnotes, tableRef, title);
  };

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
          <RHFForm
            id="downloadTableForm"
            onSubmit={async ({ fileFormat }) => {
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
              <RHFFormFieldRadioGroup<FormValues>
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
              <ButtonGroup>
                <Button type="submit" disabled={formState.isSubmitting}>
                  Download table
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
          </RHFForm>
        );
      }}
    </FormProvider>
  );
};

export default DownloadTable;
