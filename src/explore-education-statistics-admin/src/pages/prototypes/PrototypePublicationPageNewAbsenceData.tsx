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
              Subject title
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

      <form>
        <fieldset className="govuk-fieldset">
          <legend className="govuk-fieldset__legend govuk-fieldset__legend--m">
            Add new data to release
          </legend>

          <div className="govuk-form-group">
            <label htmlFor="release-title" className="govuk-label">
              Subject title
            </label>
            <input type="text" className="govuk-input govuk-!-width-one-half" />
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

          <fieldset className="govuk-fieldset govuk-!-margin-top-6">
            <legend className="govuk-fieldset__legend govuk-fieldset__legend--s">
              Level of methodology
            </legend>
            <div className="govuk-radios">
              <div className="govuk-radios__item">
                <input
                  type="radio"
                  name="methodology-level"
                  id="methodology-school-location"
                  value="school-location"
                  className="govuk-radios__input"
                />
                <label
                  htmlFor="data-type=final"
                  className="govuk-label govuk-radios__label"
                >
                  School location
                </label>
              </div>
              <div className="govuk-radios__item">
                <input
                  type="radio"
                  name="methodology-level"
                  id="methodology-pupil-residency"
                  value="pupil residency"
                  className="govuk-radios__input"
                />
                <label
                  htmlFor="data-type=final"
                  className="govuk-label govuk-radios__label"
                >
                  Pupil residency
                </label>
              </div>
            </div>
          </fieldset>

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

          <div className="govuk-form-group govuk-!-margin-top-6">
            <label
              className="govuk-label govuk-label--s"
              htmlFor="file-upload-2"
            >
              Upload metadata
            </label>
            <input
              className="govuk-file-upload"
              id="file-upload-2"
              name="file-upload-2"
              type="file"
            />
          </div>
        </fieldset>
        <div className="govuk-form-group govuk-!-margin-top-6">
          <button className="govuk-button">Upload data files</button>
        </div>
      </form>
    </PrototypePage>
  );
};

export default PublicationDataPage;
