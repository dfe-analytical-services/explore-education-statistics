import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import Details from '@common/components/Details';
import { Release } from '@common/services/publicationService';
import { FileInfo } from '@common/services/types/file';
import React, { ReactNode } from 'react';

interface Props {
  renderCreateTablesButton?: ReactNode;
  onSectionOpenEvent?: (accordionSection: {
    id: string;
    title: string;
  }) => void;
  release: Release;
  renderDownloadLink: (file: FileInfo) => ReactNode;
  renderMetaGuidanceLink: ReactNode;
  renderPreReleaseAccessLink?: ReactNode;
}
const ReleaseDataAndFiles = ({
  onSectionOpenEvent,
  release,
  renderCreateTablesButton,
  renderDownloadLink,
  renderMetaGuidanceLink,
  renderPreReleaseAccessLink,
}: Props) => {
  const dataFiles = release.downloadFiles.filter(
    file => file.name === 'All files' || file.type !== 'Ancillary',
  );

  const ancillaryFiles = release.downloadFiles.filter(
    file => file.type === 'Ancillary' && file.name !== 'All files',
  );

  return (
    <div className="dfe-download-section">
      <Accordion
        id="dataDownloads"
        showOpenAll={false}
        onSectionOpen={accordionSection => {
          if (onSectionOpenEvent) {
            onSectionOpenEvent(accordionSection);
          }
        }}
      >
        <AccordionSection heading="Explore data and files">
          {dataFiles.length > 0 && (
            <>
              <p>
                All data used to create this release is published as open data
                and is available for download.
              </p>
              <p>
                You can create your own tables from this data using our table
                tool, or view featured tables that we have built for you.
              </p>

              <ul className="govuk-list">
                {dataFiles.map(file => {
                  return <li key={file.id}>{renderDownloadLink(file)}</li>;
                })}
              </ul>
            </>
          )}
          {renderCreateTablesButton && (
            <div className="dfe-flex dfe-justify-content--space-between dfe-align-items--center">
              <div>
                <h2 className="govuk-heading-m">Create your own tables</h2>
                <p>
                  Explore our range of data and build your own tables from it.
                </p>
              </div>
              <p className="govuk-!-width-one-quarter dfe-flex-shrink--0">
                {renderCreateTablesButton}
              </p>
            </div>
          )}

          {release.hasMetaGuidance && (
            <>
              <h2 className="govuk-heading-m">Open data</h2>
              <p>
                The open data files contain all data used in this release in a
                machine readable format.
              </p>
              <p>
                Learn more about the data files used in this release using our{' '}
                {renderMetaGuidanceLink}.
              </p>
            </>
          )}
          {ancillaryFiles.length > 0 && (
            <>
              <h2 className="govuk-heading-m">Other files</h2>
              <p>All other files available for download are listed below:</p>
              <Details summary="List of other files">
                <ul className="govuk-list">
                  {ancillaryFiles.map(file => {
                    return <li key={file.id}>{renderDownloadLink(file)}</li>;
                  })}
                </ul>
              </Details>
            </>
          )}
          {release.hasPreReleaseAccessList && renderPreReleaseAccessLink && (
            <>
              <h2 className="govuk-heading-m">Pre-release access list</h2>
              <p>{renderPreReleaseAccessLink}</p>
            </>
          )}
        </AccordionSection>
      </Accordion>
    </div>
  );
};

export default ReleaseDataAndFiles;
