import { Form, FormFieldRadioGroup } from '@common/components/form';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import downloadTableCsvFile from '@common/modules/table-tool/components/utils/downloadTableCsvFile';
import downloadTableOdsFile from '@common/modules/table-tool/components/utils/downloadTableOdsFile';
import useToggle from '@common/hooks/useToggle';
import generateTableTitle from '@common/modules/table-tool/utils/generateTableTitle';
import Yup from '@common/validation/yup';
import LoadingSpinner from '@common/components/LoadingSpinner';
import { Formik } from 'formik';
import React, { createElement, RefObject } from 'react';

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
  const [processingData, toggleProcessingData] = useToggle(false);

  const handleCsvDownload = async () => {
    await downloadTableCsvFile(fileName, fullTable);
    toggleProcessingData();
  };

  const handleOdsDownload = () => {
    const title = generateTableTitle(fullTable.subjectMeta);
    downloadTableOdsFile(fileName, fullTable.subjectMeta, tableRef, title);
    toggleProcessingData();
  };

  return (
    <Formik<FormValues>
      initialValues={{
        fileFormat: undefined,
      }}
      onSubmit={values => {
        toggleProcessingData();
        if (onSubmit) {
          onSubmit(values.fileFormat);
        }
        return values.fileFormat === 'csv'
          ? handleCsvDownload()
          : handleOdsDownload();
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
              <ButtonGroup>
                <Button type="submit" disabled={processingData}>
                  Download table
                </Button>
                <LoadingSpinner
                  alert
                  className="govuk-!-margin-left-2"
                  inline
                  hideText
                  loading={processingData}
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
