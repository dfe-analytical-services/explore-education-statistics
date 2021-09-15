import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import Details from '@common/components/Details';
import { Release } from '@common/services/publicationService';
import { FileInfo } from '@common/services/types/file';
import classNames from 'classnames';
import orderBy from 'lodash/orderBy';
import React, { ReactNode } from 'react';
import styles from './ReleaseDataAndFilesAccordion.module.scss';

interface Props {
  release: Release;
  renderAllFilesButton?: ReactNode;
  renderCreateTablesButton?: ReactNode;
  renderDataCatalogueLink?: ReactNode;
  renderDownloadLink: (file: FileInfo) => ReactNode;
  renderMetaGuidanceLink: ReactNode;
  renderPreReleaseAccessLink?: ReactNode;
  onSectionOpen?: (accordionSection: { id: string; title: string }) => void;
}

const ReleaseDataAndFilesAccordion = ({
  release,
  renderAllFilesButton,
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

  const hasAllFilesButton = allFilesZip && renderAllFilesButton;

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
          <div className="govuk-grid-row">
            <div
              className={classNames({
                'govuk-grid-column-three-quarters': hasAllFilesButton,
                'govuk-grid-column-full': !hasAllFilesButton,
              })}
            >
              <p>
                All data used to create this release is published as open data
                and is available for download.
              </p>
            </div>

            {hasAllFilesButton && (
              <div className="govuk-grid-column-one-quarter">
                {renderAllFilesButton}
              </div>
            )}
          </div>

          {renderCreateTablesButton && (
            <>
              <h3>Create your own tables</h3>

              <div className="govuk-grid-row">
                <div className="govuk-grid-column-three-quarters">
                  <p>
                    You can create your own tables from this data using our
                    table tool, or view featured tables that we have built for
                    you.
                  </p>
                </div>
                <div className="govuk-grid-column-one-quarter">
                  {renderCreateTablesButton}
                </div>
              </div>
            </>
          )}

          <div className="govuk-grid-row">
            <div className="govuk-grid-column-three-quarters">
              <h3>Open data</h3>
              <p>
                The open data files contain all data used in this release in a
                machine readable format.
              </p>

              {!renderDataCatalogueLink && files.length > 0 && (
                <ul className="govuk-list" data-testid="download-files">
                  {files.map(file => (
                    <li key={file.id}>
                      {renderDownloadLink(file)}
                      {` (${file.extension}, ${file.size})`}
                    </li>
                  ))}
                </ul>
              )}

              {release.hasMetaGuidance && (
                <p>
                  Learn more about the data files used in this release using our{' '}
                  {renderMetaGuidanceLink}.
                </p>
              )}

              {renderDataCatalogueLink && (
                <p>
                  Browse and download individual open data files in our{' '}
                  {renderDataCatalogueLink}.
                </p>
              )}
            </div>
          </div>

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
