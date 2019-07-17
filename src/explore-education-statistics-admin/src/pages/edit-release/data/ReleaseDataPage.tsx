import ReleasePageTemplate from "@admin/pages/edit-release/components/ReleasePageTemplate";
import ReleaseDataFileUploadsSection from "@admin/pages/edit-release/data/ReleaseDataFileUploadsSection";
import service from '@admin/services/common/service';
import {IdLabelPair} from "@admin/services/common/types";
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import React, {useEffect, useState} from 'react';
import {RouteComponentProps} from 'react-router';

interface MatchProps {
  releaseId: string;
}

const ReleaseDataPage = ({ match }: RouteComponentProps<MatchProps>) => {
  const { releaseId } = match.params;
  const [publicationDetails, setPublicationDetails] = useState<IdLabelPair>();

  useEffect(() => {
    service.getPublicationDetailsForRelease(releaseId).then(setPublicationDetails)
  }, [releaseId]);

  return (
    <>
      {publicationDetails && (
        <ReleasePageTemplate
          publicationTitle={publicationDetails.label}
          releaseId={releaseId}
        >
          <h3>Data uploads</h3>

          <Tabs id="dataUploadTab">
            <TabsSection id="data-upload" title="Data uploads">
              <ReleaseDataFileUploadsSection releaseId={releaseId} />
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
        </ReleasePageTemplate>
      )}
    </>
  );
};

export default ReleaseDataPage;
