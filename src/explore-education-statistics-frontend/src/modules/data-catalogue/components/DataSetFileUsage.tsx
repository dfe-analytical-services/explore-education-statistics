import ButtonText from '@common/components/ButtonText';
import ChevronCard from '@common/components/ChevronCard';
import ChevronGrid from '@common/components/ChevronGrid';
import CodeBlock from '@common/components/CodeBlock';
import CopyTextButton from '@common/components/CopyTextButton';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import React from 'react';
import Link from '@frontend/components/Link';
import DataSetFilePageSection from '@frontend/modules/data-catalogue/components/DataSetFilePageSection';
import { pageBaseSections } from '@frontend/modules/data-catalogue/DataSetFilePage';
import { DataSetFile } from '@frontend/services/dataSetFileService';

interface Props {
  dataSetFileId: DataSetFile['id'];
  hasApiDataSet?: boolean;
  tableToolLink: string;
  onDownload: () => void;
}

export default function DataSetFileUsage({
  dataSetFileId,
  hasApiDataSet = false,
  tableToolLink,
  onDownload,
}: Props) {
  const downloadUrl = new URL(
    `/data-catalogue/data-set/${dataSetFileId}/csv`,
    process.env.PUBLIC_URL,
  ).href;

  return (
    <DataSetFilePageSection
      heading={pageBaseSections.dataSetUsage}
      id="dataSetUsage"
    >
      <ChevronGrid>
        <ChevronCard
          cardSize="l"
          description="Download the underlying data as a compressed ZIP file"
          link={
            <ButtonText onClick={onDownload}>
              Download this data set (ZIP)
            </ButtonText>
          }
          noBorder
          noChevron
        />
        <ChevronCard
          cardSize="l"
          className={hasApiDataSet ? 'govuk-!-padding-bottom-0' : undefined}
          description="View tables that we have built for you, or create your own tables from open data using our table tool"
          link={<Link to={tableToolLink}>View or create your own tables</Link>}
        />
      </ChevronGrid>
      {!hasApiDataSet && (
        <>
          <h3>Download this data using code</h3>

          <p>
            Access this data using common programming languages using the URL
            below.
          </p>

          <CopyTextButton
            className="govuk-!-margin-top-5 govuk-!-margin-bottom-5"
            id="copy-download-url"
            text={downloadUrl}
            label="URL"
            labelHidden={false}
          />

          <h4>Example code</h4>

          <Tabs id="dataSetUsage-code">
            <TabsSection title="Python">
              <h5 className="govuk-heading-s">Python</h5>

              <CodeBlock language="python">
                {`import pandas as pd

pd.read_csv("${downloadUrl}")`}
              </CodeBlock>
            </TabsSection>
            <TabsSection title="R">
              <h5 className="govuk-heading-s">R</h5>

              <CodeBlock language="r">{`read.csv("${downloadUrl}")`}</CodeBlock>
            </TabsSection>
          </Tabs>
        </>
      )}
    </DataSetFilePageSection>
  );
}
