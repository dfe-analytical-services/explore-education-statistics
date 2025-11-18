import AccordionSection from '@common/components/AccordionSection';
import Details from '@common/components/Details';
import Accordion from '@common/components/Accordion';
import ChevronCard from '@common/components/ChevronCard';
import ChevronGrid from '@common/components/ChevronGrid';
import styles from '@common/modules/release/components/ReleaseDataAndFiles.module.scss';
import { FileInfo } from '@common/services/types/file';
import orderBy from 'lodash/orderBy';
import React, { ReactNode } from 'react';

interface Props {
  downloadFiles: FileInfo[];
  hasDataGuidance: boolean;
  renderCreateTablesLink?: ReactNode;
  renderDataCatalogueLink?: ReactNode;
  renderAllFilesLink?: ReactNode;
  renderDataGuidanceLink: ReactNode;
  renderDownloadLink: (file: FileInfo) => ReactNode;
  renderRelatedDashboards?: ReactNode;
  showDownloadFilesList?: boolean;
  trackScroll?: boolean;
  onSectionOpen?: (accordionSection: { id: string; title: string }) => void;
}

const ReleaseDataAndFiles = ({
  downloadFiles,
  hasDataGuidance,
  renderCreateTablesLink,
  renderDataCatalogueLink,
  renderAllFilesLink,
  renderDataGuidanceLink,
  renderDownloadLink,
  renderRelatedDashboards,
  showDownloadFilesList = false,
  trackScroll = false,
  onSectionOpen,
}: Props) => {
  const dataFiles = orderBy(
    downloadFiles.filter(file => file.type === 'Data'),
    ['name'],
  );

  const ancillaryFiles = orderBy(
    downloadFiles.filter(
      file => file.type === 'Ancillary' && file.name !== 'All files',
    ),
    ['name'],
  );

  const hasAllFilesButton =
    (dataFiles.length > 0 || ancillaryFiles.length > 0) && renderAllFilesLink;

  return (
    <>
      <div
        id="data-and-files-section"
        data-scroll={trackScroll ? true : undefined}
      >
        <h2 className="govuk-heading-m" id="explore-data-and-files">
          Explore data and files used in this release
        </h2>

        <ChevronGrid testId="data-and-files">
          <ChevronCard
            description="View tables that we have built for you, or create your own tables from open data using our table tool"
            link={renderCreateTablesLink}
          />
          {downloadFiles && (
            <>
              <ChevronCard
                description="Browse and download open data files from this release in our data catalogue"
                descriptionAfter={
                  showDownloadFilesList &&
                  dataFiles.length > 0 && (
                    <Details
                      summary="Download files"
                      className="govuk-!-margin-bottom-0 govuk-!-margin-top-2 dfe-position--relative"
                    >
                      <ul className="govuk-list" data-testid="data-files">
                        {dataFiles.map(file => (
                          <li key={file.id}>{renderDownloadLink(file)}</li>
                        ))}
                      </ul>
                    </Details>
                  )
                }
                link={renderDataCatalogueLink}
              />

              {hasDataGuidance && (
                <ChevronCard
                  description="Learn more about the data files used in this release using our online guidance"
                  link={renderDataGuidanceLink}
                />
              )}

              {hasAllFilesButton && (
                <ChevronCard
                  description="Download all data available in this release as a compressed ZIP file"
                  link={renderAllFilesLink}
                  noChevron
                />
              )}
            </>
          )}
        </ChevronGrid>
      </div>

      {(ancillaryFiles.length > 0 || renderRelatedDashboards) && (
        <Accordion
          id="data-accordion"
          showOpenAll={false}
          onSectionOpen={onSectionOpen}
        >
          {ancillaryFiles.length > 0 && (
            <AccordionSection
              id="supporting-files"
              heading="Additional supporting files"
            >
              <div
                data-scroll={trackScroll ? true : undefined}
                id="supporting-files-inner"
              >
                <p>
                  All supporting files from this release are listed for
                  individual download below:
                </p>
                <ul className="govuk-list" data-testid="other-files">
                  {ancillaryFiles.map(file => (
                    <li
                      key={file.id}
                      className={`${styles.listItem} govuk-!-margin-bottom-4`}
                    >
                      <h3 className="govuk-heading-s govuk-!-margin-bottom-2">
                        {renderDownloadLink(file)}
                      </h3>
                      {file.summary && <p>{file.summary}</p>}
                    </li>
                  ))}
                </ul>
              </div>
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
      )}
    </>
  );
};

export default ReleaseDataAndFiles;
