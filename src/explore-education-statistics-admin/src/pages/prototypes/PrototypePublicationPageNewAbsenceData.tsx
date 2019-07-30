import React from 'react';
import ModalConfirm from '@common/components/ModalConfirm';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import useToggle from '@common/hooks/useToggle';
import Link from '@admin/components/Link';
import PrototypeAdminNavigation from './components/PrototypeAdminNavigation';
import PrototypePage from './components/PrototypePage';

const PublicationDataPage = () => {
  const [showReplaceDataModal, toggleReplaceDataModal] = useToggle(false);
  const [showDeleteFilesModal, toggleDeleteFilesModal] = useToggle(false);
  return (
    <PrototypePage
      wide
      breadcrumbs={[
        {
          link: '/prototypes/admin-dashboard?status=editNewRelease',
          text: 'Administrator dashboard',
        },
        { text: 'Create new release', link: '#' },
      ]}
    >
      <PrototypeAdminNavigation sectionId="addData" />
      <Tabs id="dataUploadTab">
        <TabsSection id="data-upload" title="Data uploads">
          <div className="govuk-body">
            This data will be used to create your data blocks and users will be
            able to download this data from this release.
          </div>

          <div className="govuk-table">
            <caption className="govuk-table__caption govuk-heading-m">
              Current data for this release
            </caption>
            <thead className="govuk-table__head">
              <tr className="govuk-table__row">
                <th className="govuk-table__header" scope="col">
                  Title
                </th>
                <th className="govuk-table__header" scope="col">
                  Data file
                </th>
                <th
                  className="govuk-table__header govuk-table__cell--numeric"
                  scope="col"
                >
                  Filesize
                </th>
                <th
                  className="govuk-table__header govuk-table__header--numeric"
                  scope="col"
                >
                  Number of rows
                </th>
                <th className="govuk-table__header" scope="col">
                  Metadata file
                </th>
                <th className="govuk-table__header" colSpan={3} scope="col">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody>
              <tr className="govuk-table__row">
                <td className="govuk-table__cell">Geographical absence</td>
                <td className="govuk-table__cell">
                  <a href="#">absence_geoglevels.csv</a>
                </td>
                <td className="govuk-table__cell govuk-table__cell--numeric">
                  61 Mb
                </td>
                <td className="govuk-table__cell govuk-table__cell--numeric">
                  212,000
                </td>
                <td className="govuk-table__cell">
                  <a href="#">meta_absence_geoglevels.csv</a>
                </td>
                <td className="govuk-table__cell">
                  <a href="#" onClick={() => toggleDeleteFilesModal(true)}>
                    Delete files
                  </a>
                </td>
                <td className="govuk-table__cell">
                  <a href="#" onClick={() => toggleReplaceDataModal(true)}>
                    Replace data
                  </a>
                </td>
                <td className="govuk-table__cell">
                  <a href="#">Replace metadata</a>
                </td>
              </tr>
              <tr className="govuk-table__row">
                <td className="govuk-table__cell">Local authority</td>
                <td className="govuk-table__cell">
                  <a href="#">absence_lacharacteristics.csv</a>
                </td>
                <td className="govuk-table__cell govuk-table__cell--numeric">
                  66 Mb
                </td>
                <td className="govuk-table__cell govuk-table__cell--numeric">
                  240,000
                </td>
                <td className="govuk-table__cell">
                  <a href="#">meta_absence_lacharacteristics.csv</a>
                </td>
                <td className="govuk-table__cell">
                  <a href="#">Delete files</a>
                </td>
                <td className="govuk-table__cell">
                  <a href="#">Replace data</a>
                </td>
                <td className="govuk-table__cell">
                  <a href="#">Replace metadata</a>
                </td>
              </tr>
              <tr className="govuk-table__row">
                <td className="govuk-table__cell">National characteristics</td>
                <td className="govuk-table__cell">
                  <a href="#">absence_natcharacteristics.csv</a>
                </td>
                <td className="govuk-table__cell govuk-table__cell--numeric">
                  71 Mb
                </td>
                <td className="govuk-table__cell govuk-table__cell--numeric">
                  320,000
                </td>
                <td className="govuk-table__cell">
                  <a href="#">meta_absence_natcharacteristics.csv</a>
                </td>
                <td className="govuk-table__cell">
                  <a href="#">Delete files</a>
                </td>
                <td className="govuk-table__cell">
                  <a href="#">Replace data</a>
                </td>
                <td className="govuk-table__cell">
                  <a href="#">Replace metadata</a>
                </td>
              </tr>
            </tbody>
          </div>

          <form>
            <fieldset className="govuk-fieldset">
              <legend className="govuk-fieldset__legend govuk-fieldset__legend--m">
                Upload new data to this release
              </legend>

              <div className="govuk-form-group">
                <label htmlFor="release-title" className="govuk-label">
                  Add data file title
                  <span className="govuk-hint">
                    This will be the name shown to users for the data file when
                    they download it from this release.
                    <br />
                    You should also use it as a reference for the data file if
                    you refer to it in this release.
                  </span>
                </label>
                <input
                  type="text"
                  id="release-title"
                  className="govuk-input govuk-!-width-one-half"
                />
              </div>

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
            <p className="govuk-body govuk-!-margin-top-9">
              Depending on your internet connection speed, files may take some
              time to upload. Do not close this page until the file has
              uploaded.
            </p>
            <p className="govuk-body">
              If you close this page while the file is still uploading - the
              upload will fail.
            </p>
            <div className="govuk-form-group govuk-!-margin-top-6">
              <button className="govuk-button" type="button">
                Upload data files
              </button>
            </div>
          </form>
        </TabsSection>
        <TabsSection id="file-upload" title="File uploads">
          <p className="govuk-body">
            These files should be things like infographics or more complex
            tables and charts which you have not been able to build using the
            table or chart tools.
          </p>
          <p className="govuk-body">
            Users will be able to download these files from this release.
          </p>
          <table className="govuk-table">
            <caption className="govuk-table__caption govuk-heading-m">
              Current files for this release
            </caption>
            <thead className="govuk-table__head">
              <tr className="govuk-table__row">
                <th className="govuk-table__header" scope="col">
                  Title
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
                Upload files to this release
              </legend>

              <div className="govuk-form-group">
                <label
                  htmlFor="release-fileupload-name"
                  className="govuk-label"
                >
                  Add file title
                  <span className="govuk-hint">
                    This will be the name shown to users for the file when they
                    download it from this release.
                    <br />
                    You should also use it as a reference for the file if you
                    refer to it in this release.
                  </span>
                </label>
                <input
                  type="text"
                  id="release-fileupload-name"
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
            <p className="govuk-body govuk-!-margin-top-9">
              Depending on your internet connection speed, files may take some
              time to upload. Do not close this page until the file has
              uploaded.
            </p>
            <p className="govuk-body">
              If you close this page while the file is still uploading - the
              upload will fail.
            </p>
            <div className="govuk-form-group govuk-!-margin-top-6">
              <button className="govuk-button" type="button">
                Upload files
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
            <span className="govuk-heading-m govuk-!-margin-bottom-0">
              Previous step
            </span>
            Release summary
          </Link>
        </div>
        <div className="govuk-grid-column-one-half dfe-align--right">
          <Link to="/prototypes/publication-create-new-absence-table?status=step1">
            <span className="govuk-heading-m govuk-!-margin-bottom-0">
              Next step
            </span>
            Manage tables and charts
          </Link>
        </div>
      </div>
    </PrototypePage>
  );
};

export default PublicationDataPage;
