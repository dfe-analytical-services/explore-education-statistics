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
  const downloadLink = `${process.env.CONTENT_API_BASE_URL}/data-set-files/${dataSetFileId}/download`;

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
          description="View tables that we have built for you, or create your own tables from open data using our table tool"
          link={<Link to={tableToolLink}>View or create your own tables</Link>}
        />
        {hasApiDataSet && (
          <ChevronCard
            cardSize="l"
            description="This data set is also available via an API, follow the link to get all the information to get started"
            link={
              <Link to={process.env.PUBLIC_API_DOCS_URL ?? ''}>
                API documentation
              </Link>
            }
          />
        )}
        <ChevronCard
          link={<>Download this data using code</>}
          cardSize="l"
          description="Access this data using common programming languages"
          descriptionAfter={
            <>
              <CopyTextButton
                className="govuk-!-margin-top-5"
                text={downloadLink}
                labelHidden={false}
              />
              <h4>Example code</h4>
              <Tabs id="dataSetUsage-code">
                <TabsSection title="Python" headingTag="h4">
                  <CodeBlock
                    language="python"
                    code={`import pandas as pd
pd.read_csv("${downloadLink}")`}
                  />
                </TabsSection>
                <TabsSection title="R" headingTag="h4">
                  <CodeBlock
                    language="r"
                    code={`read.csv("${downloadLink}")`}
                  />
                </TabsSection>
              </Tabs>
            </>
          }
          noChevron
        />
      </ChevronGrid>
    </DataSetFilePageSection>
  );
}
