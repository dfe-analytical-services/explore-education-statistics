import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import FormattedDate from '@common/components/FormattedDate';
import ContentHtml from '@common/components/ContentHtml';
import ReleaseDataGuidanceDataFile from '@common/modules/release/components/ReleaseDataGuidanceDataFile';
import { DataSetDataGuidance } from '@common/services/releaseDataGuidanceService';
import React, { ReactNode } from 'react';

interface Props {
  published?: string;
  dataGuidance: string;
  renderDataCatalogueLink?: ReactNode;
  dataSets: DataSetDataGuidance[];
}

const ReleaseDataGuidancePageContent = ({
  published,
  dataGuidance,
  renderDataCatalogueLink,
  dataSets,
}: Props) => {
  return (
    <>
      {published && (
        <p className="govuk-!-margin-bottom-8" data-testid="published-date">
          <strong>
            Published <FormattedDate>{published}</FormattedDate>
          </strong>
        </p>
      )}

      {dataGuidance && (
        <ContentHtml html={dataGuidance} testId="dataGuidance-content" />
      )}

      {dataSets.length > 0 && (
        <>
          <h3 className="govuk-!-margin-top-6">Data files</h3>

          {renderDataCatalogueLink && (
            <p>
              All data files associated with this releases are listed below with
              guidance on their content. To download any of these files, please
              visit our {renderDataCatalogueLink}.
            </p>
          )}

          <Accordion id="dataFiles">
            {dataSets.map(dataSet => (
              <AccordionSection heading={dataSet.name} key={dataSet.fileId}>
                <ReleaseDataGuidanceDataFile
                  key={dataSet.fileId}
                  dataSet={dataSet}
                />
              </AccordionSection>
            ))}
          </Accordion>
        </>
      )}
    </>
  );
};

export default ReleaseDataGuidancePageContent;
