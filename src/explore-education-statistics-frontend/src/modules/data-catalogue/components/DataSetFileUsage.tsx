import ButtonText from '@common/components/ButtonText';
import ChevronGrid from '@common/components/ChevronGrid';
import ChevronCard from '@common/components/ChevronCard';
import DataSetFilePageSection from '@frontend/modules/data-catalogue/components/DataSetFilePageSection';
import { pageBaseSections } from '@frontend/modules/data-catalogue/DataSetFilePage';
import Link from '@frontend/components/Link';
import React from 'react';

interface Props {
  hasApiDataSet?: boolean;
  tableToolLink: string;
  onDownload: () => void;
}

export default function DataSetFileUsage({
  hasApiDataSet = false,
  tableToolLink,
  onDownload,
}: Props) {
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
      </ChevronGrid>
    </DataSetFilePageSection>
  );
}
