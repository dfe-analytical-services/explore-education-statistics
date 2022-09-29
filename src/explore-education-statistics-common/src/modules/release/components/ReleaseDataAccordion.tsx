import AccordionSection from '@common/components/AccordionSection';
import Details from '@common/components/Details';
import SectionBreak from '@common/components/SectionBreak';
import Accordion from '@common/components/Accordion';
import { Release } from '@common/services/publicationService';
import { FileInfo } from '@common/services/types/file';
import classNames from 'classnames';
import orderBy from 'lodash/orderBy';
import React, { ReactNode } from 'react';
import styles from './ReleaseDataAccordion.module.scss';

interface Props {
  release: Release;
  renderAllFilesButton?: ReactNode;
  renderCreateTablesButton: ReactNode;
  renderDataCatalogueLink: ReactNode;
  renderDataGuidanceLink: ReactNode;
  renderDownloadLink: (file: FileInfo) => ReactNode;
  renderRelatedDashboards?: ReactNode;
  onSectionOpen?: (accordionSection: { id: string; title: string }) => void;
  showDownloadFilesList?: boolean;
}

const ReleaseDataAccordion = ({
  release,
  renderAllFilesButton,
  renderCreateTablesButton,
  renderDataCatalogueLink,
  renderDataGuidanceLink,
  renderDownloadLink,
  renderRelatedDashboards,
  onSectionOpen,
  showDownloadFilesList = false,
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
        id="data-accordion"
        showOpenAll={false}
        onSectionOpen={onSectionOpen}
      >
        {release.downloadFiles && (
          <AccordionSection
            id="explore-data-and-files"
            heading="Explore data and files"
          >
            <div
              className={`govuk-grid-row dfe-flex dfe-align-items--center ${styles.section}`}
            >
              <div
                className={classNames({
                  'govuk-grid-column-three-quarters': hasAllFilesButton,
                  'govuk-grid-column-full': !hasAllFilesButton,
                })}
              >
                <p>
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

            <SectionBreak />

            <div
              className={`govuk-grid-row dfe-flex dfe-align-items--center ${styles.section}`}
            >
              <div className="govuk-grid-column-three-quarters">
                <h3>Open data</h3>
                <p>
                  Browse and download individual open data files from this
                  release in our data catalogue
                </p>

                {showDownloadFilesList && dataFiles.length > 0 && (
                  <Details
                    summary="Download files"
                    className="govuk-!-margin-bottom-0 govuk-!-margin-top-2"
                  >
                    <ul className="govuk-list" data-testid="data-files">
                      {dataFiles.map(file => (
                        <li key={file.id}>{renderDownloadLink(file)}</li>
                      ))}
                    </ul>
                  </Details>
                )}
              </div>
              <div className="govuk-grid-column-one-quarter">
                {renderDataCatalogueLink}
              </div>
            </div>

            <SectionBreak />

            {release.hasDataGuidance && (
              <>
                <div
                  className={`govuk-grid-row dfe-flex dfe-align-items--center ${styles.section}`}
                >
                  <div className="govuk-grid-column-three-quarters">
                    <h3>Guidance</h3>
                    <p>
                      Learn more about the data files used in this release using
                      our online guidance
                    </p>
                  </div>
                  <div className="govuk-grid-column-one-quarter">
                    {renderDataGuidanceLink}
                  </div>
                </div>

                <SectionBreak />
              </>
            )}

            <div
              className={`govuk-grid-row dfe-flex dfe-align-items--center ${styles.section}`}
            >
              <div className="govuk-grid-column-three-quarters">
                <h3>Create your own tables</h3>
                <p>
                  You can view featured tables that we have built for you, or
                  create your own tables from the open data using our table tool
                </p>
              </div>
              <div className="govuk-grid-column-one-quarter">
                {renderCreateTablesButton}
              </div>
            </div>

            <SectionBreak visible={ancillaryFiles.length > 0} />

            {ancillaryFiles.length > 0 && (
              <>
                <h3>All supporting files</h3>
                <p>
                  All supporting files from this release are listed for
                  individual download below:
                </p>

                <Details summary="List of all supporting files">
                  <ul className="govuk-list" data-testid="other-files">
                    {ancillaryFiles.map(file => (
                      <li key={file.id}>
                        {renderDownloadLink(file)}

                        {file.summary && (
                          <Details
                            summary="More details"
                            className="govuk-!-margin-top-2"
                            hiddenText={` for file ${file.name}`}
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
        )}
        {renderRelatedDashboards && (
          <AccordionSection
            id="related-dashboards"
            heading="View related dashboard(s)"
          >
            {renderRelatedDashboards}
          </AccordionSection>
        )}
      </Accordion>
    </div>
  );
};

export default ReleaseDataAccordion;
