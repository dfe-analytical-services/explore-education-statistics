import Link from '@admin/components/Link';
import { dataRoute } from '@admin/routes/releaseRoutes';
import { DataFileView } from '@admin/services/edit-release/data/types';
import Button from '@common/components/Button';
import { Form, FormFieldset, Formik } from '@common/components/form';
import FormFieldFileSelector from '@common/components/form/FormFieldFileSelector';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import ModalConfirm from '@common/components/ModalConfirm';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import Yup from '@common/lib/validation/yup';
import { FormikProps } from 'formik';
import React, { useEffect, useState } from 'react';
import { RouteComponentProps } from 'react-router';
import service from '@admin/services/edit-release/data/service';
import ReleasePageTemplate from '@admin/pages/edit-release/components/ReleasePageTemplate';

interface MatchProps {
  releaseId: string;
}

interface FormValues {
  subjectTitle: string;
  dataFile: File | null;
  metadataFile: File | null;
}

const ReleaseDataPage = ({ match }: RouteComponentProps<MatchProps>) => {
  const { releaseId } = match.params;

  const [dataFiles, setDataFiles] = useState<DataFileView>();
  const [deleteFileId, setDeleteFilesRequest] = useState('');

  useEffect(() => {
    service.getReleaseDataFiles(releaseId).then(setDataFiles);
  }, [releaseId]);

  const formId = 'dataFileUploadForm';

  return (
    <ReleasePageTemplate
      publicationTitle={dataFiles ? dataFiles.publicationTitle : ''}
      releaseId={releaseId}
    >
      <h3>Data uploads</h3>

      <Tabs id="dataUploadTab">
        <TabsSection id="data-upload" title="Data uploads">
          {dataFiles &&
            dataFiles.dataFiles.map(dataFile => (
              <SummaryList key={dataFile.file.id}>
                <SummaryListItem term="Subject title">
                  {dataFile.title}
                </SummaryListItem>
                <SummaryListItem term="Data file">
                  <a
                    href={service.createDownloadDataFileLink(
                      releaseId,
                      dataFile.file.id,
                    )}
                  >
                    {dataFile.file.fileName}
                  </a>
                </SummaryListItem>
                <SummaryListItem term="Filesize">
                  {dataFile.fileSize.size.toLocaleString()}{' '}
                  {dataFile.fileSize.unit}
                </SummaryListItem>
                <SummaryListItem term="Number of rows">
                  {dataFile.numberOfRows.toLocaleString()}
                </SummaryListItem>
                <SummaryListItem term="Metadata file">
                  <a
                    href={service.createDownloadDataMetadataFileLink(
                      releaseId,
                      dataFile.file.id,
                    )}
                  >
                    {dataFile.metadataFile.fileName}
                  </a>
                </SummaryListItem>
                <SummaryListItem
                  term="Actions"
                  actions={
                    <Link
                      to="#"
                      onClick={_ => setDeleteFilesRequest(dataFile.file.id)}
                    >
                      Delete files
                    </Link>
                  }
                />
              </SummaryList>
            ))}
          <Formik<FormValues>
            enableReinitialize
            initialValues={{
              subjectTitle: '',
              dataFile: null,
              metadataFile: null,
            }}
            onSubmit={async (values: FormValues, actions) => {
              service
                .uploadDataFiles(releaseId, {
                  subjectTitle: values.subjectTitle,
                  dataFile: values.dataFile as File,
                  metadataFile: values.metadataFile as File,
                })
                .then(_ => service.getReleaseDataFiles(releaseId))
                .then(setDataFiles)
                .then(_ => {
                  actions.resetForm();
                  document
                    .querySelectorAll(`#${formId} input[type='file']`)
                    .forEach(input => {
                      const fileInput = input as HTMLInputElement;
                      fileInput.value = '';
                    });
                });
            }}
            validationSchema={Yup.object<FormValues>({
              subjectTitle: Yup.string().required('Enter a subject title'),
              dataFile: Yup.mixed().required('Choose a data file'),
              metadataFile: Yup.mixed().required('Choose a metadata file'),
            })}
            render={(form: FormikProps<FormValues>) => {
              return (
                <Form id={formId}>
                  <FormFieldset
                    id={`${formId}-allFieldsFieldset`}
                    legend="Add new data to release"
                  >
                    <FormFieldTextInput<FormValues>
                      id={`${formId}-subjectTitle`}
                      name="subjectTitle"
                      label="Subject title"
                    />

                    <FormFieldFileSelector<FormValues>
                      id={`${formId}-dataFile`}
                      name="dataFile"
                      label="Upload data file"
                      formGroupClass="govuk-!-margin-top-6"
                      form={form}
                    />

                    <FormFieldFileSelector<FormValues>
                      id={`${formId}-metadataFile`}
                      name="metadataFile"
                      label="Upload metadata file"
                      form={form}
                    />
                  </FormFieldset>

                  <Button type="submit" className="govuk-!-margin-top-6">
                    Upload data files
                  </Button>

                  <div className="govuk-!-margin-top-6">
                    <Link to={dataRoute.generateLink(releaseId)}>Cancel</Link>
                  </div>
                </Form>
              );
            }}
          />
        </TabsSection>
        <TabsSection id="file-upload" title="File uploads">
          <table className="govuk-table">
            <caption className="govuk-table__caption govuk-heading-m">
              File uploads available for this release
            </caption>
            <thead className="govuk-table__head">
              <tr className="govuk-table__row">
                <th className="govuk-table__header" scope="col">
                  Name
                </th>
                <th className="govuk-table__header" scope="col">
                  File
                </th>
                <th
                  className="govuk-table__header govuk-table__cell--numeric"
                  scope="col"
                >
                  Filesize
                </th>

                <th className="govuk-table__header" colSpan={3} scope="col">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody>
              <tr className="govuk-table__row">
                <td className="govuk-table__cell">Example graphic</td>
                <td className="govuk-table__cell">
                  <a href="#">example-graphics.png</a>
                </td>
                <td className="govuk-table__cell govuk-table__cell--numeric">
                  61 Mb
                </td>
                <td className="govuk-table__cell">
                  <a href="#">View file</a>
                </td>
                <td className="govuk-table__cell">
                  <a href="#">Delete file</a>
                </td>
              </tr>
            </tbody>
          </table>

          <form>
            <fieldset className="govuk-fieldset">
              <legend className="govuk-fieldset__legend govuk-fieldset__legend--m">
                Upload file
              </legend>

              <div className="govuk-form-group">
                <label
                  htmlFor="release-fileupload-name"
                  className="govuk-label"
                >
                  Name
                </label>
                <input
                  type="text"
                  className="govuk-input govuk-!-width-one-half"
                />
              </div>

              <div className="govuk-form-group govuk-!-margin-top-6">
                <label
                  className="govuk-label govuk-label--s"
                  htmlFor="file-upload-1"
                >
                  Upload data
                </label>
                <input
                  className="govuk-file-upload"
                  id="file-upload-1"
                  name="file-upload-1"
                  type="file"
                />
              </div>
            </fieldset>
            <div className="govuk-form-group govuk-!-margin-top-6">
              <button className="govuk-button" type="button">
                Upload file
              </button>
            </div>
          </form>
        </TabsSection>
      </Tabs>

      <ModalConfirm
        mounted={deleteFileId != null && deleteFileId.length > 0}
        title="Confirm deletion of selected data files"
        onExit={() => setDeleteFilesRequest('')}
        onCancel={() => setDeleteFilesRequest('')}
        onConfirm={() =>
          service
            .deleteDataFiles(releaseId, deleteFileId)
            .then(_ => service.getReleaseDataFiles(releaseId))
            .then(setDataFiles)
            .then(_ => setDeleteFilesRequest(''))
        }
      >
        <p>This data will no longer be available for use in this release</p>
      </ModalConfirm>
    </ReleasePageTemplate>
  );
};

export default ReleaseDataPage;
