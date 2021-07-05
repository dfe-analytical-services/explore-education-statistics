import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import Details from '@common/components/Details';
import { Release } from '@common/services/publicationService';
import { FileInfo } from '@common/services/types/file';
import React, { ReactNode } from 'react';
import styles from './ReleaseDataAndFilesAccordion.module.scss';

interface Props {
  release: Release;
  renderCreateTablesButton?: ReactNode;
  renderDownloadLink: (file: FileInfo) => ReactNode;
  renderMetaGuidanceLink: ReactNode;
  renderPreReleaseAccessLink?: ReactNode;
  onSectionOpen?: (accordionSection: { id: string; title: string }) => void;
}
const ReleaseDataAndFilesAccordion = ({
  release,
  renderCreateTablesButton,
  renderDownloadLink,
  renderMetaGuidanceLink,
  renderPreReleaseAccessLink,
  onSectionOpen,
}: Props) => {
  const otherFiles = release.downloadFiles.filter(
    file => file.name === 'All files' || file.type !== 'Ancillary',
  );

  const ancillaryFiles = release.downloadFiles.filter(
    file => file.type === 'Ancillary' && file.name !== 'All files',
  );

  return (
    <div className={styles.container}>
      <Accordion
        id="dataDownloads"
        showOpenAll={false}
        onSectionOpen={accordionSection => {
          if (onSectionOpen) {
            onSectionOpen(accordionSection);
          }
        }}
      >
        <AccordionSection heading="Explore data and files">
          {otherFiles.length > 0 && (
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
                {otherFiles.map(file => {
                  return <li key={file.id}>{renderDownloadLink(file)}</li>;
                })}
              </ul>
            </>
          )}
          {renderCreateTablesButton && (
            <div className={styles.createTablesButtonContainer}>
              <div>
                <h3>Create your own tables</h3>
                <p>
                  Explore our range of data and build your own tables from it.
                </p>
              </div>
              <div className="govuk-!-width-one-quarter">
                {renderCreateTablesButton}
              </div>
            </div>
          )}

          {release.hasMetaGuidance && (
            <>
              <h3>Open data</h3>
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
              <h3>Other files</h3>
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
              <h3>Pre-release access list</h3>
              <p>{renderPreReleaseAccessLink}</p>
            </>
          )}
        </AccordionSection>
      </Accordion>
    </div>
  );
};

export default ReleaseDataAndFilesAccordion;
