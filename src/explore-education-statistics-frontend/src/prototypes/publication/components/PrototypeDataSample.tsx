import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import PrototypeDataTilesHighlights
  from '@common/prototypes/publication/components/PrototypeDataTilesHighlights';
import PrototypeTableSample from '@common/prototypes/publication/components/PrototypeTableSample';
import React from 'react';
import PrototypeChartSample from './PrototypeChartSample';

interface Props {
  sectionId?: string;
  chartTitle?: string;
  xAxisLabel?: string;
  yAxisLabel?: string;
  chartData?: any;
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
      <Tabs>
        {sectionId === 'headlines' && (
          <TabsSection id={`${sectionId}SummaryData`} title="Summary">
            <PrototypeDataTilesHighlights />
            <ul className="govuk-list govuk-list--bullet">
              <li>pupils missed on average 8.2 school days</li>
              <li>
                overall and unauthorised absence rates up on previous year
              </li>
              <li>
                unauthorised rise due to higher rates of unauthorised holidays
              </li>
              <li>10% of pupils persistently absent during 2016/17</li>
            </ul>
          </TabsSection>
        )}
        <TabsSection id={`${sectionId}TableData`} title="Data tables">
          <PrototypeTableSample caption={`Table showing ${chartTitle}`} />
        </TabsSection>
        <TabsSection id={`${sectionId}ChartData`} title="Charts" lazy={true}>
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
