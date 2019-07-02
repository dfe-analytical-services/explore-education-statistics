import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import PrototypeDataTilesHighlights from '@common/prototypes/publication/components/PrototypeDataTilesHighlights';
import PrototypeTableSample from '@common/prototypes/publication/components/PrototypeTableSample';
import React from 'react';
import PrototypeChartSample from './PrototypeChartSample';

interface Props {
  sectionId?: string;
  chartTitle?: string;
  xAxisLabel?: string;
  yAxisLabel?: string;
  chartData?: object[];
  chartDataKeys: string[];
}

const PrototypeDataSample = ({
  sectionId,
  chartTitle,
  xAxisLabel,
  yAxisLabel,
  chartData,
  chartDataKeys,
}: Props) => {
  return (
    <>
      <Tabs id="data-sample">
        {sectionId === 'headlines' && (
          <TabsSection id={`${sectionId}SummaryData`} title="Summary">
            <PrototypeDataTilesHighlights />
            <ul className="govuk-list govuk-list--bullet">
              <li>fact 1 about apprenticeships starts and the levy</li>
              <li>fact 2 about apprenticeships starts and the levy</li>
              <li>fact 3 about apprenticeships starts and the levy</li>
              <li>fact 4 about apprenticeships starts and the levy</li>
            </ul>
          </TabsSection>
        )}
        <TabsSection id={`${sectionId}TableData`} title="Data tables">
          <PrototypeTableSample caption={`Table showing ${chartTitle}`} />
        </TabsSection>
        <TabsSection id={`${sectionId}ChartData`} title="Charts" lazy>
          <h2 className="govuk-heading-s">{`Chart showing ${chartTitle}`}</h2>
          <PrototypeChartSample
            xAxisLabel={xAxisLabel}
            yAxisLabel={yAxisLabel}
            chartData={chartData}
            chartDataKeys={chartDataKeys}
          />
        </TabsSection>
        <TabsSection id={`${sectionId}Downloads`} title="Download data">
          <p>
            You can customise and download data as Excel or .csv files. Our data
            can also be accessed via an API.
          </p>
          <ul className="govuk-list">
            <li>
              <a href="#" className="govuk-link">
                Download .csv files
              </a>
            </li>
            <li>
              <a href="#" className="govuk-link">
                Download Excel files
              </a>
            </li>
            <li>
              <a href="#" className="govuk-link">
                Download json files
              </a>
            </li>
            <li>
              <a href="#" className="govuk-link">
                Access API
              </a>{' '}
              -{' '}
              <a href="#" className="govuk-link">
                What is an API?
              </a>
            </li>
          </ul>
        </TabsSection>
      </Tabs>
    </>
  );
};

export default PrototypeDataSample;
