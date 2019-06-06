import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import PrototypeTableSample from '@common/prototypes/publication/components/PrototypeTableSample';
import React from 'react';
import PrototypeChartSample from './PrototypeChartSample';
import PrototypeDataTilesHighlights from './PrototypeDataTilesHighlightsExclusions';

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
      <Tabs>
        {sectionId === 'headlines' && (
          <TabsSection id={`${sectionId}SummaryData`} title="Summary">
            <PrototypeDataTilesHighlights />
            <ul className="govuk-list govuk-list--bullet">
              <li>
                overall permanent exclusions rate increased to 0.10% - up from
                0.08% in 2015/16
              </li>
              <li>
                number of exclusions increased to 7,720 - up from 6,685 in
                2015/16
              </li>
              <li>
                overall fixed-period exclusions rate increased to 4.76% - up
                from 4.29% in 2015/16
              </li>
              <li>
                number of exclusions increased to 381,865 - up from 339,360 in
                2015/16
              </li>
            </ul>
          </TabsSection>
        )}
        <TabsSection id={`${sectionId}ChartData`} title="Charts">
          <h2 className="govuk-heading-s">{`Chart showing ${chartTitle}`}</h2>
          <PrototypeChartSample
            xAxisLabel={xAxisLabel}
            yAxisLabel={yAxisLabel}
            chartData={chartData}
            chartDataKeys={chartDataKeys}
          />
        </TabsSection>
        <TabsSection id={`${sectionId}TableData`} title="Data tables">
          <PrototypeTableSample caption={`Table showing ${chartTitle}`} />
        </TabsSection>
        <TabsSection id={`${sectionId}Downloads`} title="Data downloads">
          <h2 className="govuk-heading-s">
            Download overall absence data files
          </h2>
          <ul className="govuk-list">
            <li>
              <a className="govuk-link" href="#">
                Excel table
              </a>
            </li>
            <li>
              <a className="govuk-link" href="#">
                csv
              </a>
            </li>
            <li>
              <a className="govuk-link" href="#">
                json
              </a>
            </li>
            <li>
              <a className="govuk-link" href="#">
                API
              </a>
            </li>
          </ul>
        </TabsSection>
      </Tabs>
    </>
  );
};

export default PrototypeDataSample;
