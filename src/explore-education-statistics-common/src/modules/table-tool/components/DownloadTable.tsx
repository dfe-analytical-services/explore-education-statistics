import { Form, FormFieldRadioGroup } from '@common/components/form';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import downloadTableOdsFile from '@common/modules/table-tool/components/utils/downloadTableOdsFile';
import generateTableTitle from '@common/modules/table-tool/utils/generateTableTitle';
import downloadFile from '@common/utils/file/downloadFile';
import Yup from '@common/validation/yup';
import LoadingSpinner from '@common/components/LoadingSpinner';
import { Formik } from 'formik';
import React, { createElement, RefObject } from 'react';

export type FileFormat = 'ods' | 'csv';

interface FormValues {
  fileFormat: FileFormat;
}

interface Props {
  fileName: string;
  fullTable: FullTable;
  headingTag?: 'h2' | 'h3' | 'h4';
  headingSize?: 's' | 'm' | 'l';
  tableRef: RefObject<HTMLElement>;
  onCsvDownload: () => Blob | Promise<Blob>;
  onSubmit?: (type: FileFormat) => void;
}

const DownloadTable = ({
  fileName,
  fullTable,
  headingTag = 'h3',
  headingSize = 's',
  tableRef,
  onCsvDownload,
  onSubmit,
}: Props) => {
  const handleCsvDownload = async () => {
    const csv = await onCsvDownload();
    downloadFile(csv, fileName);
  };

  const handleOdsDownload = () => {
    const title = generateTableTitle(fullTable.subjectMeta);
    downloadTableOdsFile(fileName, fullTable.subjectMeta, tableRef, title);
  };

  return (
    <Formik<FormValues>
      initialValues={{
        fileFormat: undefined as never,
      }}
      onSubmit={async ({ fileFormat }) => {
        await onSubmit?.(fileFormat);

        if (fileFormat === 'csv') {
          await handleCsvDownload();
        } else {
          await handleOdsDownload();
        }
      }}
      validationSchema={Yup.object<FormValues>({
        fileFormat: Yup.string()
          .oneOf<FileFormat>(['ods', 'csv'])
          .required('Choose a file format'),
      })}
    >
      {form => {
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
              <ButtonGroup>
                <Button type="submit" disabled={form.isSubmitting}>
                  Download table
                </Button>
                <LoadingSpinner
                  alert
                  className="govuk-!-margin-left-2"
                  inline
                  hideText
                  loading={form.isSubmitting}
                  size="md"
                  text="Preparing download"
                />
              </ButtonGroup>
            </>
          </Form>
        );
      }}
    </Formik>
  );
};

export default DownloadTable;
