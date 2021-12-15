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
  renderCreateTablesButton: ReactNode;
  renderDataCatalogueLink: ReactNode;
  renderDataGuidanceLink: ReactNode;
  renderDownloadLink: (file: FileInfo) => ReactNode;
  showDownloadFilesList?: boolean;
  onSectionOpen?: (accordionSection: { id: string; title: string }) => void;
}

const ReleaseDataAndFilesAccordion = ({
  release,
  renderAllFilesButton,
  renderCreateTablesButton,
  renderDataCatalogueLink,
  renderDataGuidanceLink,
  renderDownloadLink,
  showDownloadFilesList = false,
  onSectionOpen,
}: Props) => {
  const dataFiles = orderBy(
    release.downloadFiles.filter(file => file.type === 'Data'),
    ['name'],
  );

  const ancillaryFiles = orderBy(
    release.downloadFiles.filter(
      file => file.type === 'Ancillary' && file.name !== 'All files',
    ),
    ['name'],
  );

  const hasAllFilesButton =
    (dataFiles.length > 0 || ancillaryFiles.length > 0) && renderAllFilesButton;

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
          <div className="govuk-grid-row dfe-flex dfe-align-items--center">
            <div
              className={classNames({
                'govuk-grid-column-three-quarters': hasAllFilesButton,
                'govuk-grid-column-full': !hasAllFilesButton,
              })}
            >
              <p className="govuk-!-margin-bottom-0">
                All data used in this release is available as open data for
                download
              </p>
            </div>

            {hasAllFilesButton && (
              <div className="govuk-grid-column-one-quarter">
                {renderAllFilesButton}
              </div>
            )}
          </div>

          <hr className="govuk-section-break govuk-section-break--m govuk-section-break--visible" />

          <div className="govuk-grid-row dfe-flex dfe-align-items--center">
            <div className="govuk-grid-column-three-quarters">
              <h3>Open data</h3>
              <p className="govuk-!-margin-bottom-0">
                Browse and download individual open data files from this release
                in our data catalogue
              </p>

              {showDownloadFilesList && dataFiles.length > 0 && (
                <Details
                  summary="Download files"
                  className="govuk-!-margin-bottom-0 govuk-!-margin-top-2"
                >
                  <ul className="govuk-list" data-testid="data-files">
                    {dataFiles.map(file => (
                      <li key={file.id}>
                        {renderDownloadLink(file)}
                        {` (${file.extension}, ${file.size})`}
                      </li>
                    ))}
                  </ul>
                </Details>
              )}
            </div>
            <div className="govuk-grid-column-one-quarter">
              {renderDataCatalogueLink}
            </div>
          </div>
          <hr className="govuk-section-break govuk-section-break--m govuk-section-break--visible" />

          {release.hasDataGuidance && (
            <>
              <div className="govuk-grid-row dfe-flex dfe-align-items--center">
                <div className="govuk-grid-column-three-quarters">
                  <h3>Guidance</h3>
                  <p className="govuk-!-margin-bottom-0">
                    Learn more about the data files used in this release using
                    our online guidance
                  </p>
                </div>
                <div className="govuk-grid-column-one-quarter">
                  {renderDataGuidanceLink}
                </div>
              </div>
              <hr className="govuk-section-break govuk-section-break--m govuk-section-break--visible" />
            </>
          )}

          <div className="govuk-grid-row dfe-flex dfe-align-items--center">
            <div className="govuk-grid-column-three-quarters">
              <h3>Create your own tables</h3>
              <p className="govuk-!-margin-bottom-0">
                You can view featured tables that we have built for you, or
                create your own tables from the open data using our table tool
              </p>
            </div>
            <div className="govuk-grid-column-one-quarter">
              {renderCreateTablesButton}
            </div>
          </div>
          <hr className="govuk-section-break govuk-section-break--m govuk-section-break--visible" />

          {ancillaryFiles.length > 0 && (
            <>
              <h3>All supporting files</h3>
              <p>
                All supporting files from this release are listed for individual
                download below:
              </p>

              <Details summary="List of all supporting files">
                <ul className="govuk-list" data-testid="other-files">
                  {ancillaryFiles.map(file => (
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
        </AccordionSection>
      </Accordion>
    </div>
  );
};

export default ReleaseDataAndFilesAccordion;
