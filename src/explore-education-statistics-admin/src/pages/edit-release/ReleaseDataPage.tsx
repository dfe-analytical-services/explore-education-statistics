import Link from '@admin/components/Link';
import { DataFileView } from '@admin/services/api/edit-release/data/types';
import ModalConfirm from '@common/components/ModalConfirm';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import React, { useEffect, useState } from 'react';
import { RouteComponentProps } from 'react-router';
import service from '@admin/services/api/edit-release/data/service';
import ReleasePageTemplate from '@admin/pages/edit-release/components/ReleasePageTemplate';

interface MatchProps {
  releaseId: string;
}

const ReleaseDataPage = ({ match }: RouteComponentProps<MatchProps>) => {
  const { releaseId } = match.params;

  const [dataFiles, setDataFiles] = useState<DataFileView>();

  useEffect(() => {
    service.getReleaseDataFiles(releaseId).then(setDataFiles);
  }, [releaseId]);

  const toggleDeleteFilesModal = (_: boolean) => {};

  const toggleReplaceDataModal = (_: boolean) => {};

  const showReplaceDataModal = false;

  const showDeleteFilesModal = false;

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
                  {dataFile.file.fileName}
                </SummaryListItem>
                <SummaryListItem term="Filesize">
                  {dataFile.fileSize.size} {dataFile.fileSize.unit}
                </SummaryListItem>
                <SummaryListItem term="Number of rows">
                  {dataFile.numberOfRows}
                </SummaryListItem>
                <SummaryListItem term="Metadata file">
                  {dataFile.metadataFile.fileName}
                </SummaryListItem>
                <SummaryListItem
                  term="Actions"
                  actions={
                    <>
                      <Link to="#">Delete files</Link>
                      <Link to="#">Replace data</Link>
                      <Link to="#">Replace metadata</Link>
                    </>
                  }
                />
              </SummaryList>
            ))}

          <form>
            <fieldset className="govuk-fieldset">
              <legend className="govuk-fieldset__legend govuk-fieldset__legend--m">
                Add new data to release
              </legend>

              <div className="govuk-form-group">
                <label htmlFor="release-title" className="govuk-label">
                  Subject title
                </label>
                <input
                  type="text"
                  className="govuk-input govuk-!-width-one-half"
                />
              </div>

              <fieldset className="govuk-fieldset">
                <legend className="govuk-fieldset__legend govuk-fieldset__legend--s">
                  Data type
                </legend>
                <div className="govuk-radios">
                  <div className="govuk-radios__item">
                    <input
                      type="radio"
                      name="data-type"
                      id="data-type-final"
                      value="final"
                      className="govuk-radios__input"
                    />
                    <label
                      htmlFor="data-type=final"
                      className="govuk-label govuk-radios__label"
                    >
                      Final
                    </label>
                  </div>
                  <div className="govuk-radios__item">
                    <input
                      type="radio"
                      name="data-type"
                      id="data-type-provisional"
                      value="provisional"
                      className="govuk-radios__input"
                    />
                    <label
                      htmlFor="data-type-final"
                      className="govuk-label govuk-radios__label"
                    >
                      Provisional
                    </label>
                  </div>
                  <div className="govuk-radios__item">
                    <input
                      type="radio"
                      name="data-type"
                      id="data-type-revised"
                      value="revised"
                      className="govuk-radios__input"
                    />
                    <label
                      htmlFor="data-type-revised"
                      className="govuk-label govuk-radios__label"
                    >
                      Revised
                    </label>
                  </div>
                </div>
              </fieldset>

              <div className="govuk-form-group govuk-!-margin-top-6">
                <label
                  className="govuk-label govuk-label--s"
                  htmlFor="data-upload-1"
                >
                  Upload data
                </label>
                <input
                  className="govuk-file-upload"
                  id="data-upload-1"
                  name="data-upload-1"
                  type="file"
                />
              </div>

              <div className="govuk-form-group govuk-!-margin-top-6">
                <label
                  className="govuk-label govuk-label--s"
                  htmlFor="data-upload-2"
                >
                  Upload metadata
                </label>
                <input
                  className="govuk-file-upload"
                  id="data-upload-2"
                  name="data-upload-2"
                  type="file"
                />
              </div>
            </fieldset>
            <div className="govuk-form-group govuk-!-margin-top-6">
              <button className="govuk-button" type="button">
                Upload data files
              </button>
            </div>
          </form>
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
        mounted={showReplaceDataModal}
        title="Confirm replace data file"
        onExit={() => toggleReplaceDataModal(false)}
        onConfirm={() => toggleReplaceDataModal(false)}
        onCancel={() => toggleReplaceDataModal(false)}
      >
        <p>Please ensure supporting meta data file is still correct</p>
      </ModalConfirm>

      <ModalConfirm
        mounted={showDeleteFilesModal}
        title="Confirm deletion of selected data files"
        onExit={() => toggleDeleteFilesModal(false)}
        onConfirm={() => toggleDeleteFilesModal(false)}
        onCancel={() => toggleDeleteFilesModal(false)}
      >
        <p>This data will no longer be available for use in this release</p>
      </ModalConfirm>

      <div className="govuk-grid-row govuk-!-margin-top-9">
        <div className="govuk-grid-column-one-half ">
          <Link to="/prototypes/publication-create-new-absence-config">
            Previous step, release setup
          </Link>
        </div>
        <div className="govuk-grid-column-one-half dfe-align--right">
          <Link to="/prototypes/publication-create-new-absence-table?status=step1">
            Next step, build tables
          </Link>
        </div>
      </div>
    </ReleasePageTemplate>
  );
};

export default ReleaseDataPage;
