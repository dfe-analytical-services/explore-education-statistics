import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import Details from '@common/components/Details';
import { Release } from '@common/services/publicationService';
import { FileInfo } from '@common/services/types/file';
import orderBy from 'lodash/orderBy';
import React, { ReactNode } from 'react';
import styles from './ReleaseDataAndFilesAccordion.module.scss';

interface Props {
  release: Release;
  renderCreateTablesButton?: ReactNode;
  renderDataCatalogueLink?: ReactNode;
  renderDownloadLink: (file: FileInfo) => ReactNode;
  renderMetaGuidanceLink: ReactNode;
  renderPreReleaseAccessLink?: ReactNode;
  onSectionOpen?: (accordionSection: { id: string; title: string }) => void;
}

const ReleaseDataAndFilesAccordion = ({
  release,
  renderCreateTablesButton,
  renderDataCatalogueLink,
  renderDownloadLink,
  renderMetaGuidanceLink,
  renderPreReleaseAccessLink,
  onSectionOpen,
}: Props) => {
  const allFilesZip = release.downloadFiles.find(
    file => file.name === 'All files' && file.type === 'Ancillary',
  );

  const files = orderBy(
    release.downloadFiles.filter(
      file => file.type !== 'Ancillary' && file.name !== 'All files',
    ),
    ['name'],
  );

  const otherFiles = orderBy(
    release.downloadFiles.filter(
      file => file.type === 'Ancillary' && file.name !== 'All files',
    ),
    ['name'],
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
          <p>
            All data used to create this release is published as open data and
            is available for download.
          </p>
          <p>
            You can create your own tables from this data using our table tool,
            or view featured tables that we have built for you.
          </p>

          {(allFilesZip || files.length > 0) && (
            <ul className="govuk-list" data-testid="download-files">
              {allFilesZip && (
                <li>
                  {renderDownloadLink(allFilesZip)}
                  {` (${allFilesZip.extension}, ${allFilesZip.size})`}
                </li>
              )}

              {files.map(file => (
                <li key={file.id}>
                  {renderDownloadLink(file)}
                  {` (${file.extension}, ${file.size})`}
                </li>
              ))}
            </ul>
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

              {renderDataCatalogueLink && (
                <p>
                  Browse and download individual open data files in our{' '}
                  {renderDataCatalogueLink}.
                </p>
              )}
            </>
          )}

          {otherFiles.length > 0 && (
            <>
              <h3>Other files</h3>
              <p>All other files available for download are listed below:</p>

              <Details summary="List of other files">
                <ul className="govuk-list" data-testid="other-download-files">
                  {otherFiles.map(file => (
                    <li key={file.id}>
                      {renderDownloadLink(file)}
                      {` (${file.extension}, ${file.size})`}

                      {file.summary && (
                        <Details
                          summary="More details"
                          className="govuk-!-margin-top-2"
                        >
                          <div className="dfe-white-space--pre-wrap">
                            {file.summary}
                          </div>
                        </Details>
                      )}
                    </li>
                  ))}
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
