import React from 'react';
import PrototypeAdminNavigation from './components/PrototypeAdminNavigation';
import PrototypePage from './components/PrototypePage';

const PublicationDataPage = () => {
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
      <div className="govuk-table">
        <caption className="govuk-table__caption govuk-heading-m">
          Current data for this release
        </caption>
        <thead className="govuk-table__head">
          <tr className="govuk-table__row">
            <th className="govuk-table__header" scope="col">
              Data label
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

      <h2 className="govuk-heading-m govuk-!-margin-top-9">
        Add new data to release
      </h2>

      <div className="govuk-form-group">
        <label className="govuk-label" htmlFor="file-upload-1">
          Upload a file test
        </label>
        <input
          className="govuk-file-upload"
          id="file-upload-1"
          name="file-upload-1"
          type="file"
        />
      </div>
    </PrototypePage>
  );
};

export default PublicationDataPage;
