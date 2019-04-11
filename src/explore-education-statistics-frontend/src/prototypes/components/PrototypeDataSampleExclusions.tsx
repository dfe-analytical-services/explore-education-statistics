import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import React from 'react';
import PrototypeChartSample from './PrototypeChartSample';
import PrototypeDataTilesHighlights from './PrototypeDataTilesHighlightsExclusions';
import PrototypeTableSample from './PrototypeTableSample';

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
              <li>
                overall rate of permanent exclusions has increased from 0.08 per
                cent of pupil enrolments in 2015/16 to 0.10 per cent in 2016/17
              </li>
              <li>
                number of exclusions has also increased, from 6,685 to 7,720
              </li>
              <li>
                overall rate of fixed period exclusions increased, from 4.29 per
                cent of pupil enrolments in 2015/16 to 4.76 per cent in 2016/17
              </li>
              <li>
                number of exclusions has also increased, from 339,360 to
                381,865.
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
